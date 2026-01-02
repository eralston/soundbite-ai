//using Masticore;
//using Masticore.Ai;
//using Masticore.Entity;
//using Masticore.Entity.Tests;
//using Masticore.Models;
//using Masticore.Resources;
//using Masticore.Security;
//using Masticore.Tests;
//using Masticore.Transcription.Azure;
//using Microsoft.Extensions.Logging.Abstractions;
//using Soundbite.Entity;
//using Soundbite.Models;
//using Soundbite.Models.JobRequests;
//using Soundbite.Services.Services;
//using Soundbite.Services.Tests.Content;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;

//namespace Soundbite.Services.Tests
//{
//    /// <summary>
//    /// Tests for <see cref="AiService"/> and friends
//    /// </summary>
//    public class AiServiceTests : ServiceTestBase
//    {
//        public static string WavFilePath = Environment.CurrentDirectory + "\\..\\..\\..\\files\\NoBasisForGovt.wav";
//        public static string OtherWavFilePath = Environment.CurrentDirectory + "\\..\\..\\..\\files\\Recording.wav";
//        // public const string Mp3FilePath = "C:\\Code\\Soundbite\\Masticore.Transcription.Tests\\files\\NoBasisForGovt.mp3";
//        public static string Mp3FilePath = Environment.CurrentDirectory + "\\..\\..\\..\\files\\TranscriptionTests.mp3";
//        public const string AzureRegion = "westus";
//        public const string AzureKey = "[AZURE TRANSCRIPTION KEY]";

//        private static IEnumerable<NewParticipant> ToNewParticipants(IEnumerable<PersonEntity> audience, ParticipantRole role)
//        {
//            return audience.Select((p) =>
//            {
//                return ToNewParticipant(role, p);
//            });
//        }

//        private static NewParticipant ToNewParticipant(ParticipantRole role, PersonEntity p)
//        {
//            return new NewParticipant { ParticipantRole = role, PersonRoute = p.Route };
//        }

//        [Fact]
//        public async Task SessionSummaryAsync()
//        {
//            // ARRANGE

//            // 1) Create Session With Real Recording
//            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
//            SbDb db = Builder.DbContext.Value;

//            SessionService sessions = new SessionService(
//                Builder.Rbac.Value,
//                Builder.SeriesService.Value,
//                Builder.ImageService.Value,
//                Builder.ClipFileService.Value,
//                Builder.LifeCycleFactory.Value,
//                Builder.Infrastructure.Value,
//                Builder.ProviderFactory.Value,
//                Builder.Mapper.Value,
//                ThrowIfErrorLogger<SessionService>.Instance);

//            // Make Things

//            PersonEntity me = await db.PersonWhereRoute(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.PersonOrgAdmin.Route);
//            IEnumerable<PersonEntity> peopleInOrg = await db.PeopleAsync(IdentityDbSeed.OrgPrimary.Route);
//            IEnumerable<NewParticipant> hosts = new NewParticipant[] { ToNewParticipant(ParticipantRole.Host, me) };
//            IEnumerable<NewParticipant> newAudience = ToNewParticipants(peopleInOrg, ParticipantRole.Audience);
//            IEnumerable<NewParticipant> allParticipants = newAudience.Concat(hosts);

//            NewSession newSession = new NewSession
//            {
//                Recurrence = Recurrence.NoRepeat,
//                RecurrenceData = null,
//                Name = "Happy Path",
//                Limit = 120,
//                SessionType = SessionType.Announcement,
//                SessionSecurity = SessionSecurityType.Protected,
//                ReminderSent = null,
//                Publish = Time.UtcNow,
//                PublishSent = null,
//                FirstPrompt = "Is this working?",
//                Participants = allParticipants,
//            };

//            SessionDetails session = await sessions.CreateAsync(
//                IdentityDbSeed.OrgPrimary.Route,
//                newSession);

//            ClipService clips = new ClipService(
//                Builder.SecurityContext.Value,
//                Builder.ClipFileService.Value,
//                Builder.LifeCycleFactory.Value,
//                Builder.Infrastructure.Value,
//                Builder.SbMediaService.Value,
//                Builder.Mapper.Value,
//                ThrowIfErrorLogger<ClipService>.Instance,
//                Builder.Rbac.Value);
//            NewClip newClip = new NewClip
//            {
//                ClipType = ClipType.Prompt,
//                FileType = FileType.Mp3,
//                Stream = MediaExamples.Valid_TransciptRecording,
//                ParticipantRole = ParticipantRole.Host
//            };
//            ClipDetails clip = await clips.CreateAsync(IdentityDbSeed.OrgPrimary.Route, session.Route, session.Prompts.First().Route, newClip);

//            // 2) Transcribe Recording

//            AzureTranscriptionService azureTranscriptions = new AzureTranscriptionService(
//                new NullLogger<AzureTranscriptionService>(),
//                Builder.MediaProcessing.Value,
//                AzureRegion,
//                AzureKey);

//            Builder.TranscriptionService.Use(azureTranscriptions);

//            SbTranscriptionService transcripts = new SbTranscriptionService(
//                ThrowIfErrorLogger<SbTranscriptionService>.Instance,
//                Builder.Infrastructure.Value,
//                Builder.SecurityContext.Value,
//                Builder.TranscriptionService.Value,
//                Builder.JobQueue.Value,
//                Builder.QueuedJobStatus.Value,
//                Builder.ClipFileService.Value,
//                Builder.TranscriptFileService.Value,
//                Builder.LifeCycleFactory.Value,
//                Builder.Rbac.Value
//                );

//            await transcripts.ProcessTranscriptionRequest(new TranscriptionRequest
//            {
//                OrgRoute = IdentityDbSeed.OrgPrimary.Route,
//                SessionRoute = session.Route,
//                PromptRoute = session.Prompts.First().Route,
//                ClipRoute = clip.ClipRoute,
//                ClipFileType = FileType.Mp3
//            });

//            // 3) Setup AI Client
//            // TODO: Move somewhere actually secret
//            OpenAiSettings settings = new OpenAiSettings()
//            {
//                Region = "southcentralus",
//                ApiKey = "[OPEN AI API KEY]",
//                Endpoint = "[OPEN AI ENDPOINT]",
//                Deployment = "test-playground"
//            };

//            OpenAiCompletionClient aiClient = new OpenAiCompletionClient(
//                ThrowIfErrorLogger<OpenAiCompletionClient>.Instance,
//                settings,
//                Masticore.Tests.Extensions.CreateHttpClientFactory());
//            AiService aiService = new AiService(
//                Builder.Infrastructure.Value,
//                ThrowIfErrorLogger<AiService>.Instance,
//                Builder.Rbac.Value,
//                aiClient,
//                transcripts);

//            // ACT

//            // 4) Request suggested CTA based on the transcript

//            SessionSummary cta = await aiService.SessionSummaryAsync(IdentityDbSeed.OrgPrimary.Route, session.Route);

//            Assert.NotNull(cta);
//            Assert.NotNull(cta.Summary);
//            // TODO: Some check of its quality
//        }
//    }
//}
