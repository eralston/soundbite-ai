using System;

namespace Masticore.Resources
{
    /// <summary>
    /// An object that tracked when access was requested and accepted
    /// </summary>
    [CodeGenModel(Name = "InviteLifecycle")]
    public interface IInviteLifecycle
    {
        /// <summary>
        /// Datetime of the invite send
        /// </summary>
        public DateTime? InviteUtc { get; set; }

        /// <summary>
        /// Datetime of the invite fulfillment
        /// </summary>
        public DateTime? InviteAcceptUtc { get; set; }
    }
}
