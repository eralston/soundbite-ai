using Newtonsoft.Json;

namespace Soundbite.Models
{
    /// <summary>
    /// Represents a single recipient in the <see cref="TeamsActivityMassNotificationRequest"/>.
    /// </summary>    
    public class TeamsActivityMassNotificationRecipient
    {
        /// <summary>
        /// Gets or sets the ODataType for the recipient. There is currently only one supported
        /// and the value for that type is assigned by default.
        /// </summary>
        [JsonProperty("@odata.type")]
        public string ODataType { get; set; } = "microsoft.graph.aadUserNotificationRecipient";

        /// <summary>
        /// Gets or sets the user principal name (UPN) of the intended recipient. At the time of 
        /// writing this value needs to be the UPN.  Sometimes the upn and email address of the
        /// user are the same, sometimes they are not.  This MUST be the UPN.
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; }
    }
}
