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

