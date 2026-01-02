using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Soundbite.Entity.Migrations
{
    public partial class _3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SyncConfigJson",
                table: "Organizations",
                type: "ntext",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SyncType",
                table: "Organizations",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UniversalId",
                table: "Groups",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SyncRuns",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<int>(nullable: true),
                    UniversalId = table.Column<string>(nullable: false),
                    SyncConfigJson = table.Column<string>(type: "ntext", nullable: true),
                    SyncType = table.Column<string>(nullable: true),
                    Scope = table.Column<int>(nullable: false),
                    IsFailed = table.Column<bool>(nullable: false),
                    DeltaBytes = table.Column<long>(nullable: false),
                    DeltaTime = table.Column<double>(nullable: false),
                    ActionCount = table.Column<int>(nullable: false),
                    ResultJson = table.Column<string>(type: "ntext", nullable: false),
                    ParentId = table.Column<int>(nullable: true),
                    OrganizationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncRuns", x => x.Id);
                    table.UniqueConstraint("AK_SyncRuns_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_SyncRuns_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyncRuns_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SyncRuns_SyncRuns_ParentId",
                        column: x => x.ParentId,
                        principalTable: "SyncRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncRuns_CreatedById",
                table: "SyncRuns",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SyncRuns_OrganizationId",
                table: "SyncRuns",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncRuns_ParentId",
                table: "SyncRuns",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_SyncRuns_UniversalId",
                table: "SyncRuns",
                column: "UniversalId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncRuns");

            migrationBuilder.DropColumn(
                name: "SyncConfigJson",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "SyncType",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "UniversalId",
                table: "Groups");
        }
    }
}
