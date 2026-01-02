-- This is a complete trio of organizations for use in localhost scenarios
-- It includes three orgs, test people under them, a even session content for getting started
-- Best to reset the database beforehand using Nuke.sql then re-run all migrations via update-database

-- BEFORE RUNNING THIS SCRIPT, LOGIN FOR THE FIRST TIME WITH THE USER YOU WANT TO BE THE ADMIN OF THE NEW SOUNDBITE ORG
-- This latches onto the User with user ID 1

GO

UPDATE [Users] set [UserRole] = 100 where [ID] = 1;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Name', N'Route', N'UniversalId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Tenants]'))
    SET IDENTITY_INSERT [Tenants] ON;
INSERT INTO [Tenants] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [Name], [Route], [UniversalId], [UpdatedUtc])
VALUES (1, 1, '2020-08-28T14:31:33.3176337Z', NULL, N'Soundbite', N'IXtDxGJE', NULL, '2020-08-28T14:31:33.3176337Z'),
(2, 1, '2020-08-28T14:31:33.3215167Z', NULL, N'Westeros', N'Oj9dQMd1', NULL, '2020-08-28T14:31:33.3215167Z'),
(3, 1, '2020-08-28T14:31:33.3291223Z', NULL, N'Invisible', N'DdPJhDDA', NULL, '2020-08-28T14:31:33.3291223Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Name', N'Route', N'UniversalId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Tenants]'))
    SET IDENTITY_INSERT [Tenants] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowMarketing', N'AllowNews', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Email', N'FamilyName', N'GivenName', N'InviteAcceptUtc', N'InviteUtc', N'Phone', N'Route', N'Title', N'UniversalId', N'UpdatedUtc', N'UserRole') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [AllowMarketing], [AllowNews], [AllowEmail], [AllowSms], [CreatedById], [CreatedUtc], [DeletedUtc], [Email], [FamilyName], [GivenName], [InviteAcceptUtc], [InviteUtc], [Phone], [Route], [Title], [UniversalId], [UpdatedUtc], [UserRole], [ProviderType])
