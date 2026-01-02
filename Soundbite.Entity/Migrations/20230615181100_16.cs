using Microsoft.EntityFrameworkCore.Migrations;

namespace Soundbite.Entity.Migrations
{
    public partial class _16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HostingData",
                table: "Clips",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HostingType",
                table: "Clips",
                nullable: false,
                defaultValue: (int)ClipHostingType.AzureStorage);

            migrationBuilder.AddColumn<string>(
                name: "MetaData",
                table: "Clips",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "ClipOperations",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClipOperations_ExternalId",
                table: "ClipOperations",
                column: "ExternalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClipOperations_ExternalId",
                table: "ClipOperations");

            migrationBuilder.DropColumn(
                name: "HostingData",
                table: "Clips");

            migrationBuilder.DropColumn(
                name: "HostingType",
                table: "Clips");

            migrationBuilder.DropColumn(
                name: "MetaData",
                table: "Clips");

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "ClipOperations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
