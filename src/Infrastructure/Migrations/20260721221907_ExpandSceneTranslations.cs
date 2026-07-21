using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluentra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandSceneTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "translation",
                table: "scenes",
                newName: "translation_pt");

            migrationBuilder.AddColumn<string>(
                name: "translation_es",
                table: "scenes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "translation_fr",
                table: "scenes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "translation_es",
                table: "scenes");

            migrationBuilder.DropColumn(
                name: "translation_fr",
                table: "scenes");

            migrationBuilder.RenameColumn(
                name: "translation_pt",
                table: "scenes",
                newName: "translation");
        }
    }
}