VALUES (15, CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3230551Z', NULL, N'hodor@stark', N'or', N'Hod', '2020-08-28T14:31:33.3230562Z', '2020-08-28T14:31:33.3230561Z', N'123-456-7890', N'WFo8E5nU', N'Test User', N'81d7f2cce4c948fdad122ce014bffbcc', '2020-08-28T14:31:33.3230551Z', 50, 1),
(14, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3230453Z', NULL, N'benjen@stark', N'Stark', N'Benjen', '2020-08-28T14:31:33.3230467Z', '2020-08-28T14:31:33.3230465Z', N'123-456-7890', N'T1674hj1', N'Test User', N'd36150925b3a468cb63c797b6d3103e1', '2020-08-28T14:31:33.3230453Z', 50, 1),
(13, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit),1, '2020-08-28T14:31:33.3230235Z', NULL, N'rickon@stark.', N'Stark', N'Rickon', '2020-08-28T14:31:33.3230246Z', '2020-08-28T14:31:33.3230245Z', N'123-456-7890', N'uFR3cnwE', N'Test User', N'd088bc1c355f462cb2fa0dc017e7dcfb', '2020-08-28T14:31:33.3230235Z', 50, 1),
(12, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3230141Z', NULL, N'jon@snow', N'Snow', N'Jon', '2020-08-28T14:31:33.3230153Z', '2020-08-28T14:31:33.3230152Z', N'123-456-7890', N'8Ir1qjck', N'Test User', N'1075c51ab27c4f5f906569fb335578ea', '2020-08-28T14:31:33.3230141Z', 50, 1),
(11, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit),1, '2020-08-28T14:31:33.3230043Z', NULL, N'bran@stark', N'Stark', N'Bran', '2020-08-28T14:31:33.3230055Z', '2020-08-28T14:31:33.3230054Z', N'123-456-7890', N'9RhBfgqP', N'Test User', N'dcdcad7275d2460f8b6ff571e4f36494', '2020-08-28T14:31:33.3230043Z', 50, 1),
(10, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3229935Z', NULL, N'arya@stark', N'Stark', N'Arya', '2020-08-28T14:31:33.3229949Z', '2020-08-28T14:31:33.3229947Z', N'123-456-7890', N'LOhhvtkJ', N'Test User', N'5cec3a8eea13457cb4938b8830054366', '2020-08-28T14:31:33.3229935Z', 50, 1),
(9, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3229790Z', NULL, N'robb@stark', N'Stark', N'Robb', '2020-08-28T14:31:33.3229798Z', '2020-08-28T14:31:33.3229798Z', N'123-456-7890', N'DqJfnDbU', N'Test User', N'6ee70ba16f8c4c918a6834890db48b19', '2020-08-28T14:31:33.3229790Z', 50, 1),
(8, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3229705Z', NULL, N'sansa@stark', N'Stark', N'Sansa', '2020-08-28T14:31:33.3229713Z', '2020-08-28T14:31:33.3229712Z', N'123-456-7890', N'lAVm0y4z', N'Test User', N'5507c1dcf63d4e4faf9aafa3aa680f89', '2020-08-28T14:31:33.3229705Z', 50, 1),
(7, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3229640Z', NULL, N'catelyn@stark', N'Stark', N'Catelyn', '2020-08-28T14:31:33.3229650Z', '2020-08-28T14:31:33.3229649Z', N'123-456-7890', N'7M8FHGdE', N'Test User', N'3b7160cbe5934fde995a676b7382313a', '2020-08-28T14:31:33.3229640Z', 50, 1),
(6, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3229380Z', NULL, N'ned@stark', N'Stark', N'Ned', '2020-08-28T14:31:33.3229537Z', '2020-08-28T14:31:33.3229535Z', N'123-456-7890', N'rEVP7RUv', N'Test User', N'6d479eb925784c0ca28d30cadfb886ce', '2020-08-28T14:31:33.3229380Z', 50, 1),
(5, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3199705Z', NULL, N'ada@soundbite', N'Lovelace', N'Ada', '2020-08-28T14:31:33.3199717Z', '2020-08-28T14:31:33.3199715Z', N'123-456-7890', N'ENUQ6arb', N'Test User', N'8a71dd0705214668b28cb23af7b0fe9a', '2020-08-28T14:31:33.3199705Z', 50, 1),
(4, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3199601Z', NULL, N'alan@soundbite', N'Turing', N'Alan', '2020-08-28T14:31:33.3199616Z', '2020-08-28T14:31:33.3199615Z', N'123-456-7890', N'lymbJOWo', N'Test User', N'4a3a3a0e7f8f472f853d1c2e7f4f125c', '2020-08-28T14:31:33.3199601Z', 50, 1),
(3, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3199323Z', NULL, N'mike@soundbite', N'Lantzy', N'Mike', '2020-08-28T14:31:33.3199456Z', '2020-08-28T14:31:33.3199454Z', N'123-456-7890', N'DOfNNpFQ', N'Test User', N'3e9b6edd3c6d4da5be872afaf335d6f0', '2020-08-28T14:31:33.3199323Z', 50, 1),
(2, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3196758Z', NULL, N'margaret@soundbite', N'Hamilton', N'Margaret', '2020-08-28T14:31:33.3197261Z', '2020-08-28T14:31:33.3197244Z', N'123-456-7890', N'rv64QEeL', N'Test User', N'cfba7e6fe1a4459c992e3b86f241dceb', '2020-08-28T14:31:33.3196758Z', 50, 1),
(16, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3289208Z', NULL, N'cersei@lannister', N'Lannister', N'Cersei', '2020-08-28T14:31:33.3289253Z', '2020-08-28T14:31:33.3289250Z', N'123-456-7890', N'5TylgTBX', N'Test User', N'f4b4190049434f949a4dea5fd0464bab', '2020-08-28T14:31:33.3289208Z', 50, 1),
(17, CAST(1 AS bit), CAST(1 AS bit), CAST(0 AS bit), CAST(1 AS bit), 1, '2020-08-28T14:31:33.3291361Z', NULL, N'invisible@nowhere.org', N'Never See Me', N'Should', '2020-08-28T14:31:33.3291379Z', '2020-08-28T14:31:33.3291377Z', N'123-456-7890', N'1OGHRgTm', N'Test User', N'9d557613a9d143389c30c05ffb3f903f', '2020-08-28T14:31:33.3291361Z', 50, 1);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AllowMarketing', N'AllowNews', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Email', N'FamilyName', N'GivenName', N'InviteAcceptUtc', N'InviteUtc', N'Phone', N'Route', N'Title', N'UniversalId', N'UpdatedUtc', N'UserRole') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Description', N'Name', N'Route', N'TenantId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Organizations]'))
    SET IDENTITY_INSERT [Organizations] ON;
INSERT INTO [Organizations] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [Description], [Name], [Route], [TenantId], [UpdatedUtc], [UniversalId])
VALUES (1, 1, '2020-08-28T14:31:33.3183228Z', NULL, N'The finest widgets', N'Soundbite', N'SQ4yhWkP', 1, '2020-08-28T14:31:33.3183228Z', '1'),
(2, 1, '2020-08-28T14:31:33.3228930Z', NULL, N'Winter is Coming', N'House Stark', N'hk8hPNOW', 2, '2020-08-28T14:31:33.3228930Z', '2'),
(3, 1, '2020-08-28T14:31:33.3289130Z', NULL, N'Hear Me Roar', N'House Lannister', N'07eQxo3J', 2, '2020-08-28T14:31:33.3289130Z', '3'),
(4, 1, '2020-08-28T14:31:33.3291322Z', NULL, N'You should never see this org', N'Invisible', N'kbQRED95', 3, '2020-08-28T14:31:33.3291322Z', '4');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Description', N'Name', N'Route', N'TenantId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Organizations]'))
    SET IDENTITY_INSERT [Organizations] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'OrganizationId', N'PersonRole', N'Route', N'UpdatedUtc', N'UserId') AND [object_id] = OBJECT_ID(N'[People]'))
    SET IDENTITY_INSERT [People] ON;
INSERT INTO [People] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [OrganizationId], [PersonRole], [Route], [UpdatedUtc], [UserId])
VALUES (1, 1, '2020-08-28T14:31:33.3190512Z', NULL, 1, 100, N'lrUpvbN5', '2020-08-28T14:31:33.3190512Z', 1),
(17, 1, '2020-08-28T14:31:33.3289337Z', NULL, 3, 50, N'VP7yY5fz', '2020-08-28T14:31:33.3289337Z', 16),
(16, 1, '2020-08-28T14:31:33.3230601Z', NULL, 2, 50, N'nVCXzRMC', '2020-08-28T14:31:33.3230601Z', 15),
(15, 1, '2020-08-28T14:31:33.3230510Z', NULL, 2, 50, N'P7wfTLtr', '2020-08-28T14:31:33.3230510Z', 14),
(14, 1, '2020-08-28T14:31:33.3230284Z', NULL, 2, 50, N'yCSuIMfF', '2020-08-28T14:31:33.3230284Z', 13),
(13, 1, '2020-08-28T14:31:33.3230194Z', NULL, 2, 50, N'eaxOxgpp', '2020-08-28T14:31:33.3230194Z', 12),
(12, 1, '2020-08-28T14:31:33.3230097Z', NULL, 2, 50, N'ybUHDnXp', '2020-08-28T14:31:33.3230097Z', 11),
(18, 1, '2020-08-28T14:31:33.3291421Z', NULL, 4, 50, N'Bt18g3ed', '2020-08-28T14:31:33.3291421Z', 17),
(10, 1, '2020-08-28T14:31:33.3229826Z', NULL, 2, 50, N'Bs4RLlOB', '2020-08-28T14:31:33.3229826Z', 9),
(9, 1, '2020-08-28T14:31:33.3229753Z', NULL, 2, 50, N'zMa2Fxqy', '2020-08-28T14:31:33.3229753Z', 8),
(11, 1, '2020-08-28T14:31:33.3229999Z', NULL, 2, 50, N'SIg4Fm8y', '2020-08-28T14:31:33.3229999Z', 10),
(7, 1, '2020-08-28T14:31:33.3229607Z', NULL, 2, 50, N'hWjrHhEa', '2020-08-28T14:31:33.3229607Z', 6),
(6, 1, '2020-08-28T14:31:33.3229300Z', NULL, 2, 100, N'G8qGRk9J', '2020-08-28T14:31:33.3229300Z', 1),
(2, 1, '2020-08-28T14:31:33.3199048Z', NULL, 1, 50, N'OqDaOqVh', '2020-08-28T14:31:33.3199048Z', 2),
(8, 1, '2020-08-28T14:31:33.3229677Z', NULL, 2, 50, N'VpgLQPmD', '2020-08-28T14:31:33.3229677Z', 7),
(3, 1, '2020-08-28T14:31:33.3199547Z', NULL, 1, 50, N'XfdIBE5H', '2020-08-28T14:31:33.3199547Z', 3),
(5, 1, '2020-08-28T14:31:33.3199771Z', NULL, 1, 50, N'YpgKBFkl', '2020-08-28T14:31:33.3199771Z', 5),
(4, 1, '2020-08-28T14:31:33.3199663Z', NULL, 1, 50, N'LsT1uka4', '2020-08-28T14:31:33.3199663Z', 4);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'OrganizationId', N'PersonRole', N'Route', N'UpdatedUtc', N'UserId') AND [object_id] = OBJECT_ID(N'[People]'))
    SET IDENTITY_INSERT [People] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'Publish', N'DeletedUtc', N'Limit', N'Name', N'OrganizationId', N'Reminder', N'Route', N'SeriesId', N'SessionType', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Sessions]'))
    SET IDENTITY_INSERT [Sessions] ON;
INSERT INTO [Sessions] ([Id], [CreatedById], [CreatedUtc], [Publish], [DeletedUtc], [Limit], [Name], [OrganizationId], [Reminder], [Route], [SeriesId], [SessionType], [UpdatedUtc], [IsTemplate])
VALUES (1, NULL, '2020-08-28T14:31:33.3250987Z', '2020-08-28T14:31:33.3232028Z', NULL, 180, N'Throne Room Meeting', 2, '2020-08-28T14:31:33.3232026Z', N'myTfe7eY', NULL, 2, '2020-08-28T14:31:33.3250987Z', CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'Publish', N'DeletedUtc', N'Limit', N'Name', N'OrganizationId', N'Reminder', N'Route', N'SeriesId', N'SessionType', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Sessions]'))
    SET IDENTITY_INSERT [Sessions] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Description', N'Name', N'OrganizationId', N'Route', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Groups]'))
    SET IDENTITY_INSERT [Groups] ON;
