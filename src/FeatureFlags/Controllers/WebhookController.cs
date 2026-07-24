#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Text.Json;
using FeatureFlags.Exceptions;
using FeatureFlags.Integration;
using FeatureFlags.Models;
using FeatureFlags.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Controller for receiving and validating incoming webhook payloads from external systems.
/// Validates payload size before processing to prevent DoS attacks via excessive memory allocation.
/// </summary>
[ApiController]
[Route("api/webhooks")]
public class WebhookController : ControllerBase
{
    private const int MaxWebhookPayloadSizeBytes = 1024 * 1024; // 1 MB limit
    private readonly ILogger<WebhookController> _logger;
    private readonly IWebhookService _webhookService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookController"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="webhookService">The webhook service.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public WebhookController(
        ILogger<WebhookController> logger,
        IWebhookService webhookService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
    }

    /// <summary>
    /// Receives and validates an incoming webhook payload from an external system.
    /// Validates the payload size BEFORE reading the full body to prevent DoS via excessive memory allocation.
    /// </summary>
    /// <param name="request">The incoming webhook request.</param>
    /// <returns>200 OK if validation succeeds, 413 Payload Too Large if payload exceeds limit.</returns>
    /// <remarks>
    /// This endpoint accepts webhook payloads from external systems that need to notify this application
    /// of feature flag changes or other events. The payload is validated for:
    /// 1. Size limit (to prevent DoS attacks)
    /// 2. HMAC signature (if secret is configured for the webhook)
    /// 3. JSON structure
    /// </remarks>
    [HttpPost("receive/{webhookId}")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReceiveWebhook(
        int webhookId,
        CancellationToken cancellationToken = default)
    {
        // Check Content-Length header first (if provided)
        if (Request.ContentLength.HasValue)
        {
            if (Request.ContentLength > MaxWebhookPayloadSizeBytes)
            {
                _logger.LogWarning("Webhook payload too large: {ContentLength} bytes (max {MaxSize} bytes)",
                    Request.ContentLength, MaxWebhookPayloadSizeBytes);
                return StatusCode(StatusCodes.Status413PayloadTooLarge,
                    $"Webhook payload exceeds maximum size of {MaxWebhookPayloadSizeBytes} bytes");
            }
        }
        else
        {
            // If Content-Length is not provided, check Content-Type and buffer minimally
            // We'll validate by reading the stream with a size limit
            Request.EnableBuffering();
        }

        // Get the webhook configuration
        var webhook = await _webhookService.GetWebhookAsync(webhookId, cancellationToken);
        if (webhook is null)
        {
            _logger.LogWarning("Webhook not found: {WebhookId}", webhookId);
            return NotFound();
        }

        if (!webhook.IsActive)
        {
            _logger.LogWarning("Inactive webhook attempted to receive payload: {WebhookId}", webhookId);
            return StatusCode(StatusCodes.Status400BadRequest, "Webhook is inactive");
        }

        // Read the request body with size validation
        string payload;
        try
        {
            using var reader = new StreamReader(
                Request.Body,
                encoding: System.Text.Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            // Limit the read to prevent memory exhaustion
            var maxReadLength = Math.Min(MaxWebhookPayloadSizeBytes, Request.ContentLength ?? MaxWebhookPayloadSizeBytes);
            var buffer = new char[maxReadLength];
            var bytesRead = await reader.ReadBlockAsync(buffer, 0, buffer.Length);

            if (bytesRead >= MaxWebhookPayloadSizeBytes)
            {
                _logger.LogWarning("Webhook payload size limit exceeded during read: {BytesRead} bytes", bytesRead);
                return StatusCode(StatusCodes.Status413PayloadTooLarge,
                    $"Webhook payload exceeds maximum size of {MaxWebhookPayloadSizeBytes} bytes");
            }

            payload = new string(buffer, 0, bytesRead);

            // Reset the body stream position for any subsequent reads
            Request.Body.Position = 0;
        }
        catch (Exception ex) when (ex is not FeatureFlagException)
        {
            _logger.LogError(ex, "Failed to read webhook payload");
            return StatusCode(StatusCodes.Status400BadRequest, "Invalid payload format");
        }

        // Validate HMAC signature if secret is configured
        if (!string.IsNullOrEmpty(webhook.Secret))
        {
            if (!Request.Headers.TryGetValue("X-Hub-Signature-256", out var signatureHeader))
            {
                _logger.LogWarning("Webhook payload missing signature header: {WebhookId}", webhookId);
                return Unauthorized("Missing signature header");
            }

            var expectedSignature = signatureHeader.ToString();
            if (string.IsNullOrEmpty(expectedSignature) || !expectedSignature.StartsWith("sha256="))
            {
                _logger.LogWarning("Webhook payload has invalid signature format: {WebhookId}", webhookId);
                return Unauthorized("Invalid signature format");
            }

            var actualSignature = expectedSignature[7..]; // Remove "sha256=" prefix
            var computedSignature = HashingUtilities.ComputeHmacSha256(payload, webhook.Secret);

            if (!string.Equals(actualSignature, computedSignature, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Webhook payload signature validation failed: {WebhookId}", webhookId);
                return Unauthorized("Invalid signature");
            }

            _logger.LogDebug("Webhook payload signature validated successfully: {WebhookId}", webhookId);
        }

        // Parse and process the payload
        try
        {
            var webhookPayload = JsonSerializer.Deserialize<WebhookPayload>(payload);
            if (webhookPayload is null)
            {
                _logger.LogWarning("Failed to deserialize webhook payload: {WebhookId}", webhookId);
                return BadRequest("Invalid payload format");
            }

            // TODO: Process the webhook payload (e.g., update feature flags, trigger events)
            // For now, just acknowledge receipt
            _logger.LogInformation("Webhook received and validated: {WebhookId}, Event: {EventType}",
                webhookId, webhookPayload.EventType);

            return Ok(new { status = "received", eventType = webhookPayload.EventType });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse webhook payload JSON: {WebhookId}", webhookId);
            return BadRequest("Invalid JSON payload");
        }
        catch (Exception ex) when (ex is not FeatureFlagException)
        {
            _logger.LogError(ex, "Error processing webhook payload: {WebhookId}", webhookId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to process webhook");
        }
    }

    /// <summary>
    /// Gets the maximum allowed webhook payload size in bytes.
    /// </summary>
    /// <returns>The maximum size limit.</returns>
    public static int GetMaxPayloadSize() => MaxWebhookPayloadSizeBytes;
}
