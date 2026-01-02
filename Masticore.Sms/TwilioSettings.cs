namespace Masticore.Sms
{
    /// <summary>
    /// Expected format for Twilio connection settings as of 2022Q1
    /// https://www.twilio.com/
    /// </summary>
    public class TwilioSettings
    {
        /// <summary>
        /// Your Twilio account ID
        /// https://www.twilio.com/docs/glossary/what-is-a-sid
        /// </summary>
        public string AccountSID { get; set; }

        /// <summary>
        /// Unique hashed token for accessing Twilio
        /// https://support.twilio.com/hc/en-us/articles/223136027-Auth-Tokens-and-How-to-Change-Them
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// Phone number from which Twilio will send the message
        /// Must be configured in Twilio
        /// https://www.twilio.com/phone-numbers
        /// </summary>
        public string PhoneNumber { get; set; }
    }
}
