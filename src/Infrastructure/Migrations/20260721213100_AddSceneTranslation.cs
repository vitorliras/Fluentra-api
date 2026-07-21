using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fluentra.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSceneTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "translation",
                table: "scenes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "translation",
                table: "scenes");
        }
    }
}
