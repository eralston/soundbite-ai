ALTER TABLE [Clips] ADD [MediaProcessingState] int NOT NULL DEFAULT 0;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230409024535_14', N'3.1.19');

GO