INSERT INTO [Groups] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [Description], [Name], [OrganizationId], [Route], [UpdatedUtc])
VALUES (2, 1, '2020-08-28T14:31:33.3212839Z', NULL, N'Seeded Team', N'Blue Team', 1, N'gJGxtT1T', '2020-08-28T14:31:33.3212839Z'),
(4, 1, '2020-08-28T14:31:33.3230646Z', NULL, N'Seeded Team', N'Mr. & Mrs.', 2, N'514Gksum', '2020-08-28T14:31:33.3230646Z'),
(5, 1, '2020-08-28T14:31:33.3230936Z', NULL, N'Seeded Team', N'Leaders', 2, N'v4l2QVgE', '2020-08-28T14:31:33.3230936Z'),
(6, 1, '2020-08-28T14:31:33.3231400Z', NULL, N'Seeded Team', N'Undead', 2, N'vc2xExhG', '2020-08-28T14:31:33.3231400Z'),
(7, 1, '2020-08-28T14:31:33.3231655Z', NULL, N'Seeded Team', N'Heirs', 2, N'IsMhRPuC', '2020-08-28T14:31:33.3231655Z'),
(3, 1, '2020-08-28T14:31:33.3213044Z', NULL, N'Seeded Team', N'Purple Team', 1, N'e5p5h9uv', '2020-08-28T14:31:33.3213044Z'),
(8, 1, '2020-08-28T14:31:33.3289558Z', NULL, N'Seeded Team', N'Raging Psychopaths', 3, N'l0GWYTtf', '2020-08-28T14:31:33.3289558Z'),
(1, 1, '2020-08-28T14:31:33.3204494Z', NULL, N'Seeded Team', N'Red Team', 1, N'u61rdTHY', '2020-08-28T14:31:33.3204494Z'),
(9, 1, '2020-08-28T14:31:33.3291457Z', NULL, N'Seeded Team', N'Team Invisible', 4, N'3p4VqGrP', '2020-08-28T14:31:33.3291457Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Description', N'Name', N'OrganizationId', N'Route', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Groups]'))
    SET IDENTITY_INSERT [Groups] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'MemberRole', N'PersonId', N'Route', N'TeamId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Members]'))
    SET IDENTITY_INSERT [Members] ON;
