DROP TABLE [ParticipantGroupMembers];

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Participants]') AND [c].[name] = N'IsDirectParticipant');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Participants] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Participants] DROP COLUMN [IsDirectParticipant];

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20221012172956_11';

GO

