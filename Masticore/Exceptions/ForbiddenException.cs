using System;

namespace Masticore.Exceptions
{
    /// <summary>
    /// <see cref="Exception"/> for expressing the equivalent of a 401 forbidden error, but within services that are host agnostic
    /// </summary>
    public class ForbiddenException : UserSafeException
    {
        /// <summary>
        /// Constructor with message argument
        /// </summary>
        /// <param name="message"></param>
        public ForbiddenException(string message = "No access to resource") : base(message) { }
    }
}