INSERT INTO [Members] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [MemberRole], [PersonId], [Route], [GroupId], [UpdatedUtc])
VALUES (1, 1, '2020-08-28T14:31:33.3209770Z', NULL, 100, 1, N'21bwENxN', 1, '2020-08-28T14:31:33.3209770Z'),
(26, 1, '2020-08-28T14:31:33.3231987Z', NULL, 50, 14, N'0AbYXztr', 7, '2020-08-28T14:31:33.3231987Z'),
(25, 1, '2020-08-28T14:31:33.3231894Z', NULL, 50, 13, N'pt7x1dte', 7, '2020-08-28T14:31:33.3231894Z'),
(24, 1, '2020-08-28T14:31:33.3231857Z', NULL, 50, 12, N'bKNJzo49', 7, '2020-08-28T14:31:33.3231857Z'),
(23, 1, '2020-08-28T14:31:33.3231818Z', NULL, 50, 11, N'98By0Odk', 7, '2020-08-28T14:31:33.3231818Z'),
(22, 1, '2020-08-28T14:31:33.3231777Z', NULL, 50, 10, N'f324jQlu', 7, '2020-08-28T14:31:33.3231777Z'),
(21, 1, '2020-08-28T14:31:33.3231738Z', NULL, 50, 9, N'D9fVYECn', 7, '2020-08-28T14:31:33.3231738Z'),
(20, 1, '2020-08-28T14:31:33.3231699Z', NULL, 100, 6, N'7vRSeFv5', 7, '2020-08-28T14:31:33.3231699Z'),
(19, 1, '2020-08-28T14:31:33.3231574Z', NULL, 50, 15, N'FKoBx7Yj', 6, '2020-08-28T14:31:33.3231574Z'),
(18, 1, '2020-08-28T14:31:33.3231483Z', NULL, 50, 8, N'1vcL4h78', 6, '2020-08-28T14:31:33.3231483Z'),
(17, 1, '2020-08-28T14:31:33.3231442Z', NULL, 100, 6, N'rXhaPDVU', 6, '2020-08-28T14:31:33.3231442Z'),
(16, 1, '2020-08-28T14:31:33.3231360Z', NULL, 50, 15, N'zqU7g2fw', 5, '2020-08-28T14:31:33.3231360Z'),
(15, 1, '2020-08-28T14:31:33.3231323Z', NULL, 50, 8, N'0f29HmGb', 5, '2020-08-28T14:31:33.3231323Z'),
(14, 1, '2020-08-28T14:31:33.3231284Z', NULL, 50, 7, N'bOO7kJcX', 5, '2020-08-28T14:31:33.3231284Z'),
(13, 1, '2020-08-28T14:31:33.3231240Z', NULL, 100, 6, N'kbLNQh46', 5, '2020-08-28T14:31:33.3231240Z'),
(12, 1, '2020-08-28T14:31:33.3230837Z', NULL, 50, 8, N'NkTjPvB7', 4, '2020-08-28T14:31:33.3230837Z'),
(11, 1, '2020-08-28T14:31:33.3230797Z', NULL, 50, 7, N'jBHczgbY', 4, '2020-08-28T14:31:33.3230797Z'),
(10, 1, '2020-08-28T14:31:33.3230710Z', NULL, 100, 6, N'w1of1rUW', 4, '2020-08-28T14:31:33.3230710Z'),
(2, 1, '2020-08-28T14:31:33.3212531Z', NULL, 50, 2, N'fldXSpLl', 1, '2020-08-28T14:31:33.3212531Z'),
(3, 1, '2020-08-28T14:31:33.3212780Z', NULL, 50, 3, N'glmvImWf', 1, '2020-08-28T14:31:33.3212780Z'),
(4, 1, '2020-08-28T14:31:33.3212958Z', NULL, 100, 1, N'f5Ye85Hm', 2, '2020-08-28T14:31:33.3212958Z'),
(5, 1, '2020-08-28T14:31:33.3212987Z', NULL, 50, 3, N'1CFrBxBf', 2, '2020-08-28T14:31:33.3212987Z'),
(6, 1, '2020-08-28T14:31:33.3213017Z', NULL, 50, 4, N'vQl3HmrP', 2, '2020-08-28T14:31:33.3213017Z'),
(7, 1, '2020-08-28T14:31:33.3213074Z', NULL, 100, 1, N'tuRWAHkm', 3, '2020-08-28T14:31:33.3213074Z'),
(27, 1, '2020-08-28T14:31:33.3289620Z', NULL, 100, 17, N'sjMISRVS', 8, '2020-08-28T14:31:33.3289620Z'),
(8, 1, '2020-08-28T14:31:33.3213145Z', NULL, 50, 4, N'MaEfPFOT', 3, '2020-08-28T14:31:33.3213145Z'),
(9, 1, '2020-08-28T14:31:33.3213172Z', NULL, 50, 5, N'jBcMJ7n4', 3, '2020-08-28T14:31:33.3213172Z'),
(28, 1, '2020-08-28T14:31:33.3291570Z', NULL, 100, 18, N'xgMXcLLY', 9, '2020-08-28T14:31:33.3291570Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'MemberRole', N'PersonId', N'Route', N'TeamId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Members]'))
    SET IDENTITY_INSERT [Members] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'ParticipantRole', N'ParticipantState', N'PersonId', N'Route', N'SessionId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Participants]'))
    SET IDENTITY_INSERT [Participants] ON;
