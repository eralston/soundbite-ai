ALTER TABLE [Sessions] ADD [SessionCommentPolicy] int NOT NULL DEFAULT 0;

GO

CREATE TABLE [SessionComments] (
    [Id] int NOT NULL IDENTITY,
    [CreatedUtc] datetime2 NOT NULL,
    [UpdatedUtc] datetime2 NOT NULL,
    [DeletedUtc] datetime2 NULL,
    [CreatedById] int NULL,
    [Route] nvarchar(21) NOT NULL,
    [Content] nvarchar(512) NOT NULL,
    [SessionId] int NOT NULL,
    [PersonId] int NOT NULL,
    CONSTRAINT [PK_SessionComments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SessionComments_Users_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SessionComments_People_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [People] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_SessionComments_Sessions_SessionId] FOREIGN KEY ([SessionId]) REFERENCES [Sessions] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_SessionComments_CreatedById] ON [SessionComments] ([CreatedById]);

GO

CREATE INDEX [IX_SessionComments_PersonId] ON [SessionComments] ([PersonId]);

GO

CREATE INDEX [IX_SessionComments_SessionId] ON [SessionComments] ([SessionId]);

GO

CREATE TABLE [dbo].[EnvSettings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NULL,
	[Description] [nvarchar](2000) NULL,
	[GroupKey] [nvarchar](255) NULL,
	[IsSecure] [bit] NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_EnvSettings] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'StatusJson' AND Object_ID = Object_ID(N'dbo.SessionNotifications'))
BEGIN
	ALTER TABLE dbo.SessionNotifications ADD StatusJson nvarchar(MAX) NULL
END
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240101222258_19', N'3.1.19');

GO