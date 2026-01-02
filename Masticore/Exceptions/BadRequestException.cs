using System;

namespace Masticore.Exceptions
{
    /// <summary>
    /// Abstracts out a 400 BAD REQUEST
    /// </summary>
    public class BadRequestException : UserSafeException
    {
        /// <summary>
        /// Constructor with message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public BadRequestException(string message = "Request made to a valid path, but parameters were not understood", Exception innerException = null) : base(message, innerException) { }
    }
}
