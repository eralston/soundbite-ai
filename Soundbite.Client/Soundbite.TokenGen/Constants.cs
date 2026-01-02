namespace Soundbite.TokenGen
{
    /// <summary>
    /// Container for all constants in the "Soundbite" Client SDK
    /// </summary>
    public static class Constants
    {
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
    }
}