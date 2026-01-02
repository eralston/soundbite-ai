/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO

/*** Clips Table TranscriptState Column ***/
ALTER TABLE dbo.Clips
	DROP CONSTRAINT DF_Clips_TranscriptState /*DF__Clips__Transcrip__2022C2A6*/
GO
ALTER TABLE dbo.Clips
	DROP COLUMN TranscriptState
GO
ALTER TABLE dbo.Clips SET (LOCK_ESCALATION = TABLE)
GO

/*** Sessions Table Transcribe Column ***/
ALTER TABLE dbo.Sessions
	DROP CONSTRAINT DF_Sessions_Transcribe
GO
ALTER TABLE dbo.Sessions
	DROP COLUMN Transcribe
GO
ALTER TABLE dbo.Sessions SET (LOCK_ESCALATION = TABLE)
GO

/****** Object:  Table [dbo].[QueuedJobStatuses]    Script Date: 1/2/2023 1:24:28 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[QueuedJobStatuses]') AND type in (N'U'))
DROP TABLE [dbo].[QueuedJobStatuses]
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20230102182758_12';


COMMIT