-- Nuke and update-database
-- NOTE: This is handmade from the Nuke.sql and Migration-*.Up.sql scripts

/* Drop all non-system stored procs */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'P' AND category = 0 ORDER BY [name])

WHILE @name is not null
BEGIN
    SELECT @SQL = 'DROP PROCEDURE [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Procedure: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'P' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all views */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'V' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP VIEW [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped View: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'V' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all functions */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP FUNCTION [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Function: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] IN (N'FN', N'IF', N'TF', N'FS', N'FT') AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

/* Drop all Foreign Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME)

WHILE @name is not null
BEGIN
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint IS NOT NULL
    BEGIN
        SELECT @SQL = 'ALTER TABLE [dbo].[' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint) +']'
        EXEC (@SQL)
        PRINT 'Dropped FK Constraint: ' + @constraint + ' on ' + @name
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'FOREIGN KEY' ORDER BY TABLE_NAME)
END
GO

/* Drop all Primary Key constraints */
DECLARE @name VARCHAR(128)
DECLARE @constraint VARCHAR(254)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME)

WHILE @name IS NOT NULL
BEGIN
    SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    WHILE @constraint is not null
    BEGIN
        SELECT @SQL = 'ALTER TABLE [dbo].[' + RTRIM(@name) +'] DROP CONSTRAINT [' + RTRIM(@constraint)+']'
        EXEC (@SQL)
        PRINT 'Dropped PK Constraint: ' + @constraint + ' on ' + @name
        SELECT @constraint = (SELECT TOP 1 CONSTRAINT_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' AND CONSTRAINT_NAME <> @constraint AND TABLE_NAME = @name ORDER BY CONSTRAINT_NAME)
    END
SELECT @name = (SELECT TOP 1 TABLE_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE constraint_catalog=DB_NAME() AND CONSTRAINT_TYPE = 'PRIMARY KEY' ORDER BY TABLE_NAME)
END
GO

/* Drop all tables */
DECLARE @name VARCHAR(128)
DECLARE @SQL VARCHAR(254)

SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 ORDER BY [name])

WHILE @name IS NOT NULL
BEGIN
    SELECT @SQL = 'DROP TABLE [dbo].[' + RTRIM(@name) +']'
    EXEC (@SQL)
    PRINT 'Dropped Table: ' + @name
    SELECT @name = (SELECT TOP 1 [name] FROM sysobjects WHERE [type] = 'U' AND category = 0 AND [name] > @name ORDER BY [name])
END
GO

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

ALTER TABLE [Sessions] ADD [ReminderCalEventId] nvarchar(128) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210220170821_2', N'3.1.9');

GO

ALTER TABLE [Organizations] ADD [SyncConfigJson] ntext NULL;

GO

ALTER TABLE [Organizations] ADD [SyncType] nvarchar(25) NULL;

GO

ALTER TABLE [Groups] ADD [UniversalId] nvarchar(max) NULL;

GO

CREATE TABLE [SyncRuns] (
    [Id] int NOT NULL IDENTITY,
    [Route] nvarchar(8) NOT NULL,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [CreatedById] int NULL,
    [UniversalId] nvarchar(450) NOT NULL,
    [SyncConfigJson] ntext NULL,
    [SyncType] nvarchar(max) NULL,
    [Scope] int NOT NULL,
    [IsFailed] bit NOT NULL,
    [DeltaBytes] bigint NOT NULL,
    [DeltaTime] float NOT NULL,
    [ActionCount] int NOT NULL,
    [ResultJson] ntext NOT NULL,
    [ParentId] int NULL,
    [OrganizationId] int NOT NULL,
    CONSTRAINT [PK_SyncRuns] PRIMARY KEY ([Id]),
    CONSTRAINT [AK_SyncRuns_Route] UNIQUE ([Route]),
    CONSTRAINT [FK_SyncRuns_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SyncRuns_Organizations_OrganizationId] FOREIGN KEY ([OrganizationId]) REFERENCES [Organizations] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SyncRuns_SyncRuns_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [SyncRuns] ([Id]) ON DELETE NO ACTION
);

