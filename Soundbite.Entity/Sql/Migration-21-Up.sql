CREATE TABLE [UserIdentities] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(21) NOT NULL,
    [Identifier] nvarchar(450) NOT NULL,
    [ProviderType] int NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_UserIdentities] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserIdentities_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserIdentities_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_UserIdentities_CreatedById] ON [UserIdentities] ([CreatedById]);

GO

CREATE INDEX [IX_UserIdentities_UserId] ON [UserIdentities] ([UserId]);

GO

CREATE UNIQUE INDEX [IX_UserIdentities_Identifier_ProviderType] ON [UserIdentities] ([Identifier], [ProviderType]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241025160737_21', N'3.1.19');

GO

