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

