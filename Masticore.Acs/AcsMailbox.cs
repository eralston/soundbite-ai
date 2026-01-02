using Azure;
using Azure.Communication.Email;

namespace Masticore.Acs
{
    /// <summary>
    /// Provides the <see cref="AcsSettings"/> for a given context
    /// </summary>
    public interface IAcsSettingFactory
    {
        /// <summary>
        /// Maps the given orgroute to an <see cref="AcsSettings"/> object
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<AcsSettings> GetSettingsAsync(string? orgRoute = null);
    }

    /// <summary>
    /// Azure implementation of <see cref="IMailbox"/>
    /// </summary>
    public class AcsMailbox : IMailbox
    {
        protected class SettingsAndClient
        {
            public EmailClient? Client;
            public AcsSettings? Settings;
        }

        private static async Task SendMessageWithSettings(MailMessage mail, SettingsAndClient both)
        {
            EmailContent content = new(mail.Subject)
            {
                PlainText = mail.Body.Text,
                Html = mail.Body.Html
            };

            EmailRecipients toAddresses = new();
            toAddresses.To.Add(GetEmailAddress(mail.To));
            if (mail.CC_1 != null)
            {
                toAddresses.CC.Add(GetEmailAddress(mail.CC_1));
            }
            if (mail.CC_2 != null)
            {
                toAddresses.CC.Add(GetEmailAddress(mail.CC_2));
            }

            EmailMessage message = new(both.Settings?.FromEmail, toAddresses, content);
            message.ReplyTo.Add(new EmailAddress(both.Settings?.FromEmail, both.Settings?.FromName));

            Validator.ArgNotNull(nameof(both.Client), both.Client, "E-Mail client was not found, unable to proceed with sending message");
            await both.Client.SendAsync(WaitUntil.Completed, message, CancellationToken.None);
        }

        protected AcsSettings DefaultSettings { get; }
        protected EmailClient? DefaultClient { get; set; }
        protected SettingsAndClient? _defaultBoth { get; set; }

        protected IAcsSettingFactory? Factory { get; }
        protected Dictionary<string, SettingsAndClient> Clients { get; } = new Dictionary<string, SettingsAndClient>();

        /// <summary>
        /// Constructor - API Key is required, logger is optional
        /// </summary>
        /// <param name="apiKey"></param>
        public AcsMailbox(AcsSettings settings, IAcsSettingFactory? factory = null)
        {
            Validator.ArgNotNull(nameof(settings), settings);
            Validator.ArgNotNullOrEmpty(nameof(settings.ConnectionString), settings.ConnectionString);
            Validator.ArgNotNullOrEmpty(nameof(settings.FromEmail), settings.FromEmail);
            DefaultSettings = settings;
            Factory = factory;
        }

        protected async Task<SettingsAndClient> GetSettingsForOrg(string? orgRoute = null)
        {
            if (string.IsNullOrEmpty(orgRoute) || Factory == null)
            {
                return DefaultSettingsAndClient;
            }
            else if (Clients.ContainsKey(orgRoute))
            {
                return Clients[orgRoute];
            }
            else
            {
                AcsSettings settingsForOrg = await Factory.GetSettingsAsync(orgRoute);
                if (settingsForOrg != null && !string.IsNullOrWhiteSpace(settingsForOrg.ConnectionString))
                {
                    EmailClient newClient = new EmailClient(settingsForOrg.ConnectionString);
                    SettingsAndClient both = new SettingsAndClient { Client = newClient, Settings = settingsForOrg };
                    Clients[orgRoute] = both;
                    return both;
                }
                else
                {
                    Clients[orgRoute] = DefaultSettingsAndClient;
                    return DefaultSettingsAndClient;
                }
            }
        }

        /// <summary>
        /// Lazy loads an <see cref="EmailClient"/> instance
        /// </summary>
        protected SettingsAndClient DefaultSettingsAndClient
        {
            get
            {
                if (_defaultBoth == null)
                {
                    if (string.IsNullOrEmpty(DefaultSettings.ConnectionString))
                    {
                        throw new Exception($"{nameof(AcsMailbox)} cannot send e-mail with empty API key");
                    }
                    DefaultClient = new EmailClient(DefaultSettings.ConnectionString);
                    _defaultBoth = new SettingsAndClient { Client = DefaultClient, Settings = DefaultSettings };
                }
                return _defaultBoth;
            }
        }

        /// <summary>
        /// Converts from <see cref="Address"/> to <see cref="EmailAddress"/>
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        protected static EmailAddress GetEmailAddress(Address addr)
        {
            return new EmailAddress(addr.Email, addr.Name);
        }

        #region IMailbox

        /// <inheritdoc />
        public async Task SendAsync(MailMessage mail)
        {
            await SendMessageWithSettings(mail, DefaultSettingsAndClient);
        }

        /// <inheritdoc />
        public async Task SendForOrgAsync(MailMessage mail, string orgRoute)
        {
            SettingsAndClient settings = await GetSettingsForOrg(orgRoute);
            await SendMessageWithSettings(mail, settings);
        }

        #endregion
    }
}
