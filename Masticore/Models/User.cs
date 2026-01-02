using Masticore.Resources;
using Newtonsoft.Json;
using System;

namespace Masticore.Models
{
    /// <summary>
    /// Represents the core data fields for a user.
    /// </summary>
    /// <seealso cref="ResourceBase" />
    public class User : ResourceBase, IUserFields, IUserNotifications
    {
        #region IUserFields

        /// <summary>
        /// Gets or sets an ID linking the item to an external third-party resource.
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string UniversalId { get; set; }

        /// <summary>
        /// Date the user accepted this invite
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public DateTime? InviteAcceptUtc { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the given/first name of the user.
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// Gets or sets the family/last name of the user.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the user.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the title of the user.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the user principal name
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string Upn { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a flag indicating whether the user wishes to recieve news.
        /// </summary>
        public bool AllowNews { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the user wishes to recieve marketing communication.
        /// </summary>
        public bool AllowMarketing { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the user wishes to recieve emails.
        /// </summary>
        public bool AllowEmail { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether the user wishes to text messages.
        /// </summary>
        public bool AllowSms { get; set; }

        /// <summary>
        /// Gets or sets the application-level role of the user.
        /// </summary>
        public UserRole UserRole { get; set; }

        /// <summary>
        /// Gets or sets the provider used to authenticate the user.
        /// </summary>        
        public ProviderType ProviderType { get; set; }

        /// <summary>
        /// Gets or sets calendar settings for the user.  This is a JSON string value because
        /// different calendar providers may require slightly different settings.
        /// </summary>        
        public string CalendarSettings { get; set; }

        #endregion

        #region Non-Persisted Properties

        /// <summary>
        /// Gets a flag indicating whether there are outstanding invitations for the user.
        /// </summary>
        public bool IsAccepting { get; set; }

        /// <summary>
        /// Gets a URL that points to the "avatar" image associated with the user.  The URL also 
        /// contains a shorted lived token required to acccess the image.  This value should not be 
        /// persisted in third-party systems.
        /// </summary>        
        public string ImageSrc { get; set; }

        #endregion

        /// <summary>
        /// Gets the combined name for this user based on available information
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string DisplayName => this.DisplayName();

    }

    /// <summary>
    /// <see cref="User"/> with optional list of alias emails
    /// </summary>
    public class UserWithAliases : User, IUserFieldsWithAliases
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public UserWithAliases() { }

        /// <summary>
        /// Constructor to load from <see cref="IUserFields"/> object
        /// </summary>
        /// <param name="user"></param>
        public UserWithAliases(IUserFields user)
        {
            // Map from IUserFields to this object
            GivenName = user.GivenName;
            FamilyName = user.FamilyName;
            Email = user.Email;
            Phone = user.Phone;
            Title = user.Title;
            Upn = user.Upn;
            UniversalId = user.UniversalId;
            AliasEmails = new string[0];
        }
        /// <summary>
        /// Gets or sets the array of alias emails for this user
        /// </summary>
        public string[] AliasEmails { get; set; }
    }
}
