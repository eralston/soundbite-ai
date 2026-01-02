using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Soundbite.Entity.Migrations
{
    public partial class _5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClipEvents",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<int>(nullable: true),
                    EventType = table.Column<int>(nullable: false),
                    TargetId = table.Column<int>(nullable: false),
                    ClipEventType = table.Column<int>(nullable: false),
                    Duration = table.Column<int>(nullable: false),
                    Position = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClipEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClipEvents_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClipEvents_Clips_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Clips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClipEvents_CreatedById",
                table: "ClipEvents",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ClipEvents_TargetId",
                table: "ClipEvents",
                column: "TargetId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClipEvents");
        }
    }
}
