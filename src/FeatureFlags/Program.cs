#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using FeatureFlags.Configuration;
using FeatureFlags.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure logging using Microsoft.Extensions.Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<FeatureFlagDbContext>(options =>
    options.UseSqlServer(connectionString));

// Validate feature flag options at startup (fails fast on misconfiguration)
builder.Services.AddFeatureFlags(options => builder.Configuration.GetSection("FeatureFlags").Bind(options));

// Register feature flag services
builder.Services.AddFeatureFlagServices(builder.Configuration);

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<FeatureFlagDbContext>();
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

try
{
    app.Logger.LogInformation("Starting Feature Flag Engine");
    await app.RunAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Application terminated unexpectedly");
    throw;
}
