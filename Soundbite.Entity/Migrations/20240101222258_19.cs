using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Soundbite.Entity.Migrations
{
    public partial class _19 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionCommentPolicy",
                table: "Sessions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StatusJson",
                table: "SessionNotifications",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EnvSettings",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(maxLength: 2000, nullable: true),
                    GroupKey = table.Column<string>(maxLength: 255, nullable: true),
                    IsSecure = table.Column<bool>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvSettings", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SessionComments",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 21, nullable: false),
                    Content = table.Column<string>(maxLength: 512, nullable: false),
                    SessionId = table.Column<int>(nullable: false),
                    PersonId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionComments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionComments_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionComments_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnvSettings_GroupKey",
                table: "EnvSettings",
                column: "GroupKey");

            migrationBuilder.CreateIndex(
                name: "IX_SessionComments_CreatedById",
                table: "SessionComments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionComments_PersonId",
                table: "SessionComments",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionComments_SessionId",
                table: "SessionComments",
                column: "SessionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnvSettings");

            migrationBuilder.DropTable(
                name: "SessionComments");

            migrationBuilder.DropColumn(
                name: "SessionCommentPolicy",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "StatusJson",
                table: "SessionNotifications");
        }
    }
}
