using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Soundbite.Entity.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    UniversalId = table.Column<string>(maxLength: 36, nullable: true),
                    RefreshToken = table.Column<string>(maxLength: 500, nullable: true),
                    Email = table.Column<string>(maxLength: 320, nullable: false),
                    GivenName = table.Column<string>(maxLength: 64, nullable: true),
                    FamilyName = table.Column<string>(maxLength: 64, nullable: true),
                    Phone = table.Column<string>(maxLength: 64, nullable: true),
                    Title = table.Column<string>(maxLength: 64, nullable: true),
                    AllowEmail = table.Column<bool>(nullable: false),
                    AllowNews = table.Column<bool>(nullable: false),
                    AllowMarketing = table.Column<bool>(nullable: false),
                    AllowSms = table.Column<bool>(nullable: false),
                    InviteUtc = table.Column<DateTime>(nullable: true),
                    InviteAcceptUtc = table.Column<DateTime>(nullable: true),
                    UserRole = table.Column<int>(nullable: false),
                    ProviderType = table.Column<int>(nullable: false),
                    CalendarSettings = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.UniqueConstraint("AK_Users_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_Users_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    UniversalId = table.Column<string>(maxLength: 36, nullable: true),
                    Name = table.Column<string>(maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                    table.UniqueConstraint("AK_Tenants_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_Tenants_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(maxLength: 128, nullable: true),
                    Description = table.Column<string>(maxLength: 512, nullable: true),
                    UniversalId = table.Column<string>(maxLength: 36, nullable: false),
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.UniqueConstraint("AK_Organizations_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_Organizations_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organizations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(maxLength: 128, nullable: true),
                    Description = table.Column<string>(maxLength: 256, nullable: true),
                    OrganizationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                    table.UniqueConstraint("AK_Groups_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_Groups_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Groups_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "People",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    PersonRole = table.Column<int>(nullable: false),
                    InviteUtc = table.Column<DateTime>(nullable: true),
                    InviteAcceptUtc = table.Column<DateTime>(nullable: true),
                    UserId = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_People", x => x.Id);
                    table.UniqueConstraint("AK_People_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_People_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_People_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_People_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TokenSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: true),
                    OrganizationId = table.Column<int>(nullable: true),
                    Config = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokenSettings_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TokenSettings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TokenSettings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    MemberRole = table.Column<int>(nullable: false),
                    InviteUtc = table.Column<DateTime>(nullable: true),
                    InviteAcceptUtc = table.Column<DateTime>(nullable: true),
                    PersonId = table.Column<int>(nullable: false),
                    GroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.UniqueConstraint("AK_Members_Route", x => x.Route);
                    table.UniqueConstraint("AK_Members_PersonId_GroupId", x => new { x.PersonId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_Members_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Members_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Members_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    ParticipantRole = table.Column<int>(nullable: false),
                    GroupId = table.Column<int>(nullable: false),
                    SessionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantGroups", x => x.Id);
                    table.UniqueConstraint("AK_ParticipantGroups_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_ParticipantGroups_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipantGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Recurrence = table.Column<int>(nullable: false),
                    RecurrenceData = table.Column<string>(nullable: true),
                    OrganizationId = table.Column<int>(nullable: false),
                    TemplateId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.Id);
                    table.UniqueConstraint("AK_Series_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_Series_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Series_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Limit = table.Column<int>(nullable: false),
                    SessionType = table.Column<int>(nullable: false),
                    Reminder = table.Column<DateTime>(nullable: true),
                    Publish = table.Column<DateTime>(nullable: true),
                    ReminderSent = table.Column<DateTime>(nullable: true),
                    PublishSent = table.Column<DateTime>(nullable: true),
                    IsTemplate = table.Column<bool>(nullable: false),
                    OrganizationId = table.Column<int>(nullable: false),
                    SeriesId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.UniqueConstraint("AK_Sessions_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Sessions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Sessions_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    ParticipantRole = table.Column<int>(nullable: false),
                    ParticipantState = table.Column<int>(nullable: false),
                    PersonId = table.Column<int>(nullable: false),
                    SessionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participants", x => x.Id);
                    table.UniqueConstraint("AK_Participants_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_Participants_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Participants_People_PersonId",
                        column: x => x.PersonId,
                        principalTable: "People",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Participants_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prompts",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    Text = table.Column<string>(maxLength: 2048, nullable: false),
                    SessionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prompts", x => x.Id);
                    table.UniqueConstraint("AK_Prompts_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_Prompts_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prompts_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clips",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    UpdatedUtc = table.Column<DateTime>(nullable: false),
                    CreatedById = table.Column<int>(nullable: true),
                    Route = table.Column<string>(maxLength: 8, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: true),
                    ClipType = table.Column<int>(nullable: false),
                    FileType = table.Column<int>(nullable: false),
                    Length = table.Column<int>(nullable: false),
                    PromptId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clips", x => x.Id);
                    table.UniqueConstraint("AK_Clips_Route", x => x.Route);
                    table.ForeignKey(
                        name: "FK_Clips_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clips_Prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "Prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clips_CreatedById",
                table: "Clips",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Clips_PromptId",
                table: "Clips",
                column: "PromptId");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CreatedById",
                table: "Groups",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_OrganizationId",
                table: "Groups",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_CreatedById",
                table: "Members",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Members_GroupId",
                table: "Members",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CreatedById",
                table: "Organizations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_TenantId",
                table: "Organizations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_UniversalId",
                table: "Organizations",
                column: "UniversalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantGroups_CreatedById",
                table: "ParticipantGroups",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantGroups_GroupId",
                table: "ParticipantGroups",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantGroups_SessionId",
                table: "ParticipantGroups",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_CreatedById",
                table: "Participants",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_PersonId",
                table: "Participants",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_SessionId",
                table: "Participants",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_People_CreatedById",
                table: "People",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_People_OrganizationId",
                table: "People",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_People_UserId_OrganizationId",
                table: "People",
                columns: new[] { "UserId", "OrganizationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_CreatedById",
                table: "Prompts",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Prompts_SessionId",
                table: "Prompts",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Series_CreatedById",
                table: "Series",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Series_OrganizationId",
                table: "Series",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Series_TemplateId",
                table: "Series",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_CreatedById",
                table: "Sessions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_OrganizationId",
                table: "Sessions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SeriesId",
                table: "Sessions",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_CreatedById",
                table: "Tenants",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TokenSettings_CreatedById",
                table: "TokenSettings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TokenSettings_OrganizationId",
                table: "TokenSettings",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_TokenSettings_TenantId",
                table: "TokenSettings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedById",
                table: "Users",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_UniversalId",
                table: "Users",
                column: "UniversalId",
                unique: true,
                filter: "[UniversalId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ParticipantGroups_Sessions_SessionId",
                table: "ParticipantGroups",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Series_Sessions_TemplateId",
                table: "Series",
                column: "TemplateId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_Users_CreatedById",
                table: "Organizations");

            migrationBuilder.DropForeignKey(
                name: "FK_Series_Users_CreatedById",
                table: "Series");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_CreatedById",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Tenants_Users_CreatedById",
                table: "Tenants");

            migrationBuilder.DropForeignKey(
                name: "FK_Series_Organizations_OrganizationId",
                table: "Series");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Organizations_OrganizationId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Series_Sessions_TemplateId",
                table: "Series");

            migrationBuilder.DropTable(
                name: "Clips");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "ParticipantGroups");

            migrationBuilder.DropTable(
                name: "Participants");

            migrationBuilder.DropTable(
                name: "TokenSettings");

            migrationBuilder.DropTable(
                name: "Prompts");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "People");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "Tenants");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Series");
        }
    }
}
