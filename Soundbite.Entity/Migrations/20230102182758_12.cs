using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Soundbite.Entity.Migrations
{
    public partial class _12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Transcribe",
                table: "Sessions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TranscriptState",
                table: "Clips",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QueuedJobStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 21, nullable: false),
                    OrgRoute = table.Column<string>(maxLength: 21, nullable: true),
                    Description = table.Column<string>(maxLength: 512, nullable: true),
                    RequestDate = table.Column<DateTime>(nullable: false),
                    ProcessingStarted = table.Column<DateTime>(nullable: true),
                    ProcessingComplete = table.Column<DateTime>(nullable: true),
                    JobType = table.Column<string>(maxLength: 256, nullable: true),
                    QueueMessageId = table.Column<string>(maxLength: 128, nullable: true),
                    QueueMessage = table.Column<string>(nullable: true),
                    CorrelationId = table.Column<Guid>(nullable: false),
                    JobStatus = table.Column<int>(nullable: false),
                    Details = table.Column<string>(nullable: true),
                    OrganizationId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QueuedJobStatuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QueuedJobStatuses_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QueuedJobStatuses_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QueuedJobStatuses_CreatedById",
                table: "QueuedJobStatuses",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_QueuedJobStatuses_OrganizationId",
                table: "QueuedJobStatuses",
                column: "OrganizationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QueuedJobStatuses");

            migrationBuilder.DropColumn(
                name: "Transcribe",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "TranscriptState",
                table: "Clips");
        }
    }
}
