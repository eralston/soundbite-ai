IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [UniversalId] nvarchar(36) NULL,
    [RefreshToken] nvarchar(500) NULL,
    [Email] nvarchar(320) NOT NULL,
    [GivenName] nvarchar(64) NULL,
    [FamilyName] nvarchar(64) NULL,
    [Phone] nvarchar(64) NULL,
    [Title] nvarchar(64) NULL,
    [AllowEmail] bit NOT NULL,
    [AllowNews] bit NOT NULL,
    [AllowMarketing] bit NOT NULL,
    [AllowSms] bit NOT NULL,
    [InviteUtc] datetime2 NULL,
    [InviteAcceptUtc] datetime2 NULL,
    [UserRole] int NOT NULL,
    [ProviderType] int NOT NULL,
    [CalendarSettings] nvarchar(max) NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Users_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_Users_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Tenants] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [UniversalId] nvarchar(36) NULL,
    [Name] nvarchar(128) NULL,
    CONSTRAINT [PK_Tenants] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Tenants_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_Tenants_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Organizations] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [Name] nvarchar(128) NULL,
    [Description] nvarchar(512) NULL,
    [UniversalId] nvarchar(36) NOT NULL,
    [TenantId] int NOT NULL,
    CONSTRAINT [PK_Organizations] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Organizations_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_Organizations_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Organizations_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Groups] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [Name] nvarchar(128) NULL,
    [Description] nvarchar(256) NULL,
    [OrganizationId] int NOT NULL,
    CONSTRAINT [PK_Groups] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Groups_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_Groups_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Groups_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [People] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [PersonRole] int NOT NULL,
    [InviteUtc] datetime2 NULL,
    [InviteAcceptUtc] datetime2 NULL,
    [UserId] int NOT NULL,
    [OrganizationId] int NOT NULL,
    CONSTRAINT [PK_People] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_People_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_People_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_People_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_People_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [TokenSettings] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [TenantId] int NULL,
    [OrganizationId] int NULL,
    [Config] nvarchar(max) NULL,
    CONSTRAINT [PK_TokenSettings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TokenSettings_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TokenSettings_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TokenSettings_Tenants_TenantId] FOREIGN KEY ([TenantId]) REFERENCES [Tenants] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Members] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [MemberRole] int NOT NULL,
    [InviteUtc] datetime2 NULL,
    [InviteAcceptUtc] datetime2 NULL,
    [PersonId] int NOT NULL,
    [GroupId] int NOT NULL,
    CONSTRAINT [PK_Members] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Members_Route] UNIQUE ([Route]),
    CONSTRAINT [AK_Members_PersonId_GroupId] UNIQUE ([PersonId], [GroupId]),
    CONSTRAINT [FK_Members_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Members_Groups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [Groups] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Members_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ParticipantGroups] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [ParticipantRole] int NOT NULL,
    [GroupId] int NOT NULL,
    [SessionId] int NOT NULL,
    CONSTRAINT [PK_ParticipantGroups] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_ParticipantGroups_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_ParticipantGroups_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ParticipantGroups_Groups_GroupId] FOREIGN KEY ([GroupId]) REFERENCES [Groups] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Series] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [Name] nvarchar(128) NOT NULL,
    [Recurrence] int NOT NULL,
    [RecurrenceData] nvarchar(max) NULL,
    [OrganizationId] int NOT NULL,
    [TemplateId] int NOT NULL,
    CONSTRAINT [PK_Series] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Series_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_Series_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Series_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Sessions] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [Name] nvarchar(128) NOT NULL,
    [Limit] int NOT NULL,
    [SessionType] int NOT NULL,
    [Reminder] datetime2 NULL,
    [Publish] datetime2 NULL,
    [ReminderSent] datetime2 NULL,
    [PublishSent] datetime2 NULL,
    [IsTemplate] bit NOT NULL,
    [OrganizationId] int NOT NULL,
    [SeriesId] int NULL,
    CONSTRAINT [PK_Sessions] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Sessions_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_Sessions_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Sessions_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Sessions_Series_SeriesId] FOREIGN KEY ([SeriesId]) REFERENCES [Series] ([Id]) ON DELETE NO ACTION
);

GO

