using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using VkCommentBot.Entities;

namespace VkCommentBot;

public partial class DbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public DbContext()
    {
    }

    public DbContext(DbContextOptions<DbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Scenario> Scenarios { get; set; }

    public virtual DbSet<VkPost> VkPosts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = ConfigurationManager.ConnectionStrings["Connection"].ConnectionString;
        optionsBuilder.UseNpgsql(connection);
    }
                  
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("timescaledb");

        modelBuilder.Entity<Scenario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("primary_key");

            entity.ToTable("Scenario");

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");

            entity.HasOne(d => d.Post).WithMany(p => p.Scenarios)
                .HasPrincipalKey(p => p.VkId)
                .HasForeignKey(d => d.PostId)
                .HasConstraintName("foreign_postId");
        });

        modelBuilder.Entity<VkPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("post_primary_key");

            entity.ToTable("VkPost");

            entity.HasIndex(e => e.VkId, "unique_vkpost_id").IsUnique();

            entity.Property(e => e.Id)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id");
            entity.Property(e => e.KeyWord).HasMaxLength(256);
            entity.Property(e => e.PostStatus)
                .HasMaxLength(10)
                .HasDefaultValueSql("'Выключен'::bpchar");
        });
        modelBuilder.HasSequence("chunk_constraint_name", "_timescaledb_catalog");

        OnModelCreatingPartial(modelBuilder);
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
