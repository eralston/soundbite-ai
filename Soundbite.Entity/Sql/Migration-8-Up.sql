ALTER TABLE [Users] ADD [ConfigJson] ntext NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20220331032957_8', N'3.1.19');

GO

