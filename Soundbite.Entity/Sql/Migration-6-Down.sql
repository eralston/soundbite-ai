DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organizations]') AND [c].[name] = N'ConfigJson');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Organizations] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Organizations] DROP COLUMN [ConfigJson];

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'ClipSource');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Clips] DROP COLUMN [ClipSource];

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'ClipState');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Clips] DROP COLUMN [ClipState];

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20211030234039_6';

GO

