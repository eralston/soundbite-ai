/****** Object:  Table [dbo].[OrganizationAuthProviders]    Script Date: 9/22/2023 1:20:30 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[OrganizationAuthProviders]') AND type in (N'U'))
DROP TABLE [dbo].[OrganizationAuthProviders]
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20230922170631_18';
GO