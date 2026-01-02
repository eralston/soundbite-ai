using System;

namespace Masticore.Exceptions
{
    /// <summary>
    /// A type of <see cref="Exception"/> that is safe to pass back to the user as-is
    /// </summary>
    public class UserSafeException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// /// <param name="innerException"></param>
        public UserSafeException(string message = "Internal Server Error", Exception innerException = null) : base(message, innerException) { }
    }
}
