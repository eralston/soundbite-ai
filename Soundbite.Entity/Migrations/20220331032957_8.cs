using Microsoft.EntityFrameworkCore.Migrations;

namespace Soundbite.Entity.Migrations
{
    public partial class _8 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfigJson",
                table: "Users",
                type: "ntext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfigJson",
                table: "Users");
        }
    }
}
