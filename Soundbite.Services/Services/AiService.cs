using Masticore;
using Masticore.Ai;
using Masticore.Entity;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Masticore.Transcription;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soundbite.Services.Services
{
    /// <summary>
    /// EF Implementatin of <see cref="IAiService"/> harnessing <see cref="AiClient"/>
    /// </summary>
    public class AiService : InfrastructureServiceBase<ISbInfrastructure>, IAiService
    {
        protected static string ParagraphPrompt = Environment.GetEnvironmentVariable($"{nameof(AiService)}__{nameof(ParagraphPrompt)}") ?? "Generate a one paragraph professional summary for this text: {0}";
        protected static string SocialPrompt = Environment.GetEnvironmentVariable($"{nameof(AiService)}__{nameof(SocialPrompt)}") ?? "Generate three potential social media posts for this text: {0}";
        protected static string TweetPrompt = Environment.GetEnvironmentVariable($"{nameof(AiService)}__{nameof(TweetPrompt)}") ?? "Generate three different potential tagged tweets for this text: {0}";
        protected static string NewsPrompt = Environment.GetEnvironmentVariable($"{nameof(AiService)}__{nameof(NewsPrompt)}") ?? "Generate a short newsletter article with a second person perspective and conversational tone for this text: {0}";
        protected static string BlogPrompt = Environment.GetEnvironmentVariable($"{nameof(AiService)}__{nameof(BlogPrompt)}") ?? "Generate a first person blog post with call to action for this text: {0}";

        /// <summary>
        /// Gets the <see cref="IRbac"/> object for this service, which controls access
        /// </summary>
        protected IRbac Rbac { get; }
        public ISbTranscriptionService Transcripts { get; }

        /// <summary>
        /// <see cref="AiClient"/> for this service
        /// </summary>
        public IAiClient AiClient { get; private set; }

        #region Methods

        public AiService(
            ISbInfrastructure infrastructure,
            ILogger<AiService> logger,
            IRbac rbac,
            IAiClient aiClient,
            ISbTranscriptionService transcripts)
            : base(infrastructure, logger)
        {
            AiClient = aiClient ?? throw new ArgumentNullException(nameof(aiClient));
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
            Transcripts = transcripts ?? throw new ArgumentNullException(nameof(transcripts));
        }
        private async Task<string> ReadCombinedTextOfTranscript(string orgRoute, string sessionRoute, SbDb db)
        {
            SessionEntity session = await ReadSessionAsync(db, orgRoute, sessionRoute);

            StringBuilder combinedText = new StringBuilder();
            IEnumerable<ClipEntity> clips = session.Prompts.SelectMany(p => p.Clips);
            // TODO: Doing this clip-by-clip is not efficient against the DB
            // If we ever get to the point where we do have larger collections of clips to summarize, this likely needs some minimizing of queries
            foreach (ClipEntity clip in clips)
            {
                if (clip.ClipType.SupportsTranscription())
                {
                    TranscriptionResult result = await Transcripts.ReadTranscript(orgRoute, sessionRoute, clip.Route, session.SessionSecurity == SessionSecurityType.Public);
                    combinedText.Append(result.GetText());
                }
            }

            string content = combinedText.ToString();
            return content;
        }

        /// <summary>
        /// Read the given session, do an RBAC-enforced read on the session
        /// </summary>
        /// <param name="db"></param>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns><see cref="SessionEntity"/> with its prompts and clips loaded</returns>
        private async Task<SessionEntity> ReadSessionAsync(SbDb db, string orgRoute, string sessionRoute)
        {
            // RBAC Baked in here
            int[] groupIds = await db.GroupIdsAsync(orgRoute, Rbac.UniversalIdForCurrentUser);
            SessionEntity session = await db.SessionAsync(Rbac.UniversalIdForCurrentUser, orgRoute, sessionRoute, groupIds, ParticipantRole.Host, true);
            session.AssertFound();
            return session;
        }

        private static string SessionPrompt(string content, SessionSummaryType summaryType)
        {
            return summaryType switch
            {
                SessionSummaryType.Paragraph => string.Format(ParagraphPrompt, content),
                SessionSummaryType.Social => string.Format(SocialPrompt, content),
                SessionSummaryType.Tweet => string.Format(TweetPrompt, content),
                SessionSummaryType.Newsletter => string.Format(NewsPrompt, content),
                SessionSummaryType.Blog => string.Format(BlogPrompt, content),
                // Single paragraph is probably the most generic summary
                _ => string.Format(ParagraphPrompt, content),
            };
        }

        #endregion

        #region Implementation of IAiService

        /// <inheritdoc />
        public async Task<SessionSummary> SessionSummaryAsync(string orgRoute, string sessionRoute, SessionSummaryType summaryType = SessionSummaryType.Paragraph)
        {
            Validator.ArgNotNullOrEmpty(nameof(orgRoute), orgRoute);
            Validator.ArgNotNullOrEmpty(nameof(sessionRoute), sessionRoute);

            await Rbac.AssertCurrentUserInRole(orgRoute, PersonRole.Person);

            SbDb db = await Infrastructure.DbAsync();
            // RBAC on session baked in
            string transcript = await ReadCombinedTextOfTranscript(orgRoute, sessionRoute, db);
            if (string.IsNullOrWhiteSpace(transcript))
            {
                return null;
            }

            string aiPrompt = SessionPrompt(transcript, summaryType);
            AiChatResponse aiResponse = await AiClient.RequestChat(new AiChatRequest(aiPrompt) { Divergence = 0 });
            SessionSummary ret = new SessionSummary()
            {
                OrgRoute = orgRoute,
                SessionRoute = sessionRoute,
                Summary = aiResponse.Text,
                Transcript = transcript,
            };
            return ret;
        }

        #endregion
    }
}
