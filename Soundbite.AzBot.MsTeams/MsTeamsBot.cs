using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Soundbite.AzBot.MsTeams
{
    /// <summary>
    /// MS Azure Bot Teams Activity Handler.  Responsible for responding to Bot framework events issued via teams.
    /// </summary>
    public class MsTeamsBot : TeamsActivityHandler
    {
        #region Overrides

        /// <summary>
        /// Invoked when a Messaging Extension Fetch activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="action">The messaging extension action.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {
            try
            {
                string userId = turnContext.Activity.From.AadObjectId;
                TeamsChannelAccount? teamsChannelAcct = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                string? name = teamsChannelAcct.Name;
            }
            catch (ErrorResponseException ex)
            {
                if (ex.Body.Error.Code == "BotNotInConversationRoster")
                {
                    return CreateJustInTimeInstallResponse();
                }
                throw; // It's a different error.
            }

            return CreateSoundbiteTask();

        }

        /// <summary>
        /// Invoked when a messaging extension submit action activity is received from the connector.
        /// </summary>
        /// <param name="turnContext">A strongly-typed context object for this turn.</param>
        /// <param name="action">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken)
        {

            JObject? data = action?.Data as JObject;
            if (data != null)
            {
                string? cmd = data["cmd"]?.Value<string>();
                switch (cmd)
                {
                    case "sessionCreated":
                        return Task.FromResult(CreateSessionCard(data));
                    case null:
                        JObject? msTeams = data["msteams"]?.Value<JObject>();
                        if (msTeams != null && msTeams.Value<bool>("justInTimeInstall") == true)
                        {
                            return Task.FromResult(CreateSoundbiteTask());
                        }
                        break;
                }
            }

            return Task.FromResult<MessagingExtensionActionResponse>(null);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an adaptive card with an action that, when submitted, automatically installs the
        /// bot to a conversation.
        /// </summary>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
        private static MessagingExtensionActionResponse CreateJustInTimeInstallResponse()
        {
            MessagingExtensionActionResponse? response = new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = GetAdaptiveCardAttachmentFromJson(Resources.JustInTimeBotInstallCard),
                        Height = 200,
                        Width = 400,
                        Title = "Adaptive Card - App Installation",
                    },
                },
            };
            return response;
        }

        /// <summary>
        /// Creates a response object that forwards the user to a URL where they can create a Soundbite.
        /// </summary>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
        private static MessagingExtensionActionResponse CreateSoundbiteTask()
        {
            MessagingExtensionActionResponse? response = new MessagingExtensionActionResponse()
            {
                Task = new TaskModuleContinueResponse()
                {
                    Type = "continue",
                    Value = new TaskModuleTaskInfo()
                    {
                        Height = 500,
                        Width = 525,
                        Title = "Record a New Soundbite",
                        Url = "https://soundbite.ai.ngrok.io/create"
                    }
                }
            };

            return response;
        }

        /// <summary>
        /// Creates a Hero Card that allows users to play a soundbite.
        /// </summary>
        /// <param name="data">Information about the Soundbite for which the card is being created.</param>
        /// <returns>The Messaging Extension Action Response for the action.</returns>
        private static MessagingExtensionActionResponse CreateSessionCard(JObject data)
        {
            string? orgRoute = data["orgRoute"]?.Value<string>();
            string? sessionRoute = data["route"]?.Value<string>();
            string? title = data["title"]?.Value<string>();

            HeroCard? card = new HeroCard { };
            card.Buttons = new List<CardAction>
            {
                new CardAction()
                {
                    Type = "openUrl",
                    Title = $"Play - {title}",
                    Value = string.Format("https://localhost:3000/organizations/{0}/feed#sbplay={1}", orgRoute, sessionRoute)
                }
            };

            List<MessagingExtensionAttachment>? attachments = new List<MessagingExtensionAttachment>
            {
                new MessagingExtensionAttachment
                {
                    Content = card,
                    ContentType = HeroCard.ContentType,
                    Preview = card.ToAttachment(),
                }
            };

            return new MessagingExtensionActionResponse
            {
                ComposeExtension = new MessagingExtensionResult
                {
                    AttachmentLayout = "list",
                    Type = "result",
                    Attachments = attachments,
                },
            };
        }

        /// <summary>
        /// Builds an adaptive card based on a JSON string.
        /// </summary>
        /// <param name="adaptiveCardJson">JSON string containing card definition.</param>
        /// <returns>an adaptive card attachment</returns>
        private static Attachment GetAdaptiveCardAttachmentFromJson(string adaptiveCardJson)
        {
            Attachment? adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        #endregion
    }
}