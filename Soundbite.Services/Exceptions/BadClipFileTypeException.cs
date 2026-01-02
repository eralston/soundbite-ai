using Masticore.Exceptions;

namespace Soundbite.Services
{
    /// <summary>
    /// An exception for unacceptable file types being passed to ClipService
    /// </summary>
    public class BadClipFileTypeException : UserSafeException
    {
        public BadClipFileTypeException(string msg) : base(msg) { }
    }
}
