-- This is a simplified see scenario where you only want a single Soundbite organization
-- This is useful for when you do not want example content, such as when testing directory sync
-- This is most often used after resetting the database by running the Nuke.sql script (multiple times) and then running all migrations (update-database or manually)

-- BEFORE RUNNING THIS SCRIPT, LOGIN FOR THE FIRST TIME WITH THE USER YOU WANT TO BE THE ADMIN OF THE NEW SOUNDBITE ORG
-- This latches onto the User with user ID 1

GO 
-- WARNING: THIS INSERT TO USERS SHOULD NOT BE NECESSARY UNLESS NEW USER ONBOARDING IS BROKEN
SET IDENTITY_INSERT [dbo].[Users] ON
INSERT INTO [dbo].[Users] ([Id], [CreatedUtc], [UpdatedUtc], [CreatedById], [Route], [DeletedUtc], [UniversalId], [RefreshToken], [Email], [GivenName], [FamilyName], [Phone], [Title], [AllowEmail], [AllowNews], [AllowMarketing], [AllowSms], [InviteUtc], [InviteAcceptUtc], [UserRole], [ProviderType], [CalendarSettings]) VALUES (1, N'2021-10-14 13:04:14', N'2021-10-14 13:04:31', NULL, N'QwSmmKcfuP1Xfu2Er5Yk4', NULL, N'b2be56fb-0b86-44c6-b0ab-8a00e90c61be', N'UXdTbW1LY2Z1UDFYZnUyRXI1WWs0fDYzNzcyOTQxMzA0MzQxMTk5OXwzZDgyNDY5Yi02MDkyLTQ0ODItYTY2MC0wNDUwZGYxYjYwNTc=', N'erik@soundbite.ai', N'Erik', N'Ralston', N'509-312-9058', N'Co-Founder & Chief Technology Officer', 1, 1, 1, 1, N'2021-10-14 13:04:16', N'2021-10-14 13:04:16', 100, 1, NULL)
SET IDENTITY_INSERT [dbo].[Users] OFF


-- Create a new tenant, org, and person for the first user record

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Name', N'Route', N'UniversalId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Tenants]'))
    SET IDENTITY_INSERT [Tenants] ON;
INSERT INTO [Tenants] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [Name], [Route], [UniversalId], [UpdatedUtc])
VALUES (1, 1, '2020-08-28T14:31:33.3176337Z', NULL, N'Soundbite', N'IXtDxGJE', NULL, '2020-08-28T14:31:33.3176337Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Name', N'Route', N'UniversalId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Tenants]'))
    SET IDENTITY_INSERT [Tenants] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Description', N'Name', N'Route', N'TenantId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Organizations]'))
    SET IDENTITY_INSERT [Organizations] ON;
INSERT INTO [Organizations] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [Description], [Name], [Route], [TenantId], [UpdatedUtc], [UniversalId])
VALUES (1, 1, '2020-08-28T14:31:33.3183228Z', NULL, N'The finest widgets', N'Soundbite', N'SQ4yhWkP', 1, '2020-08-28T14:31:33.3183228Z', '[AMS TENANT ID]');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Description', N'Name', N'Route', N'TenantId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Organizations]'))
    SET IDENTITY_INSERT [Organizations] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'OrganizationId', N'PersonRole', N'Route', N'UpdatedUtc', N'UserId') AND [object_id] = OBJECT_ID(N'[People]'))
    SET IDENTITY_INSERT [People] ON;
INSERT INTO [People] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [OrganizationId], [PersonRole], [Route], [UpdatedUtc], [UserId])
VALUES (1, 1, '2020-08-28T14:31:33.3190512Z', NULL, 1, 100, N'lrUpvbN5', '2020-08-28T14:31:33.3190512Z', 1);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'OrganizationId', N'PersonRole', N'Route', N'UpdatedUtc', N'UserId') AND [object_id] = OBJECT_ID(N'[People]'))
    SET IDENTITY_INSERT [People] OFF;