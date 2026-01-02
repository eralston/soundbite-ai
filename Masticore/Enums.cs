namespace Masticore
{
    /// <summary>
    /// Type of roles for a member which varies their level of access to a Group
    /// </summary>
    public enum MemberRole
    {
        /// <summary>
        /// No access
        /// </summary>        
        Unknown = 0,

        /// <summary>
        /// Read all teammates, write own record; team read
        /// </summary>
        Member = 50,

        /// <summary>
        /// Read-write all teammates; team edit
        /// </summary>
        Owner = 100
    }

    /// <summary>
    /// Roles for a person, which varies their level of access in an Organization
    /// </summary>
    public enum PersonRole
    {
        /// <summary>
        /// No access
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Read teams, no Order access
        /// </summary>
        Guest = 25,

        /// <summary>
        /// Read-write teams, Read Org
        /// </summary>
        Person = 50,

        /// <summary>
        /// Read-Write Teams and Org
        /// </summary>
        Admin = 100
    }

    /// <summary>
    /// Enumeration of the various security operation types used to decorate
    /// </summary>
    public enum SecurityOpType
    {
        /// <summary>
        /// Permission for creating data.
        /// </summary>
        Create,

        /// <summary>
        /// Permission for reading data.
        /// </summary>
        Read,

        /// <summary>
        /// Permission for updating existing data.
        /// </summary>
        Update,

        /// <summary>
        /// Permission for deleting data.
        /// </summary>
        Delete,

        /// <summary>
        /// Permission for managing multiple operations.
        /// </summary>
        Manage
    }

    /// <summary>
    /// Enumeration of the various ways to manage security for sessions.
    /// </summary>
    public enum SessionSecurityType
    {
        /// <summary>
        /// Denotes that the session is protected by standard security.
        /// </summary>
        Protected = 0,

        /// <summary>
        /// Denotes that the session is publicly available.
        /// </summary>
        Public = 10
    }

    /// <summary>
    /// Enumeration of the possible session comment policies
    /// </summary>
    public enum SessionCommentPolicy
    {
        /// <summary>
        /// The session has opted-in to comments
        /// </summary>
        Allowed = 0,

        /// <summary>
        /// The session has opted-out on comments
        /// </summary>
        Disallowed = 10,
    }

    /// <summary>
    /// Enumeration identifying the "level" at which token settings are defined.
    /// </summary>
    public enum TokenSecurityLevel
    {
        /// <summary>
        /// Default scope token
        /// </summary>
        Default = 0,

        /// <summary>
        /// Tenant-wide for a set of resources for a set of organizations
        /// </summary>
        Tenant = 10,

        /// <summary>
        /// Org-wide security
        /// </summary>
        Organization = 20
    }

    /// <summary>
    /// Enumeration of the various mechanism for manging token security in the application.
    /// </summary>
    public enum TokenSecurityType
    {
        /// <summary>
        /// Denotes that the signing type is not set, is unknown, or should be inherited.
        /// </summary>
        None = 0,

        /// <summary>
        /// Denotes that the signing mechanism uses a secret key value.
        /// </summary>
        SecretKey = 10,

        /// <summary>
        /// Denotes that the signing mechanism uses a certificate.
        /// </summary>
        Certificate = 20
    }

    /// <summary>
    /// User role in the system, which varies their system-wide access.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// No Access
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///  Read-Write on their own resources
        /// </summary>
        User = 50,

        /// <summary>
        /// Read-Write on all resources
        /// </summary>
        God = 100,
    }

    /// <summary>
    /// Enumeration containing the various methodologies for interacting with calendar events.
    /// </summary>
    public enum CalendarInteractionType
    {
        /// <summary>
        /// Indicates the user does not want to received calendar events.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the user wants to have calendar events created through the calendar
        /// provider associated with their login.
        /// </summary>
        Provider = 10,

        /// <summary>
        /// Indicates that the user wants to have an ICalendar entry sent to their email address.
        /// </summary>
        ICal = 20
    }

    /// <summary>
    /// The available patterns for recurring Sessions
    /// </summary>
    public enum Recurrence
    {
        /// <summary>
        /// A non-repeating pattern
        /// </summary>
        NoRepeat = 0,

        /// <summary>
        /// A Mon - Fri pattern
        /// </summary>
        Weekday = 1,

        /// <summary>
        /// A pattern for 7 days a week
        /// </summary>
        Daily = 2,

        /// <summary>
        /// A pattern for occurrences every week
        /// </summary>
        Weekly = 3,

        /// <summary>
        /// Occurrences for every month
        /// </summary>
        Monthly = 4,

        // Further patterns, like a fully custom one, can go here
        // Keep in mind this pattern designation can be augmented with further data for specificity like "Days of the Week for a Weekly pattern"
    }

    /// <summary>
    /// Enumeration of the various provider types supported by Soundbite
    /// </summary>
    public enum ProviderType
    {
        /// <summary>
        /// Indicates that the provider type is unspecified.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates that the provider type is Azure Active Directory
        /// </summary>
        AAD = 1,

        /// <summary>
        /// Indicates that the provider is Interact
        /// </summary>
        Interact = 100,

        /// <summary>
        /// Indicates that the provider is OKTA
        /// </summary>
        OKTA = 200
    }

    /// <summary>
    /// Enumeration of the various job states.
    /// </summary>
    public enum JobStatusType
    {
        /// <summary>
        ///  Denotes that there is a job status entry but the job is not scheduled for processing.
        /// </summary>
        None,

        /// <summary>
        /// Denotes that the status for the request has been saved but the request has not yet been made.
        /// </summary>
        Requesting,

        /// <summary>
        /// Denotes that a request to run the job has been sent but the job is not yet started.
        /// </summary>
        Requested,

        /// <summary>
        /// Denotes that the job is currently running but is not complete.
        /// </summary>
        Processing,

        /// <summary>
        /// Denotes that the job successfully completed.
        /// </summary>
        Complete,

        /// <summary>
        /// Denotes that the job failed.
        /// </summary>
        Failed
    }

    //TODO: Maybe rename FileType to MediaType type to avoid clashing with the file-type checker + it's media based

    /// <summary>
    /// The file formats supported by the clip system
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// Unknown file type
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Audio files in true MPEG-Layer-3 format; audio-only
        /// </summary>
        Mp3 = 1,

        /// <summary>
        /// Video file in MP4 format
        /// </summary>
        Mp4 = 2,

        /// <summary>
        /// Audio files recorded in browser, which technically are capable of carrying video information
        /// </summary>
        Mpg = 3,

        /// <summary>
        /// Audio file in WAV format; audio-only
        /// </summary>
        Wav = 4,

        /// <summary>
        /// Video file in WEBM format
        /// </summary>
        Webm = 5,

        /// <summary>
        /// Much like mpg, but indicated as music instead of video
        /// </summary>
        MpgAudio = 6,

        /// <summary>
        /// MOV video file.
        /// </summary>
        Mov = 7,

        /// <summary>
        /// MKV video file.
        /// </summary>
        Mkv = 8,

        /// <summary>
        /// WMV video file.
        /// </summary>
        Wmv = 9,

        /// <summary>
        /// AVI video file.
        /// </summary>
        Avi = 10,

        /// <summary>
        /// FLV video file.
        /// </summary>
        Flv = 11,

        /// <summary>
        /// F4v video file.
        /// </summary>
        F4v = 12

        // NOTE: WAVs are NOT available, even though they're native to browsers as well, because they would be too big
    }

    /// <summary>
    /// Enumeration of the various authentication providers supported by Soundbite.
    /// </summary>
    public enum AuthProviderType
    {
        /// <summary>
        /// Unknown auth provider type; usually means value was not set
        /// </summary>
        Unknown = 0,
        //Soundbite = 10,

        /// <summary>
        /// Azure Active Directory, now known as Microsoft Entra
        /// </summary>
        Azure = 20,
        //Google = 30,

        /// <summary>
        /// Favorite auth provider for people who hate Microsoft
        /// </summary>
        Okta = 40,
        //Facebook = 50
    }

    /// <summary>
    /// Enum used to distinguish between a flag that was automatically set by a 
    /// system operation vs a flag explicitly set by a user.
    /// </summary>
    public enum AutoFlagStateType
    {
        /// <summary>
        /// Flag automatically set to disabled
        /// </summary>
        AutoDisabled = -1,

        /// <summary>
        /// Flag set to disabled by user
        /// </summary>
        UserDisabled = 0,

        /// <summary>
        /// Flag automatiaclly set to enabled
        /// </summary>
        AutoEnabled = 1,

        /// <summary>
        /// Flag set to enabled by user
        /// </summary>
        UserEnabled = 2
    }

    /// <summary>
    /// Extension methods for the <see cref="AutoFlagStateType"/> enumeration.
    /// </summary>
    public static class AutoFlagStateTypeExtensions
    {
        /// <summary>
        /// Determines if the state is <c>true</c> regardless of whether it was auto or user set.
        /// </summary>
        /// <param name="autoFlag">Flag whose state is being evaluated.</param>
        /// <returns><c>true</c> if the state is <c>true</c>; otherwise <c>false</c>.</returns>
        public static bool IsTrue(this AutoFlagStateType autoFlag)
        {
            return autoFlag == AutoFlagStateType.AutoEnabled || autoFlag == AutoFlagStateType.UserEnabled;
        }

        /// <summary>
        /// Determines if the state is <c>false</c> regardless of whether it was auto or user set.
        /// </summary>
        /// <param name="autoFlag">Flag whose state is being evaluated.</param>
        /// <returns><c>true</c> if the state is <c>false</c>; otherwise <c>false</c>.</returns>
        public static bool IsFalse(this AutoFlagStateType autoFlag)
        {
            return autoFlag == AutoFlagStateType.AutoDisabled || autoFlag == AutoFlagStateType.UserDisabled;
        }
    }
}
