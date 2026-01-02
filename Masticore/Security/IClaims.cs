using Masticore.Models;
using Masticore.Resources;

namespace Masticore.Security
{
    /// <summary>
    /// Describes an object that provides information from the logged in user directly from the authorization of the user (EG, No Graph)
    /// WARNING: The combination of UniversalId and Email can for the same human under vary under certain circumstances
    /// such as when the user is deleted from the directory then re-populated with the same email (EG, re-hire)
    /// or if the email changes (EG, name change)
    /// </summary>
    public interface IClaims : IUniversal
    {
        /// <summary>
        /// Gets the email for this user
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Gets the fullname of the user
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets the unique ID for the directory authenticating this user
        /// </summary>
        string DirectoryUniversalId { get; }

        /// <summary>
        /// Gets the provider type for this set of claims
        /// </summary>
        ProviderType ProviderType { get; }
    }

    /// <summary>
    /// Extension methods for <see cref="IClaims"/>
    /// </summary>
    public static class ClaimExtensions
    {
        /// <summary>
        /// Assuming a western [firstName] [lastName] structure of space-delimited names, put them into the user field names
        /// </summary>
        /// <param name="claims"></param>
        /// <param name="userFields"></param>
        public static void ApplyWesternName(this IClaims claims, IUserFields userFields)
        {
            if (string.IsNullOrEmpty(claims.FullName))
            {
                return;
            }

            string[] nameParts = claims.FullName.Split(' ');
            if (nameParts.Length == 1)
            {
                userFields.GivenName = nameParts[0];
            }
            else if (nameParts.Length > 1)
            {
                userFields.GivenName = nameParts[0];
                userFields.FamilyName = nameParts[^1];
            }
        }
    }
}