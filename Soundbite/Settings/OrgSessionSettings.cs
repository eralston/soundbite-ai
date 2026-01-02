using Masticore;

namespace Soundbite
{
    /***********************************************************************************************
     * WARNING: Organization settings are sent client-side.  Any secure data must be marked with
     *  JsonIgnore attribute to avoid being sent out and compromising security.
     **********************************************************************************************/

    /// <summary>
    /// Defines session-specific settings available at an organization level.
    /// </summary>
    public class OrgSessionSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the organization allows comments at the top level
        /// </summary>
        public bool SessionCommentsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the default session comment policy
        /// </summary>
        public SessionCommentPolicy DefaultSessionCommentPolicy { get; set; } = SessionCommentPolicy.Allowed;

        /// <summary>
        /// Gets or sets the default Session security value for an organization.
        /// </summary>
        public SessionSecurityType DefaultSessionSecurity { get; set; } = SessionSecurityType.Protected;

        /// <summary>
        /// Gets or sets a value indicating whether transcription is enabled for sessions.
        /// </summary>
        public bool TranscriptionEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a flag indicating whether transcription is enabled by default.
        /// </summary>
        public bool TranscriptionOnByDefault { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether video is enabled for sessions.
        /// </summary>
        public bool VideoEnabled { get; set; }
    }
}