ALTER TABLE [Users] ADD [MergedToUserId] int NULL;

GO

CREATE INDEX [IX_Users_MergedToUserId] ON [Users] ([MergedToUserId]);

GO

ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Users_MergedToUserId] FOREIGN KEY ([MergedToUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240222143000_20', N'3.1.19');

GO

