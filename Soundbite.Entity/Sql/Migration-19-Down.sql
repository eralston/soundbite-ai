DROP TABLE [SessionComments];

GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Sessions]') AND [c].[name] = N'SessionCommentPolicy');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Sessions] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [Sessions] DROP COLUMN [SessionCommentPolicy];

GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EnvSettings]') AND type in (N'U'))
DROP TABLE [dbo].[EnvSettings]
GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'StatusJson' AND Object_ID = Object_ID(N'dbo.SessionNotifications'))
BEGIN
    ALTER TABLE [dbo].[SessionNotifications] DROP COLUMN [StatusJson];
END
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20240101222258_19';

GO