GO

CREATE INDEX [IX_SyncRuns_CreatedById] ON [SyncRuns] ([CreatedById]);

GO

CREATE INDEX [IX_SyncRuns_OrganizationId] ON [SyncRuns] ([OrganizationId]);

GO

CREATE INDEX [IX_SyncRuns_ParentId] ON [SyncRuns] ([ParentId]);

GO

CREATE UNIQUE INDEX [IX_SyncRuns_UniversalId] ON [SyncRuns] ([UniversalId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210915234413_3', N'3.1.19');

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'Length');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Clips] DROP COLUMN [Length];

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Route');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Users] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TokenSettings]') AND [c].[name] = N'Route');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [TokenSettings] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [TokenSettings] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Route');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Tenants] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SyncRuns]') AND [c].[name] = N'Route');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [SyncRuns] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [SyncRuns] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sessions]') AND [c].[name] = N'Route');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Sessions] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Sessions] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Series]') AND [c].[name] = N'Route');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Series] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Series] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Prompts]') AND [c].[name] = N'Route');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Prompts] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Prompts] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Route');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [People] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Participants]') AND [c].[name] = N'Route');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Participants] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Participants] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ParticipantGroups]') AND [c].[name] = N'Route');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ParticipantGroups] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [ParticipantGroups] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organizations]') AND [c].[name] = N'Route');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Organizations] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Organizations] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Members]') AND [c].[name] = N'Route');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Members] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Members] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Groups]') AND [c].[name] = N'Route');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Groups] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Groups] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'Route');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Clips] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

ALTER TABLE [Clips] ADD [BillingSeconds] int NOT NULL DEFAULT 0;

GO

ALTER TABLE [Clips] ADD [DisplaySeconds] int NOT NULL DEFAULT 0;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211007133755_4', N'3.1.19');

GO

CREATE TABLE [ClipEvents] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [CreatedById] int NULL,
    [EventType] int NOT NULL,
    [TargetId] int NOT NULL,
    [ClipEventType] int NOT NULL,
    [Duration] int NOT NULL,
    [Position] int NULL,
    CONSTRAINT [PK_ClipEvents] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClipEvents_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ClipEvents_Clips_TargetId] FOREIGN KEY ([TargetId]) REFERENCES [Clips] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_ClipEvents_CreatedById] ON [ClipEvents] ([CreatedById]);

GO

CREATE INDEX [IX_ClipEvents_TargetId] ON [ClipEvents] ([TargetId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211018135837_5', N'3.1.19');

GO

ALTER TABLE [Organizations] ADD [ConfigJson] ntext NULL;

GO

ALTER TABLE [Clips] ADD [ClipSource] int NOT NULL DEFAULT 0;

GO

ALTER TABLE [Clips] ADD [ClipState] int NOT NULL DEFAULT 0;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211030234039_6', N'3.1.19');

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Title');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Users] ALTER COLUMN [Title] nvarchar(max) NULL;

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Phone');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Users] ALTER COLUMN [Phone] nvarchar(max) NULL;

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'GivenName');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Users] ALTER COLUMN [GivenName] nvarchar(max) NULL;

GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'FamilyName');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Users] ALTER COLUMN [FamilyName] nvarchar(max) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211124143934_7', N'3.1.19');

GO

ALTER TABLE [Users] ADD [ConfigJson] ntext NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220331032957_8', N'3.1.19');

GO

ALTER TABLE [Sessions] ADD [SessionSecurity] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220506202639_9', N'3.1.19');
GO

CREATE TABLE [SessionNotifications] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [CreatedById] int NULL,
    [UserId] int NOT NULL,
    [SessionId] int NOT NULL,
    [NotificationType] int NOT NULL,
    [Channel] int NOT NULL,
    [Status] int NOT NULL,
    [Details] nvarchar(max) NULL,
    CONSTRAINT [PK_SessionNotifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SessionNotifications_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SessionNotifications_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SessionNotifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_SessionNotifications_CreatedById] ON [SessionNotifications] ([CreatedById]);

