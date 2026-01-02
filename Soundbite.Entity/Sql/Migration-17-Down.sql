DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Participants]') AND [c].[name] = N'ReactionType');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Participants] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Participants] DROP COLUMN [ReactionType];

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20230730231643_17';

GO

