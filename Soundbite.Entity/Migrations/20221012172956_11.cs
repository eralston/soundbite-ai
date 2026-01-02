using Microsoft.EntityFrameworkCore.Migrations;

namespace Soundbite.Entity.Migrations
{
    public partial class _11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDirectParticipant",
                table: "Participants",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ParticipantGroupMembers",
                columns: table => new
                {
                    ParticipantId = table.Column<int>(nullable: false),
                    ParticipantGroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantGroupMembers", x => new { x.ParticipantId, x.ParticipantGroupId });
                    table.ForeignKey(
                        name: "FK_ParticipantGroupMembers_ParticipantGroups_ParticipantGroupId",
                        column: x => x.ParticipantGroupId,
                        principalTable: "ParticipantGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ParticipantGroupMembers_Participants_ParticipantId",
                        column: x => x.ParticipantId,
                        principalTable: "Participants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantGroupMembers_ParticipantGroupId",
                table: "ParticipantGroupMembers",
                column: "ParticipantGroupId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipantGroupMembers");

            migrationBuilder.DropColumn(
                name: "IsDirectParticipant",
                table: "Participants");
        }
    }
}
