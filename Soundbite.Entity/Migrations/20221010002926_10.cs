using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Soundbite.Entity.Migrations
{
    public partial class _10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<int>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    SessionId = table.Column<int>(nullable: false),
                    NotificationType = table.Column<int>(nullable: false),
                    Channel = table.Column<int>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    Details = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionNotifications_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionNotifications_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionNotifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionNotifications_CreatedById",
                table: "SessionNotifications",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionNotifications_SessionId",
                table: "SessionNotifications",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionNotifications_UserId",
                table: "SessionNotifications",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionNotifications");
        }
    }
}
