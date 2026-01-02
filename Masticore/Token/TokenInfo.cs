namespace Masticore.Token
{
    /// <summary>
    /// Holds the details of an auth token
    /// </summary>
    public class TokenInfo
    {
        /// <summary>
        /// Gets or sets the auth token itself
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the refresh token, which can be used to re-auth and get a new token
        /// </summary>
        public string RefreshToken { get; set; }
    }
}
