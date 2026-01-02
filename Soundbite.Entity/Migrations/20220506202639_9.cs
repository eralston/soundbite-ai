using Microsoft.EntityFrameworkCore.Migrations;

namespace Soundbite.Entity.Migrations
{
    public partial class _9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionSecurity",
                table: "Sessions",
                nullable: false,
                defaultValue: 100); // Default to SessionSecurityType.Protected
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionSecurity",
                table: "Sessions");
        }
    }
}
