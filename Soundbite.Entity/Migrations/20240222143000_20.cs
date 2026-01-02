using Microsoft.EntityFrameworkCore.Migrations;

namespace Soundbite.Entity.Migrations
{
    public partial class _20 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MergedToUserId",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_MergedToUserId",
                table: "Users",
                column: "MergedToUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_MergedToUserId",
                table: "Users",
                column: "MergedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_MergedToUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_MergedToUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MergedToUserId",
                table: "Users");
        }
    }
}
