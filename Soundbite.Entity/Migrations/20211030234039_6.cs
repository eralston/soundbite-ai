using Microsoft.EntityFrameworkCore.Migrations;

namespace Soundbite.Entity.Migrations
{
    public partial class _6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigJson",
                table: "Organizations",
                type: "ntext",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClipSource",
                table: "Clips",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClipState",
                table: "Clips",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigJson",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ClipSource",
                table: "Clips");

            migrationBuilder.DropColumn(
                name: "ClipState",
                table: "Clips");
        }
    }
}
