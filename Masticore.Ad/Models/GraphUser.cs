using Masticore.Models;
using Masticore.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Masticore.Ad
{
    /// <summary>
    /// Represents a user from <seealso href="https://developer.microsoft.com/en-us/graph/graph-explorer">MS Graph</seealso>
    /// </summary>
    public class GraphUser : GraphBase, IUserFieldsWithAliases
    {
        public const string SelectFields = "id,surname,givenName,jobTitle,businessPhones,mobilePhone,mail,deletedDateTime,displayName,userPrincipalName,proxyAddresses";
        public static readonly string SelectClause = $"$select={SelectFields}";
        public const string FilterCriteria = "(userType eq 'Member' or userType eq 'Guest')";

        public const string UserDataType = "#microsoft.graph.user";
        public const string AllUsersUrl = "https://graph.microsoft.com/v1.0/users";
        public static readonly string AllUsersDeltaUrl = $"https://graph.microsoft.com/v1.0/users/delta?{SelectClause}";
        public const string FirstUserUrl = "https://graph.microsoft.com/v1.0/users?$top=1&$select=id";

        public const string AllUserTargetUrl = "https://graph.microsoft.com/v1.0/users?$select=id,mail,deletedDateTime&$filter=userType eq 'Member' or userType eq 'Guest'";

        /// <summary>
        /// Returns a url for finding the given user by user principal e-mail (EG, via claims)
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static string UserUrlByEmail(string email)
        {
            if (!email.IsEmail())
            {
                return null;
            }
            return $"{AllUsersUrl}/{email}";
        }

        /// <summary>
        /// Creates a $filter clause for the given universalId of a user
        /// </summary>
        /// <param name="universalId"></param>
        /// <returns></returns>
        public static string FilterById(string universalId)
        {
            return $"id eq '{universalId}'";
        }

        /// <summary>
        /// When appended to a <see cref="GraphGroup"/> URL, pulls out the users for the given group
        /// </summary>
        public const string MembersUrlSuffix = "/members";

        [JsonProperty("BusinessPhones")]
        public string[] BusinessPhones { get; set; }

        [JsonIgnore]
        public string Phone
        {
            get => MobilePhone ?? BusinessPhones?.FirstOrDefault();
            set => throw new NotImplementedException(nameof(Phone));
        }

        [JsonProperty("givenName")]
        public string GivenName { get; set; }

        [JsonProperty("surname")]
        public string FamilyName { get; set; }

        [JsonProperty("mail")]
        public string Email { get; set; }

        [JsonProperty("jobTitle")]
        public string Title { get; set; }

        [JsonProperty("mobilePhone")]
        public string MobilePhone { get; set; }

        [JsonProperty("userPrincipalName")]
        public string Upn { get; set; }

        [JsonProperty("proxyAddresses")]
        public string[] ProxyEmails { get; set; }

        [JsonIgnore]
        public override bool IsActiveAndValid => Email != null && base.IsActiveAndValid;

        /// <summary>
        /// Return each unique email associated with this user
        /// </summary>
        /// <returns></returns>
        public string[] AliasEmails
        {
            get
            {
                HashSet<string> emails = new HashSet<string>();

                // Optionally include main email
                string mainEmail = Email.ToEmail(false);
                if (!string.IsNullOrEmpty(mainEmail))
                {
                    emails.Add(mainEmail);
                }

                // Optionally include the proxy emails
                if (ProxyEmails != null && ProxyEmails.Length > 0)
                {
                    ProxyEmails
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Select(e => e.ToLower().Replace("smtp:", ""))
                    .Where(email => email.IsEmail())
                    .ForEach(e => emails.Add(e));
                }

                return emails.ToArray();
            }
        }
    }
}