INSERT INTO [Participants] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [ParticipantRole], [ParticipantState], [PersonId], [Route], [SessionId], [UpdatedUtc])
VALUES (1, NULL, '2020-08-28T14:31:33.3264720Z', NULL, 100, 0, 7, N'65Tp03X2', 1, '2020-08-28T14:31:33.3264720Z'),
(2, NULL, '2020-08-28T14:31:33.3267174Z', NULL, 100, 0, 8, N'82O0TVC6', 1, '2020-08-28T14:31:33.3267174Z'),
(3, NULL, '2020-08-28T14:31:33.3267312Z', NULL, 50, 0, 9, N'OxIAMoxL', 1, '2020-08-28T14:31:33.3267312Z'),
(4, NULL, '2020-08-28T14:31:33.3267343Z', NULL, 25, 0, 16, N'rxxaDX5s', 1, '2020-08-28T14:31:33.3267343Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'ParticipantRole', N'ParticipantState', N'PersonId', N'Route', N'SessionId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Participants]'))
    SET IDENTITY_INSERT [Participants] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Route', N'SessionId', N'Text', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Prompts]'))
    SET IDENTITY_INSERT [Prompts] ON;
INSERT INTO [Prompts] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [Route], [SessionId], [Text], [UpdatedUtc])
VALUES (1, NULL, '2020-08-28T14:31:33.3271150Z', NULL, N'ohB6teo6', 1, N'The only time a man can be brave is when he is afraid. Any questions?', '2020-08-28T14:31:33.3271150Z');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Route', N'SessionId', N'Text', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Prompts]'))
    SET IDENTITY_INSERT [Prompts] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Name', N'Recurrence', N'RecurrenceData', N'Route', N'TemplateId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Series]'))
    SET IDENTITY_INSERT [Series] ON;
