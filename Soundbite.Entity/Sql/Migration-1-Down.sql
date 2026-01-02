ALTER TABLE [Organizations] DROP CONSTRAINT [FK_Organizations_Users_CreatedById];

GO

ALTER TABLE [Series] DROP CONSTRAINT [FK_Series_Users_CreatedById];

GO

ALTER TABLE [Sessions] DROP CONSTRAINT [FK_Sessions_Users_CreatedById];

GO

ALTER TABLE [Tenants] DROP CONSTRAINT [FK_Tenants_Users_CreatedById];

GO

ALTER TABLE [Series] DROP CONSTRAINT [FK_Series_Organizations_OrganizationId];

GO

ALTER TABLE [Sessions] DROP CONSTRAINT [FK_Sessions_Organizations_OrganizationId];

GO

ALTER TABLE [Series] DROP CONSTRAINT [FK_Series_Sessions_TemplateId];

GO

DROP TABLE [Clips];

GO

DROP TABLE [Members];

GO

DROP TABLE [ParticipantGroups];

GO

DROP TABLE [Participants];

GO

DROP TABLE [TokenSettings];

GO

DROP TABLE [Prompts];

GO

DROP TABLE [Groups];

GO

DROP TABLE [People];

GO

DROP TABLE [Users];

GO

DROP TABLE [Organizations];

GO

DROP TABLE [Tenants];

GO

DROP TABLE [Sessions];

GO

DROP TABLE [Series];

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20210102170539_1';

GO

