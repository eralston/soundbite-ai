using Soundbite.Services.Services;


namespace Soundbite.Services.Tests
{
    /// <summary>
    /// Tests for <see cref="AiService"/> and friends
    /// </summary>
    public class TeamsGraphServiceTests : ServiceTestBase
    {
        /*

        [Fact]
        [Trait("Integration", "true")]        
        public async Task MassNotificationTest()
        {
            // Arrange
            HttpClient httpClient = new HttpClient();
            Mock<IHttpClientFactory> httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(i => i.CreateClient(It.IsAny<string>()))
                .Returns(() => httpClient);                
            Mock<IOrgSettingsService> orgSettings = new Mock<IOrgSettingsService>();
            orgSettings
                .Setup(i => i.ReadOrgSettingsAsync<OrgSettings>(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new OrgSettings()
                {
                    Azure = new OrgAzureSettings()
                    {
                        TenantId = "[AMS TENANT ID]",
                        EnableTeamsNotifications = true
                    }
                }));

            Mock<IUserSettingsService> userSettings = new Mock<IUserSettingsService>();
            TeamsAppAzureSettings teamsAppAzureSettings = new TeamsAppAzureSettings()
            {
                BaseUrl = "[TEAMS APP BASE URL]",
                ClientId = "[TEAMS APP API KEY]",
             tran   SecretKey = "[TEAMS APP SECRET]"
            };

            TeamsGraphService teamsGraphService = new TeamsGraphService(
                new NullLogger<TeamsGraphService>(),
                httpClientFactory.Object,
                teamsAppAzureSettings,
                orgSettings.Object,
                userSettings.Object);

            string url = teamsAppAzureSettings.DeepLinkForPlay(new Session() { Route = "SOME_ROUTE" });
        
            await teamsGraphService.SendTeamsAppMassNotification("TestOrgRoute", new string[] { 
                "damon@soundbite.ai",
                "blarg234234432@soundbite.ai"
            }, "Test: Mass", url, "Unit Tests");
        }

        [Fact]
        [Trait("Integration", "true")]
        public async Task SendNormalNotification()
        {
            // Arrange
            HttpClient httpClient = new HttpClient();
            Mock<IHttpClientFactory> httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(i => i.CreateClient(It.IsAny<string>()))
                .Returns(() => httpClient);
            Mock<IOrgSettingsService> orgSettings = new Mock<IOrgSettingsService>();
            orgSettings
                .Setup(i => i.ReadOrgSettingsAsync<OrgSettings>(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new OrgSettings()
                {
                    Azure = new OrgAzureSettings()
                    {
                        TenantId = "[AMS TENANT ID]",
                        EnableTeamsNotifications = true
                    }
                }));

            Mock<IUserSettingsService> userSettings = new Mock<IUserSettingsService>();
            userSettings.Setup(i => i.ReadConfigAsync<Masticore.UserSettings>(It.IsAny<string>()))
                .Returns(Task.FromResult(new Masticore.UserSettings()
                {
                    MsTeams = new UserMsTeamsSettings()
                    {
                        AltId = "Damon@soundbite.ai",
                        IsEnabled = AutoFlagStateType.AutoEnabled
                    }
                })); ;

            TeamsAppAzureSettings teamsAppAzureSettings = new TeamsAppAzureSettings()
            {
                BaseUrl = "[TEAMS APP BASE URL]",
                ClientId = "[TEAMS APP API KEY]",
                SecretKey = "[TEAMS APP SECRET]"
            };

            TeamsGraphService teamsGraphService = new TeamsGraphService(
                new NullLogger<TeamsGraphService>(),
                httpClientFactory.Object,
                teamsAppAzureSettings,
                orgSettings.Object,
                userSettings.Object);

            string url = teamsAppAzureSettings.DeepLinkForPlay(new Session() { Route = "SOME_ROUTE" });

            await teamsGraphService.SendTeamsAppNotification("SOME_ROUTE", "USER", "damon@soundbite.ai", "Test: Normal", url, "Unit Tests");
        }

        [Fact]
        [Trait("Integration", "true")]
        public async Task SendBatchNotification()
        {
            // Arrange
            HttpClient httpClient = new HttpClient();
            Mock<IHttpClientFactory> httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(i => i.CreateClient(It.IsAny<string>()))
                .Returns(() => httpClient);
            Mock<IOrgSettingsService> orgSettings = new Mock<IOrgSettingsService>();
            orgSettings
                .Setup(i => i.ReadOrgSettingsAsync<OrgSettings>(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new OrgSettings()
                {
                    Azure = new OrgAzureSettings()
                    {
                        TenantId = "[AMS TENANT ID]",
                        EnableTeamsNotifications = true
                    }
                }));

            Mock<IUserSettingsService> userSettings = new Mock<IUserSettingsService>();
            userSettings.Setup(i => i.ReadConfigAsync<Masticore.UserSettings>(It.IsAny<string>()))
                .Returns(Task.FromResult(new Masticore.UserSettings()
                {
                    MsTeams = new UserMsTeamsSettings()
                    {
                        AltId = "Damon@soundbite.ai",
                        IsEnabled = AutoFlagStateType.AutoEnabled
                    }
                })); ;

            TeamsAppAzureSettings teamsAppAzureSettings = new TeamsAppAzureSettings()
            {
                BaseUrl = "[TEAMS APP BASE URL]",
                ClientId = "[TEAMS APP API KEY]",
                SecretKey = "[TEAMS APP SECRET]"
            };

            TeamsGraphService teamsGraphService = new TeamsGraphService(
                new NullLogger<TeamsGraphService>(),
                httpClientFactory.Object,
                teamsAppAzureSettings,
                orgSettings.Object,
                userSettings.Object);

            string url = teamsAppAzureSettings.DeepLinkForPlay(new Session() { Route = "SOME_ROUTE" });

            List<string> upns = new List<string> { "Damon@soundbite.ai" };

            await teamsGraphService.SendTeamsAppBatchNotification("SOME_ROUTE", upns, "Test: Batch (Upns)", url, "Unit Tests");
        }

        [Fact]
        [Trait("Integration", "true")]
        public async Task SendBatchRequestNotification()
        {
            // Arrange
            HttpClient httpClient = new HttpClient();
            Mock<IHttpClientFactory> httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory
                .Setup(i => i.CreateClient(It.IsAny<string>()))
                .Returns(() => httpClient);
            Mock<IOrgSettingsService> orgSettings = new Mock<IOrgSettingsService>();
            orgSettings
                .Setup(i => i.ReadOrgSettingsAsync<OrgSettings>(It.IsAny<string>()))
                .Returns(() => Task.FromResult(new OrgSettings()
                {
                    Azure = new OrgAzureSettings()
                    {
                        TenantId = "[AMS TENANT ID]",
                        EnableTeamsNotifications = true
                    }
                }));

            Mock<IUserSettingsService> userSettings = new Mock<IUserSettingsService>();
            userSettings.Setup(i => i.ReadConfigAsync<Masticore.UserSettings>(It.IsAny<string>()))
                .Returns(Task.FromResult(new Masticore.UserSettings()
                {
                    MsTeams = new UserMsTeamsSettings()
                    {
                        AltId = "Damon@soundbite.ai",
                        IsEnabled = AutoFlagStateType.AutoEnabled
                    }
                })); ;

            TeamsAppAzureSettings teamsAppAzureSettings = new TeamsAppAzureSettings()
            {
                BaseUrl = "[TEAMS APP BASE URL]",
                ClientId = "[TEAMS APP API KEY]",
                SecretKey = "[TEAMS APP SECRET]"
            };

            TeamsGraphService teamsGraphService = new TeamsGraphService(
                new NullLogger<TeamsGraphService>(),
                httpClientFactory.Object,
                teamsAppAzureSettings,
                orgSettings.Object,
                userSettings.Object);

            string url = teamsAppAzureSettings.DeepLinkForPlay(new Session() { Route = "SOME_ROUTE" });

            List<string> upns = new List<string> { "damon@soundbite.ai" };

            await teamsGraphService.SendTeamsAppBatchNotification("SOME_ROUTE", new TeamsAppBatchNotificationItem[] {
                new TeamsAppBatchNotificationItem()
                {
                    Title = "Test: Batch (Requests)",
                    Url = "[TEAMS APP BASE URL]#sbplay=SOME_ROUTE",
                    Author = "Unit Tests",
                    NotificationType = SessionNotificationType.Publish,
                    Recipients = new List<TeamsAppBatchNotificationRecipient>() {
                        new TeamsAppBatchNotificationRecipient(){
                            SessionNotificationId = 1,
                            Upn = "Damon@soundbite.ai"
                        }
                    }
                }
            }, "eyJ0eXAiOiJKV1QiLCJub25jZSI6Ind1TVNtVGlDd3EyMThoVjBiZlpMaFNJRWZjWWFZTFRINU1zenRfczEtRHciLCJhbGciOiJSUzI1NiIsIng1dCI6IjVCM25SeHRRN2ppOGVORGMzRnkwNUtmOTdaRSIsImtpZCI6IjVCM25SeHRRN2ppOGVORGMzRnkwNUtmOTdaRSJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTAwMDAtYzAwMC0wMDAwMDAwMDAwMDAiLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9lNWY3NTYzMi02MzdkLTRkMmUtYThhNy1lOGY2OTkzYWNiNzQvIiwiaWF0IjoxNzA0MDUwMzg5LCJuYmYiOjE3MDQwNTAzODksImV4cCI6MTcwNDA1NDI4OSwiYWlvIjoiRTJWZ1lJaFR5ZDV3NjlUNW9QT1ZZaHlyenpUTUFRQT0iLCJhcHBfZGlzcGxheW5hbWUiOiJTb3VuZGJpdGXihKIgZm9yIE1TIFRlYW1zIC0gVGVzdCIsImFwcGlkIjoiZWQxYzU2MTgtYWEzMC00YmVhLThjNzgtNzI5NWZkZjc4NmNkIiwiYXBwaWRhY3IiOiIxIiwiaWRwIjoiaHR0cHM6Ly9zdHMud2luZG93cy5uZXQvZTVmNzU2MzItNjM3ZC00ZDJlLWE4YTctZThmNjk5M2FjYjc0LyIsImlkdHlwIjoiYXBwIiwib2lkIjoiMzk4M2RiYjMtMTE2OC00Y2Y3LTljMDItZDA4Yjc4MDQ1MWI3IiwicmgiOiIwLkFWRUFNbGIzNVgxakxrMm9wLWoybVRyTGRBTUFBQUFBQUFBQXdBQUFBQUFBQUFCUkFBQS4iLCJyb2xlcyI6WyJUZWFtc0FjdGl2aXR5LlNlbmQiXSwic3ViIjoiMzk4M2RiYjMtMTE2OC00Y2Y3LTljMDItZDA4Yjc4MDQ1MWI3IiwidGVuYW50X3JlZ2lvbl9zY29wZSI6Ik5BIiwidGlkIjoiZTVmNzU2MzItNjM3ZC00ZDJlLWE4YTctZThmNjk5M2FjYjc0IiwidXRpIjoiaTA3YUF0Rnp2RUNLV250VHNoYVpBQSIsInZlciI6IjEuMCIsIndpZHMiOlsiMDk5N2ExZDAtMGQxZC00YWNiLWI0MDgtZDVjYTczMTIxZTkwIl0sInhtc190Y2R0IjoxNTg5NzYxMDQxfQ.pXZzOGgvNoSCxV-3VoANsnpS00GO46kIi05UztYo7R7PbP3pRTkYeM1CPKS-OxyXod7xMvPJwnSI-0aeumjvR6FklSQTikReL-DLnRUBnhDz-l3G5BcX4Jn4sq26M0I0PolFhBXqEpxoKKe4JT4b2HijqyV-Lw9EknX6KYBv9Trv9NHui9KLBL9iuGpnyvOgVvv7BH9JBv1uKfVkWv15Nplzbx8CfytwQ-kpkI7j2iIEz4p61OUVxUkEkS-CJhCk4GBoJ3ZKKa7PmQA46kSB3hCBisPWAcbNnJt39mDalkp05hBjsJI4xdazyDSjt9d7nURxJicVGEyUZpELFDlKqA");
            //await teamsGraphService.SendTeamsAppBatchNotification("SOME_ROUTE", upns, "Test: Batch (Requests)", url, "Unit Tests");
        }
        */
    }
}