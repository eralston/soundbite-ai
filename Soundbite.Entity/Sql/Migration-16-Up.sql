ALTER TABLE [Clips] ADD [HostingData] nvarchar(max) NULL;

GO

ALTER TABLE [Clips] ADD [HostingType] int NOT NULL DEFAULT 10;

GO

ALTER TABLE [Clips] ADD [MetaData] nvarchar(max) NULL;

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ClipOperations]') AND [c].[name] = N'ExternalId');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [ClipOperations] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [ClipOperations] ALTER COLUMN [ExternalId] nvarchar(450) NULL;

GO

CREATE INDEX [IX_ClipOperations_ExternalId] ON [ClipOperations] ([ExternalId]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230615181100_16', N'3.1.19');

GO

