using Masticore.Exceptions;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Threading.Tasks;

namespace Masticore.Mail
{
    /// <summary>
    /// Class for reading SendGrid settings from appsettings.json
    /// </summary>
    public class SendGridSettings
    {
        public string ApiKey { get; set; }
    }

    /// <summary>
    /// E-mail implementation by SendGrid
    /// https://sendgrid.com/partners/azure/
    /// https://sendgrid.com/docs/for-developers/sending-email/v3-csharp-code-example/
    /// </summary>
    public class SendGridMailbox : IMailbox
    {
        protected ILogger<SendGridMailbox> Logger { get; }

        public SendGridMailbox(ILogger<SendGridMailbox> logger)
        {
            Logger = logger;
        }

        private string ApiKey { get; }

        private SendGridClient Client
        {
            get
            {
                string apiKey = ApiKey;
                SendGridClient client = new SendGridClient(apiKey);
                return client;
            }
        }

        public SendGridMailbox(string apiKey)
        {
            ApiKey = apiKey;
        }

        public EmailAddress Addr(Address addr)
        {
            return new EmailAddress(addr.Email, addr.Name);
        }

        public async Task SendAsync(MailMessage mail)
        {
            EmailAddress from = Addr(mail.From);
            EmailAddress to = Addr(mail.To);

            SendGridMessage msg = MailHelper.CreateSingleEmail(
                from,
                to,
                mail.Subject,
                mail.Body.Text,
                mail.Body.Html);

            if (mail.CC_1 != null && mail.CC_1.Email.ToLower() != to.Email.ToLower())
            {
                msg.AddCc(Addr(mail.CC_1));
            }

            if (mail.CC_2 != null && mail.CC_2.Email.ToLower() != to.Email.ToLower())
            {
                msg.AddCc(Addr(mail.CC_2));
            }

            Response response = await Client.SendEmailAsync(msg).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                string body = await response.Body.ReadAsStringAsync();
                Logger?.LogError($"Failed in {nameof(SendGridMailbox)} trying to send to e-mail '{mail.To.Email.RedactEmail()}': {body}");

                throw new UserSafeException($"Failed trying to send to e-mail '{mail.To.Email.RedactEmail()}'");
            }
        }

        public async Task SendForOrgAsync(MailMessage message, string orgRoute)
        {
            // This does NOT implement org-settings-based custom e-mails because SendGrid doesn't support it
            await SendAsync(message);
        }
    }
}
