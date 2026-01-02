ALTER TABLE [Sessions] ADD [SessionSecurity] int NOT NULL DEFAULT 0;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220506202639_9', N'3.1.19');
GO