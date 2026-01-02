DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sessions]') AND [c].[name] = N'SessionSecurity');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Sessions] DROP CONSTRAINT [' + @var0 + '];');

GO

ALTER TABLE [Sessions] DROP COLUMN [SessionSecurity];

GO

DELETE FROM [__EFMigrationsHistory] 
WHERE [MigrationId] = N'20220506202639_9';

GO