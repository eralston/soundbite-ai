using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Soundbite.Entity.Migrations
{
    public partial class _18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationAuthProviders",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 21, nullable: false),
                    ProviderType = table.Column<int>(nullable: false),
                    ProviderUid = table.Column<string>(maxLength: 128, nullable: true),
                    ConfigJson = table.Column<string>(nullable: true),
                    OrganizationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationAuthProviders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationAuthProviders_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationAuthProviders_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAuthProviders_CreatedById",
                table: "OrganizationAuthProviders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationAuthProviders_OrganizationId",
                table: "OrganizationAuthProviders",
                column: "OrganizationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationAuthProviders");
        }
    }
}
