namespace Soundbite.Api
{
    /// <summary>
    /// Client-side feature flags orchestrating what visual interactions are possible
    /// </summary>
    public class FeatureFlags
    {
        /// <summary>
        /// Enable/Disable the generic Invite capabilities that allow sending messages to non-members
        /// </summary>
        public bool ReferralEnabled { get; set; }

        /// <summary>
        /// Enable/Disable the team create buttons and dialog
        /// </summary>
        public bool TeamCreationEnabled { get; set; }
    }
}
