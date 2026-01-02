ALTER TABLE [Sessions] ADD [ReminderCalEventId] nvarchar(128) NULL;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20210220170821_2', N'3.1.9');

GO

