using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Masticore.Sms
{
    /// <summary>
    /// Interface for an SMS Gateway
    /// </summary>
    public interface ISmsGateway
    {
        /// <summary>
        /// Async send the given message, returning a unique identifier for it if successful
        /// </summary>
        /// <param name="sms"></param>
        /// <returns></returns>
        Task<string> SendAsync(Message sms);
    }

    /// <summary>
    /// Twilio-powered SMS client that is intended to be a singleton service
    /// </summary>
    public class TwilioSmsGateway : ISmsGateway
    {
        /// <summary>
        /// Configures the shared phone number for sending SMS messages
        /// </summary>
        public static string FromPhone { get; set; }

        /// <summary>
        /// Initialize the gateway with the given settings object
        /// </summary>
        /// <param name="settings"></param>
        public static void Init(TwilioSettings settings)
        {
            Init(settings.AccountSID, settings.AuthToken, settings.PhoneNumber);
        }

        /// <summary>
        /// Initialize the gateway with the given settings
        /// </summary>
        /// <param name="accountSid"></param>
        /// <param name="authToken"></param>
        /// <param name="fromPhone"></param>
        public static void Init(string accountSid, string authToken, string fromPhone)
        {
            FromPhone = fromPhone;
            TwilioClient.Init(accountSid, authToken);
        }

        /// <summary>
        /// Async sends a text message of the given format
        /// </summary>
        /// <param name="sms"></param>
        /// <returns></returns>
        public async Task<string> SendAsync(Message sms)
        {
            Validator.ArgNotNull(nameof(sms), sms);
            sms.Validate();

            // If we can't get a number in the right format for Twilio, then we abort
            string to = sms.To?.ToE164Format();

            if (to == null)
            {
                return null;
            }

            MessageResource response = await MessageResource.CreateAsync(
                body: sms.Body,
                from: FromPhone,
                to: sms.To.Number
            );

            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                throw new Exception("Failed to Send SMS");
            }

            return response.Sid;
        }
    }
}