GO

CREATE INDEX [IX_SessionNotifications_SessionId] ON [SessionNotifications] ([SessionId]);

GO

CREATE INDEX [IX_SessionNotifications_UserId] ON [SessionNotifications] ([UserId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221010002926_10', N'3.1.19');

GO

ALTER TABLE [Participants] ADD [IsDirectParticipant] bit NOT NULL DEFAULT CAST(0 AS bit);

GO

CREATE TABLE [ParticipantGroupMembers] (
    [ParticipantId] int NOT NULL,
    [ParticipantGroupId] int NOT NULL,
    CONSTRAINT [PK_ParticipantGroupMembers] PRIMARY KEY ([ParticipantId], [ParticipantGroupId]),
    CONSTRAINT [FK_ParticipantGroupMembers_ParticipantGroups_ParticipantGroupId] FOREIGN KEY ([ParticipantGroupId]) REFERENCES [ParticipantGroups] ([Id]),
    CONSTRAINT [FK_ParticipantGroupMembers_Participants_ParticipantId] FOREIGN KEY ([ParticipantId]) REFERENCES [Participants] ([Id])
);

GO

CREATE INDEX [IX_ParticipantGroupMembers_ParticipantGroupId] ON [ParticipantGroupMembers] ([ParticipantGroupId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20221012172956_11', N'3.1.19');

GO

BEGIN TRANSACTION
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO

ALTER TABLE dbo.Clips ADD
	TranscriptState int NOT NULL CONSTRAINT DF_Clips_TranscriptState DEFAULT 0
GO

ALTER TABLE dbo.Clips SET (LOCK_ESCALATION = TABLE)
GO

ALTER TABLE dbo.Sessions ADD
	Transcribe bit NOT NULL CONSTRAINT DF_Sessions_Transcribe DEFAULT 0
GO

ALTER TABLE dbo.Sessions SET (LOCK_ESCALATION = TABLE)
GO

CREATE TABLE [dbo].[QueuedJobStatuses](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedUtc] [datetime2](7) NOT NULL,
	[UpdatedUtc] [datetime2](7) NOT NULL,
	[DeletedUtc] [datetime2](7) NULL,
	[CreatedById] [int] NULL,
	[Route] [nvarchar](21) NOT NULL,
	[OrgRoute] [nvarchar](21) NULL,
	[Description] [nvarchar](512) NULL,
	[RequestDate] [datetime2](7) NOT NULL,
	[ProcessingStarted] [datetime2](7) NULL,
	[ProcessingComplete] [datetime2](7) NULL,
	[JobType] [nvarchar](256) NULL,
	[QueueMessageId] [nvarchar](128) NULL,
	[QueueMessage] [nvarchar](max) NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[JobStatus] [int] NOT NULL,
	[Details] [nvarchar](max) NULL,
	[OrganizationId] [int] NULL,
 CONSTRAINT [PK_QueuedJobStatuses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[QueuedJobStatuses]  WITH CHECK ADD  CONSTRAINT [FK_QueuedJobStatuses_Organizations_OrganizationId] FOREIGN KEY([OrganizationId])
REFERENCES [dbo].[Organizations] ([Id])
GO

ALTER TABLE [dbo].[QueuedJobStatuses] CHECK CONSTRAINT [FK_QueuedJobStatuses_Organizations_OrganizationId]
GO

ALTER TABLE [dbo].[QueuedJobStatuses]  WITH CHECK ADD  CONSTRAINT [FK_QueuedJobStatuses_Users_CreatedById] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[QueuedJobStatuses] CHECK CONSTRAINT [FK_QueuedJobStatuses_Users_CreatedById]
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230102182758_12', N'3.1.19');

COMMIT

ALTER TABLE [Clips] ADD [MediaProcessingState] int NOT NULL DEFAULT 0;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230409024535_14', N'3.1.19');

GO