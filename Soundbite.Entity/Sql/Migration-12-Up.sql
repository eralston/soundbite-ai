BEGIN TRANSACTION
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO

ALTER TABLE dbo.Clips ADD
	TranscriptState int NOT NULL CONSTRAINT DF_Clips_TranscriptState DEFAULT 0
GO

ALTER TABLE dbo.Clips SET (LOCK_ESCALATION = TABLE)
GO

ALTER TABLE dbo.Sessions ADD
	Transcribe bit NOT NULL CONSTRAINT DF_Sessions_Transcribe DEFAULT 0
GO

ALTER TABLE dbo.Sessions SET (LOCK_ESCALATION = TABLE)
GO

CREATE TABLE [dbo].[QueuedJobStatuses](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedUtc] [datetime2](7) NOT NULL,
	[UpdatedUtc] [datetime2](7) NOT NULL,
	[DeletedUtc] [datetime2](7) NULL,
	[CreatedById] [int] NULL,
	[Route] [nvarchar](21) NOT NULL,
	[OrgRoute] [nvarchar](21) NULL,
	[Description] [nvarchar](512) NULL,
	[RequestDate] [datetime2](7) NOT NULL,
	[ProcessingStarted] [datetime2](7) NULL,
	[ProcessingComplete] [datetime2](7) NULL,
	[JobType] [nvarchar](256) NULL,
	[QueueMessageId] [nvarchar](128) NULL,
	[QueueMessage] [nvarchar](max) NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[JobStatus] [int] NOT NULL,
	[Details] [nvarchar](max) NULL,
	[OrganizationId] [int] NULL,
 CONSTRAINT [PK_QueuedJobStatuses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[QueuedJobStatuses]  WITH CHECK ADD  CONSTRAINT [FK_QueuedJobStatuses_Organizations_OrganizationId] FOREIGN KEY([OrganizationId])
REFERENCES [dbo].[Organizations] ([Id])
GO

ALTER TABLE [dbo].[QueuedJobStatuses] CHECK CONSTRAINT [FK_QueuedJobStatuses_Organizations_OrganizationId]
GO

ALTER TABLE [dbo].[QueuedJobStatuses]  WITH CHECK ADD  CONSTRAINT [FK_QueuedJobStatuses_Users_CreatedById] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[QueuedJobStatuses] CHECK CONSTRAINT [FK_QueuedJobStatuses_Users_CreatedById]
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230102182758_12', N'3.1.19');

COMMIT