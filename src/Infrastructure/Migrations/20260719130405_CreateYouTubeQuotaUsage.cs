using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluentra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateYouTubeQuotaUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "youtube_quota_usage",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    units_consumed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_youtube_quota_usage", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_youtube_quota_usage_date",
                table: "youtube_quota_usage",
                column: "date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "youtube_quota_usage");
        }
    }
}
