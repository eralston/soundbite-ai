namespace Masticore.Sms
{
    /// <summary>
    /// SMS Message format
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Optional phone number to send the message from
        /// </summary>
        public PhoneNumber From { get; set; }

        /// <summary>
        /// Required number to send the message to
        /// </summary>
        public PhoneNumber To { get; set; }

        /// <summary>
        /// Required body for message
        /// </summary>
        public string Body { get; set; }

        public void Validate()
        {
            // From is optional
            Validator.ArgNotNull(nameof(To), To);
            Validator.ArgNotNull(nameof(To.Number), To.Number);
            Validator.ArgNotNull(nameof(Body), Body);
        }
    }
}
