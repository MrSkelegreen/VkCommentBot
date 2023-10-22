using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VkCommentBot.Migrations
{
    /// <inheritdoc />
    public partial class CommentImageAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "_timescaledb_catalog");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:timescaledb", ",,");

            migrationBuilder.CreateSequence(
                name: "chunk_constraint_name",
                schema: "_timescaledb_catalog");

            migrationBuilder.CreateTable(
                name: "VkPost",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    VkId = table.Column<long>(type: "bigint", nullable: false),
                    PostStatus = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValueSql: "'Выключен'::bpchar"),
                    KeyWord = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("post_primary_key", x => x.id);
                    table.UniqueConstraint("AK_VkPost_VkId", x => x.VkId);
                });

            migrationBuilder.CreateTable(
                name: "Scenario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    PostId = table.Column<long>(type: "bigint", nullable: true),
                    CommentImage = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("primary_key", x => x.id);
                    table.ForeignKey(
                        name: "foreign_postId",
                        column: x => x.PostId,
                        principalTable: "VkPost",
                        principalColumn: "VkId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scenario_PostId",
                table: "Scenario",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "unique_vkpost_id",
                table: "VkPost",
                column: "VkId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Scenario");

            migrationBuilder.DropTable(
                name: "VkPost");

            migrationBuilder.DropSequence(
                name: "chunk_constraint_name",
                schema: "_timescaledb_catalog");
        }
    }
}
