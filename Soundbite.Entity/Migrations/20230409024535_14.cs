using Microsoft.EntityFrameworkCore.Migrations;

namespace Soundbite.Entity.Migrations
{
    public partial class _14 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MediaProcessingState",
                table: "Clips",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaProcessingState",
                table: "Clips");
        }
    }
}
