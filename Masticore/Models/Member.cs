using Masticore.Resources;
using System;

namespace Masticore.Models
{
    /// <summary>
    /// Represents the association of a <see cref="Person"/> to a <see cref="Group"/>.
    /// </summary>
    /// <seealso cref="Masticore.Models.ResourceBase" />
    /// <seealso cref="Masticore.Resources.IInviteLifecycle" />
    public class Member : ResourceBase, IInviteLifecycle
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
        /// Gets or sets the group-level role of the user.
        /// </summary>
        public MemberRole MemberRole { get; set; }

        /// <summary>
        /// Gets or sets the route of the person associated with the group.
        /// </summary>
        public string PersonRoute { get; set; }

        /// <summary>
        /// Gets or sets the route of the group to which the person is associated.
        /// </summary>
        public string GroupRoute { get; set; }

        #endregion

        #region Relationship Properties

        /// <summary>
        /// The <see cref="Person"/> record participanting in this membership
        /// </summary>
        public Person Person { get; set; }

        /// <summary>
        /// The <see cref="Group"/> participanting in this membership
        /// </summary>
        public Group Group { get; set; }

        #endregion
    }
}