DROP TABLE [ClipOperations];

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'MediaOperationsJson');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Clips] DROP COLUMN [MediaOperationsJson];

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20230519153922_15';

GO

