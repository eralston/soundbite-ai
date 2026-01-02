/****** Object:  Table [dbo].[OrganizationAuthProviders]    Script Date: 9/22/2023 1:19:39 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[OrganizationAuthProviders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreatedUtc] [datetime2](7) NOT NULL,
	[UpdatedUtc] [datetime2](7) NOT NULL,
	[DeletedUtc] [datetime2](7) NULL,
	[CreatedById] [int] NULL,
	[Route] [nvarchar](21) NOT NULL,
	[ProviderType] [int] NOT NULL,
	[ProviderUid] [nvarchar](128) NULL,
	[ConfigJson] [nvarchar](max) NULL,
	[OrganizationId] [int] NOT NULL,
 CONSTRAINT [PK_OrganizationAuthProviders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[OrganizationAuthProviders]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationAuthProviders_Organizations_OrganizationId] FOREIGN KEY([OrganizationId])
REFERENCES [dbo].[Organizations] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[OrganizationAuthProviders] CHECK CONSTRAINT [FK_OrganizationAuthProviders_Organizations_OrganizationId]
GO

ALTER TABLE [dbo].[OrganizationAuthProviders]  WITH CHECK ADD  CONSTRAINT [FK_OrganizationAuthProviders_Users_CreatedById] FOREIGN KEY([CreatedById])
REFERENCES [dbo].[Users] ([Id])
GO

ALTER TABLE [dbo].[OrganizationAuthProviders] CHECK CONSTRAINT [FK_OrganizationAuthProviders_Users_CreatedById]
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20230922170631_18', N'3.1.19');

INSERT INTO OrganizationAuthProviders (CreatedUtc, UpdatedUtc, CreatedById, Route, ProviderType, ProviderUid, ConfigJson, OrganizationId)
SELECT 
	GETUTCDATE(), -- Created
	GETUTCDATE(), -- Updated
	null, -- Created By
	(select substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)+ substring('ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789',(abs(checksum(newid())) % 62)+1, 1)), -- Route
	20, -- Auth Provider Type (20=Azure, 40=Okta)
	JSON_VALUE(CONVERT(nvarchar(max),ConfigJson), '$.azure.tenantId'),
	null, -- ConfigJSON
	Id -- Organization ID
	FROM Organizations
	WHERE ISJSON(CONVERT(nvarchar(max),ConfigJson)) > 0