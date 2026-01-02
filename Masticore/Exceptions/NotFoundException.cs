using System;

namespace Masticore.Exceptions
{
    /// <summary>
    /// Abstracts a 404 NOT FOUND into something that is safe to show a user
    /// </summary>
    public class NotFoundException : UserSafeException
    {
        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public NotFoundException(string message = "No User for Claims", Exception innerException = null) : base(message, innerException) { }
    }
}
