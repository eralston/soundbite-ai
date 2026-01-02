ALTER TABLE [Organizations] ADD [ConfigJson] ntext NULL;

GO

ALTER TABLE [Clips] ADD [ClipSource] int NOT NULL DEFAULT 0;

GO

ALTER TABLE [Clips] ADD [ClipState] int NOT NULL DEFAULT 0;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211030234039_6', N'3.1.19');

GO

