using Masticore.Resources;

namespace Masticore.Models
{
    /// <summary>
    /// Interface defining the fields of a User
    /// </summary>
    [CodeGenModel(Name = "UserFields")]
    public interface IUserFields : IUniversal
    {
        /// <summary>
        /// Gets or sets the user principal name
        /// </summary>
        string Upn { get; set; }

        /// <summary>
        /// Gets or sets the system-wide unique email address
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Gets or sets the name of their family (in western cultures, this is the last name)
        /// </summary>
        string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets their given name (in western cultures, this is the first name)
        /// </summary>
        string GivenName { get; set; }

        /// <summary>
        /// Gets or sets their primary phone number
        /// </summary>
        string Phone { get; set; }

        /// <summary>
        /// Gets or sets their job title
        /// </summary>
        string Title { get; set; }
    }
}