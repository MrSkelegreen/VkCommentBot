﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VkCommentBot;

#nullable disable

namespace VkCommentBot.Migrations
{
    [DbContext(typeof(DbContext))]
    [Migration("20230825053936_CommentImageAdded")]
    partial class CommentImageAdded
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "timescaledb");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.HasSequence("chunk_constraint_name", "_timescaledb_catalog");

            modelBuilder.Entity("VkCommentBot.Entities.Scenario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<byte[]>("CommentImage")
                        .HasColumnType("bytea");

                    b.Property<string>("Content")
                        .HasColumnType("text");

                    b.Property<long?>("PostId")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .HasColumnType("text");

                    b.HasKey("Id")
                        .HasName("primary_key");

                    b.HasIndex("PostId");

                    b.ToTable("Scenario", (string)null);
                });

            modelBuilder.Entity("VkCommentBot.Entities.VkPost", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn(b.Property<int>("Id"));

                    b.Property<string>("KeyWord")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PostStatus")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasDefaultValueSql("'Выключен'::bpchar");

                    b.Property<long?>("VkId")
                        .IsRequired()
                        .HasColumnType("bigint");

                    b.HasKey("Id")
                        .HasName("post_primary_key");

                    b.HasIndex(new[] { "VkId" }, "unique_vkpost_id")
                        .IsUnique();

                    b.ToTable("VkPost", (string)null);
                });

            modelBuilder.Entity("VkCommentBot.Entities.Scenario", b =>
                {
                    b.HasOne("VkCommentBot.Entities.VkPost", "Post")
                        .WithMany("Scenarios")
                        .HasForeignKey("PostId")
                        .HasPrincipalKey("VkId")
                        .HasConstraintName("foreign_postId");

                    b.Navigation("Post");
                });

            modelBuilder.Entity("VkCommentBot.Entities.VkPost", b =>
                {
                    b.Navigation("Scenarios");
                });
#pragma warning restore 612, 618
        }
    }
}