INSERT INTO [Series] ([Id], [CreatedById], [CreatedUtc], [DeletedUtc], [Name], [Recurrence], [RecurrenceData], [Route], [TemplateId], [UpdatedUtc], [OrganizationId])
VALUES (1, NULL, '2020-08-28T14:31:33.3286953Z', NULL, N'The Big Council', 1, N'TBD', N'lLYCyC1Y', 1, '2020-08-28T14:31:33.3286953Z', 2);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'Name', N'Recurrence', N'RecurrenceData', N'Route', N'TemplateId', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Series]'))
    SET IDENTITY_INSERT [Series] OFF;

GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ClipType', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'FileType', N'PromptId', N'Route', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Clips]'))
    SET IDENTITY_INSERT [Clips] ON;
INSERT INTO [Clips] ([Id], [ClipType], [CreatedById], [CreatedUtc], [DeletedUtc], [FileType], [PromptId], [Route], [UpdatedUtc], [Length])
VALUES (1, 0, 6, '2020-08-28T14:31:33.3280905Z', NULL, 0, 1, N'Ll05UCGA', '2020-08-28T14:31:33.3280905Z', 0),
(2, 0, 7, '2020-08-28T14:31:33.3283201Z', NULL, 0, 1, N'5WAQyNnR', '2020-08-28T14:31:33.3283201Z', 0),
(3, 1, 8, '2020-08-28T14:31:33.3283315Z', NULL, 0, 1, N'Hj7DFXyl', '2020-08-28T14:31:33.3283315Z', 0),
(4, 2, 15, '2020-08-28T14:31:33.3283346Z', NULL, 0, 1, N'7xMLuGNK', '2020-08-28T14:31:33.3283346Z', 0);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ClipType', N'CreatedById', N'CreatedUtc', N'DeletedUtc', N'FileType', N'PromptId', N'Route', N'UpdatedUtc') AND [object_id] = OBJECT_ID(N'[Clips]'))
    SET IDENTITY_INSERT [Clips] OFF;

GO