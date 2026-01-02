DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'Length');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Clips] DROP COLUMN [Length];

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Route');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Users] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[TokenSettings]') AND [c].[name] = N'Route');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [TokenSettings] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [TokenSettings] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Tenants]') AND [c].[name] = N'Route');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Tenants] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [Tenants] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[SyncRuns]') AND [c].[name] = N'Route');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [SyncRuns] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [SyncRuns] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sessions]') AND [c].[name] = N'Route');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Sessions] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [Sessions] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Series]') AND [c].[name] = N'Route');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Series] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [Series] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Prompts]') AND [c].[name] = N'Route');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Prompts] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [Prompts] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[People]') AND [c].[name] = N'Route');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [People] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [People] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Participants]') AND [c].[name] = N'Route');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Participants] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [Participants] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ParticipantGroups]') AND [c].[name] = N'Route');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [ParticipantGroups] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [ParticipantGroups] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Organizations]') AND [c].[name] = N'Route');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [Organizations] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [Organizations] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Members]') AND [c].[name] = N'Route');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Members] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [Members] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Groups]') AND [c].[name] = N'Route');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [Groups] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [Groups] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Clips]') AND [c].[name] = N'Route');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Clips] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [Clips] ALTER COLUMN [Route] nvarchar(21) NOT NULL;

GO

ALTER TABLE [Clips] ADD [BillingSeconds] int NOT NULL DEFAULT 0;

GO

ALTER TABLE [Clips] ADD [DisplaySeconds] int NOT NULL DEFAULT 0;

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20211007133755_4', N'3.1.19');

GO

