#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Data;

/// <summary>
/// Database context for the feature flag engine.
/// Manages entity configurations, relationships, and database schema.
/// </summary>
{public sealed class FeatureFlagDbContext {
    public FeatureFlagDbContext(DbContextOptions<FeatureFlagDbContext> options)
        : base(options)
    {
    }

    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<Rule> Rules => Set<Rule>();
    public DbSet<Condition> Conditions => Set<Condition>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<RolloutStrategy> RolloutStrategies => Set<RolloutStrategy>();
    public DbSet<ABTestVariant> ABTestVariants => Set<ABTestVariant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // FeatureFlag configuration
        modelBuilder.Entity<FeatureFlag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().HasMaxLength(128);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.HasMany(e => e.Rules).WithOne(r => r.FeatureFlag).HasForeignKey(r => r.FeatureFlagId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Variants).WithOne(v => v.FeatureFlag).HasForeignKey(v => v.FeatureFlagId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.AuditLogs).WithOne(a => a.FeatureFlag).HasForeignKey(a => a.FeatureFlagId).OnDelete(DeleteBehavior.Cascade);
        });

        // Rule configuration
        modelBuilder.Entity<Rule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ConditionLogic).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => new { e.FeatureFlagId, e.Priority });
            entity.HasMany(e => e.Conditions).WithOne(c => c.Rule).HasForeignKey(c => c.RuleId).OnDelete(DeleteBehavior.Cascade);
        });

        // Condition configuration
        modelBuilder.Entity<Condition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AttributeName).IsRequired().HasMaxLength(128);
            entity.Property(e => e.ExpectedValue).IsRequired().HasMaxLength(500);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangedBy).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.OldValue).HasMaxLength(2000);
            entity.Property(e => e.NewValue).HasMaxLength(2000);
            entity.HasIndex(e => new { e.FeatureFlagId, e.ChangedAt });
            entity.HasIndex(e => e.ChangedAt);
        });

        // RolloutStrategy configuration
        modelBuilder.Entity<RolloutStrategy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartPercentage).IsRequired(false);
            entity.Property(e => e.EndPercentage).IsRequired(false);
            entity.HasIndex(e => e.FeatureFlagId);
        });

        // ABTestVariant configuration
        modelBuilder.Entity<ABTestVariant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VariantKey).IsRequired().HasMaxLength(128);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.HasIndex(e => new { e.FeatureFlagId, e.VariantKey }).IsUnique();
        });
    }
}
