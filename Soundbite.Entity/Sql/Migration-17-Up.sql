ALTER TABLE [Participants] ADD [ReactionType] int NOT NULL DEFAULT 0;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230730231643_17', N'3.1.19');

GO

