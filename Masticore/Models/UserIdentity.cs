namespace Masticore.Models
{
    /// <summary>
    /// Represents one of many possible identities for a user, which enables the same person to have multiple logins (EG, different alt e-mails, different providers, etc)
    /// </summary>
    /// <seealso cref="ResourceBase" />
    public class UserIdentity : ResourceBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets a unique identifier for the user (EG, email address, phone number, etc) that combined with the provider is their alias indicated by this identity
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the provider used to authenticate the user.
        /// </summary>        
        public ProviderType ProviderType { get; set; }

        #endregion

        #region Relationships

        /// <summary>
        /// Gets or sets a reference to the underlying user
        /// </summary>        
        public User User { get; set; }

        #endregion
    }
}
