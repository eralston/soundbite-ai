namespace Masticore
{
    /// <summary>
    /// Container for all constants in the "Masticore" projects
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Container for Model constants
        /// </summary>
        public static class Models
        {
            /// <summary>
            /// Container for Group constants
            /// </summary>
            public static class Groups
            {
                /// <summary>
                /// Maximum length of the group name
                /// </summary>
                public const int GroupNameMaxLength = 256;
            }
        }

        /// <summary>
        /// Container for all HttpHeader constants
        /// </summary>
        public static class HttpHeaders
        {
            /// <summary>
            /// Authorization header name
            /// </summary>
            public const string Authorization = "Authorization";
        }

        /// <summary>
        /// Container for all token constants
        /// </summary>
        public static class Tokens
        {
            /// <summary>
            /// Key used to store the organization route claim in a token
            /// </summary>
            public const string OrgRouteClaim = "sbor";

            /// <summary>
            /// Key used to store the audience claim in a token.
            /// </summary>
            public const string AudienceClaim = "aud";

            /// <summary>
            /// Key used to store the user route claim in a token
            /// </summary>
            public const string UserRouteClaim = "sbur";

            /// <summary>
            /// Key used to store the user email claim in a token.
            /// </summary>
            public const string UserEmailClaim = "eml";

            /// <summary>
            /// Key used to store the user universal ID claim in a token
            /// </summary>
            public const string UserUniversalIdClaim = "uuid";

            /// <summary>
            /// Key used to store the organization universal ID claim in a token
            /// </summary>
            public const string OrgUniversalIdClaim = "ouid";

            /// <summary>
            /// Key used to store the issuer claim in a token
            /// </summary>
            public const string IssuerClaim = "iss";
        }

        /// <summary>
        /// Key used by the middleware to identify a user-only org route. The user-only org route
        /// is intended to act as intermediary token when a user has successfully authenticated 
        /// against a third party provider but has not yet selected an organization.
        /// </summary>
        public const string UserOnlyOrgRoute = "usrtkn";

        /// <summary>
        /// Error messages for common synchronization configuration issues.
        /// </summary>
        public static class SyncConfigErrorMessages
        {
            /// <summary>
            /// Error message when config is missing type
            /// </summary>
            public const string MissingType = "Sync Configuration does not have a SyncType value; Provide type parameter";

            /// <summary>
            /// Error message when ync types mismatch
            /// </summary>
            public const string WrongType = "Sync Type value does not match target sync settings; If switching types, provide complete parameters.";

            /// <summary>
            /// Error message when config is missing entirely
            /// </summary>
            public const string MissingConfig = "Sync Configuration settings are missing; Try setting config parameters";

            /// <summary>
            /// Error message for bad JSON
            /// </summary>
            public const string InvalidJson = "Sync Configuration is unreadable; Try resetting the configuration";
        }
    }
}
