DROP INDEX [IX_ClipOperations_ExternalId] ON [ClipOperations];

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'HostingData');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Clips] DROP COLUMN [HostingData];

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'HostingType');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Clips] DROP COLUMN [HostingType];

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'MetaData');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Clips] DROP COLUMN [MetaData];

GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClipOperations]') AND [c].[name] = N'ExternalId');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [ClipOperations] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [ClipOperations] ALTER COLUMN [ExternalId] nvarchar(max) NULL;

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20230615181100_16';

GO

