using System.ComponentModel.DataAnnotations;

namespace Masticore.Entity
{
    /// <summary>
    /// One of potentially multiple login aliases for a <see cref="UserEntity"/>
    /// </summary>
    /// <remarks>This is in addition to the "main" identity that is held in the <see cref="UserEntity"/> itself</remarks>
    public class UserIdentityEntity : ResourceEntityBase
    {
        // The identifier is not unique across the system since e-mail addresses can reside in more than one directory, though the identifier + provider should be unique


        /// <summary>
        /// Gets or sets a unique identifier for the user (EG, email address, phone number, etc) that combined with the provider is their alias indicated by this identity
        /// </summary>
        [Required]
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the provider used to authenticate the user.
        /// </summary>        
        public ProviderType ProviderType { get; set; }

        // Relationships

        // To-One
        public int UserId { get; set; }
        public virtual UserEntity User { get; set; }
    }
}