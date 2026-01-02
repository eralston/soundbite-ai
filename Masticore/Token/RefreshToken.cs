using Newtonsoft.Json;
using System;

namespace Masticore.Token
{
    /// <summary>
    /// Defines a refresh token that can be used to re-issue an API token to a user.  Refresh tokens
    /// are issued to a user and associated with the user in the database.  When a refresh token is
    /// sent back to the system to refresh an API token, the value of the refresh token is checked
    /// to determine whether it has expired and if it matches the token on file for the user.  If
    /// so, an API token can be issued to the user along with a new refresh token.
    /// </summary>
    public class RefreshToken
    {
        #region Properties

        /// <summary>
        /// Route for the token's users
        /// </summary>
        public string UserRoute { get; set; }

        /// <summary>
        /// Expiration date for the token
        /// </summary>
        public DateTime Expires { get; set; }

        /// <summary>
        /// A random value to make the token unique
        /// </summary>
        public Guid RandomValue { get; set; }

        /// <summary>
        /// Helper to get if the token is currently expired
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsExpired => Expires < Time.UtcNow;

        #endregion

        #region Static Methods

        /// <summary>
        /// Creates a new refresh token 
        /// </summary>
        /// <param name="settings">Token service settings.</param>
        /// <param name="userRoute">Route of the user associated with the refresh token.</param>
        /// <returns>a string-based representation of the refresh token.</returns>
        public static string Create(ITokenServiceSettings settings, string userRoute)
        {
            Validator.ArgNotNull(nameof(settings), settings);
            return $"{userRoute}|{DateTime.UtcNow.AddMinutes(settings.RefreshTokenTimeoutInMinutes).Ticks}|{Guid.NewGuid()}".Base64Encode();
        }

        /// <summary>
        /// Parses a string-based refresh token into a <see cref="RefreshToken"/> instance.
        /// </summary>
        /// <param name="refreshToken">String-based refresh token.</param>
        /// <returns>a <see cref="RefreshToken"/> instance populated with data or <c>null</c> if the token cannot be parsed.</returns>
        public static RefreshToken Parse(string refreshToken)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(refreshToken))
                {
                    RefreshToken result = new RefreshToken();
                    string raw = refreshToken?.Base64Decode();
                    string[] parts = raw?.Split('|');
                    if (parts?.Length == 3)
                    {
                        result.UserRoute = parts[0];
                        result.Expires = new DateTime(long.Parse(parts[1]));
                        result.RandomValue = Guid.Parse(parts[2]);
                    }
                    return result;
                }
            }
            catch
            {
                /* Do nothing */
            }

            return null;
        }

        #endregion
    }
}