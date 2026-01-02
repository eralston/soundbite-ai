using System;

namespace Masticore.Exceptions
{
    /// <summary>
    /// Abstracts a 509 CONFLICT "The requested resource could not be found but may be available in the future. Subsequent requests by the client are permissible.
    /// </summary>
    /// <remarks>This would be most appropriate if the user would have had correct access to an entity, but it was soft-deleted</remarks>
    public class ConflictException : UserSafeException
    {
        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ConflictException(string message = "Request could not be processed because of conflict in the current state of the resource", Exception innerException = null) : base(message, innerException) { }
    }
}
