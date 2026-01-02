using Masticore.Resources;
using System;

namespace Masticore.Models
{
    /// <summary>
    /// Represents the association of a user to an organization.
    /// </summary>
    public class Person : ResourceBase, IInviteLifecycle
    {
        #region IInvited Implementation

        /// <summary>
        /// Datetime of the invite send
        /// </summary>
        public DateTime? InviteUtc { get; set; }

        /// <summary>
        /// Datetime of the invite fulfillment
        /// </summary>
        public DateTime? InviteAcceptUtc { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the organization-level role of the user.
        /// </summary>
        public PersonRole PersonRole { get; set; }

        #endregion

        #region Relationships Properties

        /// <summary>
        /// Gets or sets a reference to the underlying user
        /// </summary>        
        public User User { get; set; }

        #endregion
    }
}
