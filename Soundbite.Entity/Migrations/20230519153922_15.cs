using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Soundbite.Entity.Migrations
{
    public partial class _15 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaOperationsJson",
                table: "Clips",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClipOperations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 21, nullable: false),
                    OperationType = table.Column<int>(nullable: false),
                    OperationState = table.Column<int>(nullable: false),
                    OperationDataJson = table.Column<string>(type: "ntext", nullable: true),
                    ErrorDetails = table.Column<string>(nullable: true),
                    CompletedDate = table.Column<DateTime>(nullable: true),
                    BillingCode = table.Column<string>(nullable: true),
                    ExternalId = table.Column<string>(nullable: true),
                    ClipId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClipOperations", x => x.Id);
                    table.UniqueConstraint("AK_ClipOperations_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_ClipOperations_Clips_ClipId",
                        column: x => x.ClipId,
                        principalTable: "Clips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClipOperations_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClipOperations_ClipId",
                table: "ClipOperations",
                column: "ClipId");

            migrationBuilder.CreateIndex(
                name: "IX_ClipOperations_CreatedById",
                table: "ClipOperations",
                column: "CreatedById");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClipOperations");

            migrationBuilder.DropColumn(
                name: "MediaOperationsJson",
                table: "Clips");
        }
    }
}
