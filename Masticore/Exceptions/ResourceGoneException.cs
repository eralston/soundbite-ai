using System;

namespace Masticore.Exceptions
{
    /// <summary>
    /// Abstracts a 410 GONE for a resource that is missing from the system; like a <see cref="NotFoundException"/>, but for people who credibly might have had access to it if it existed
    /// </summary>
    /// <remarks>This would be most appropriate if the user would have had correct access to an entity, but it was soft-deleted</remarks>
    public class ResourceGoneException : UserSafeException
    {
        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ResourceGoneException(string message = "Could not find entity specified by the request", Exception innerException = null) : base(message, innerException) { }
    }
}
