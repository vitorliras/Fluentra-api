using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluentra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScenePracticeProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "scene_practice_progress",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    scene_id = table.Column<int>(type: "int", nullable: false),
                    accuracy_rate = table.Column<double>(type: "float", nullable: false),
                    passed = table.Column<bool>(type: "bit", nullable: false),
                    evaluation_json = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scene_practice_progress", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_scene_practice_progress_user_id_scene_id",
                table: "scene_practice_progress",
                columns: new[] { "user_id", "scene_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scene_practice_progress");
        }
    }
}
