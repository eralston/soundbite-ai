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