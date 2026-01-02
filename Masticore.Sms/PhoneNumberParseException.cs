using System;

namespace Masticore.Sms
{
    /// <summary>
    /// <see cref="Exception"/> class for when <see cref="PhoneNumber"/> cannot process a number
    /// </summary>
    public class PhoneNumberParseException : Exception
    {
        /// <summary>
        /// Constructor that passes around the given message
        /// </summary>
        /// <param name="msg"></param>
        public PhoneNumberParseException(string msg) : base(msg) { }
    }
}