CREATE TABLE [Participants] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [ParticipantRole] int NOT NULL,
    [ParticipantState] int NOT NULL,
    [PersonId] int NOT NULL,
    [SessionId] int NOT NULL,
    CONSTRAINT [PK_Participants] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Participants_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_Participants_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Participants_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Participants_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Prompts] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [Text] nvarchar(2048) NOT NULL,
    [SessionId] int NOT NULL,
    CONSTRAINT [PK_Prompts] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Prompts_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_Prompts_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Prompts_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [Clips] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(8) NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [ClipType] int NOT NULL,
    [FileType] int NOT NULL,
    [Length] int NOT NULL,
    [PromptId] int NULL,
    CONSTRAINT [PK_Clips] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_Clips_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_Clips_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Clips_Prompts_PromptId] FOREIGN KEY ([PromptId]) REFERENCES [Prompts] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_Clips_CreatedById] ON [Clips] ([CreatedById]);

GO

CREATE INDEX [IX_Clips_PromptId] ON [Clips] ([PromptId]);

GO

CREATE INDEX [IX_Groups_CreatedById] ON [Groups] ([CreatedById]);

GO

CREATE INDEX [IX_Groups_OrganizationId] ON [Groups] ([OrganizationId]);

GO

CREATE INDEX [IX_Members_CreatedById] ON [Members] ([CreatedById]);

GO

CREATE INDEX [IX_Members_GroupId] ON [Members] ([GroupId]);

GO

CREATE INDEX [IX_Organizations_CreatedById] ON [Organizations] ([CreatedById]);

GO

CREATE INDEX [IX_Organizations_TenantId] ON [Organizations] ([TenantId]);

GO

CREATE UNIQUE INDEX [IX_Organizations_UniversalId] ON [Organizations] ([UniversalId]);

GO

CREATE INDEX [IX_ParticipantGroups_CreatedById] ON [ParticipantGroups] ([CreatedById]);

GO

CREATE INDEX [IX_ParticipantGroups_GroupId] ON [ParticipantGroups] ([GroupId]);

GO

CREATE INDEX [IX_ParticipantGroups_SessionId] ON [ParticipantGroups] ([SessionId]);

GO

CREATE INDEX [IX_Participants_CreatedById] ON [Participants] ([CreatedById]);

GO

CREATE INDEX [IX_Participants_PersonId] ON [Participants] ([PersonId]);

GO

CREATE INDEX [IX_Participants_SessionId] ON [Participants] ([SessionId]);

GO

CREATE INDEX [IX_People_CreatedById] ON [People] ([CreatedById]);

GO

CREATE INDEX [IX_People_OrganizationId] ON [People] ([OrganizationId]);

GO

CREATE UNIQUE INDEX [IX_People_UserId_OrganizationId] ON [People] ([UserId], [OrganizationId]);

GO

CREATE INDEX [IX_Prompts_CreatedById] ON [Prompts] ([CreatedById]);

GO

CREATE INDEX [IX_Prompts_SessionId] ON [Prompts] ([SessionId]);

GO

CREATE INDEX [IX_Series_CreatedById] ON [Series] ([CreatedById]);

GO

CREATE INDEX [IX_Series_OrganizationId] ON [Series] ([OrganizationId]);

GO

CREATE INDEX [IX_Series_TemplateId] ON [Series] ([TemplateId]);

GO

CREATE INDEX [IX_Sessions_CreatedById] ON [Sessions] ([CreatedById]);

GO

CREATE INDEX [IX_Sessions_OrganizationId] ON [Sessions] ([OrganizationId]);

GO

CREATE INDEX [IX_Sessions_SeriesId] ON [Sessions] ([SeriesId]);

GO

CREATE INDEX [IX_Tenants_CreatedById] ON [Tenants] ([CreatedById]);

GO

CREATE INDEX [IX_TokenSettings_CreatedById] ON [TokenSettings] ([CreatedById]);

GO

CREATE INDEX [IX_TokenSettings_OrganizationId] ON [TokenSettings] ([OrganizationId]);

GO

CREATE INDEX [IX_TokenSettings_TenantId] ON [TokenSettings] ([TenantId]);

GO

CREATE INDEX [IX_Users_CreatedById] ON [Users] ([CreatedById]);

GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

GO

CREATE UNIQUE INDEX [IX_Users_UniversalId] ON [Users] ([UniversalId]) WHERE [UniversalId] IS NOT NULL;

GO

ALTER TABLE [ParticipantGroups] ADD CONSTRAINT [FK_ParticipantGroups_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE NO ACTION;

GO

ALTER TABLE [Series] ADD CONSTRAINT [FK_Series_Sessions_TemplateId] FOREIGN KEY ([TemplateId]) REFERENCES [Sessions] ([Id]) ON DELETE NO ACTION;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210102170539_1', N'3.1.9');

GO

