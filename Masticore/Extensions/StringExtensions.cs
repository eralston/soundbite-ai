using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Masticore
{
    /// <summary>
    /// Contains extension methods for string types
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Ensures that a null value is returned if the value is empty.
        /// </summary>
        /// <param name="value">Value of the string.</param>
        /// <param name="trim">Flag indicating whether to trim the string (default is true)</param>
        /// <returns>the value or null if the value is empty</returns>
        public static string NullIfEmpty(this string value, bool trim = true)
        {
            if (string.IsNullOrEmpty(trim ? value?.Trim() : value))
            {
                return null;
            }
            return value;
        }

        /// <summary>
        /// Determines an appropriate <see cref="FileType"/> from the given path string.
        /// </summary>
        /// <param name="s">String containing a file path used to determine the <see cref="FileType"/>.</param>
        /// <returns>a <see cref="FileType"/> value for the specified path or <see cref="FileType.Unknown"/> if a determination cannot be made.</returns>
        public static FileType GetFileTypeFromExtension(this string s)
        {
            if (!string.IsNullOrEmpty(s))
            {
                switch (Path.GetExtension(s).ToLower())
                {
                    case ".avi": return FileType.Avi;
                    case ".f4v": return FileType.F4v;
                    case ".flv": return FileType.Flv;
                    case ".mkv": return FileType.Mkv;
                    case ".mov": return FileType.Mov;
                    case ".mp3": return FileType.Mp3;
                    case ".mp4": return FileType.Mp4;
                    case ".mpeg": return FileType.Mp4;
                    case ".mpg": return FileType.Mpg;
                    case ".wav": return FileType.Wav;
                    case ".webm": return FileType.Webm;
                    case ".wmv": return FileType.Wmv;
                }
            }

            return FileType.Unknown;
        }

        /// <summary>
        /// Truncates the specified string at the specified length.
        /// </summary>
        /// <param name="s">string on which to operate</param>
        /// <param name="maxLength">Max length of string after which to truncate.</param>
        /// <returns>A string that is no longer than the specified <paramref name="maxLength"/>.</returns>
        public static string Truncate(this string s, int maxLength)
        {
            return s?.Length > maxLength ? s.Substring(0, maxLength) : s;
        }

        /// <summary>
        /// Returns the specified string with the first letter lowercased.
        /// </summary>
        /// <param name="s">string whose first letter should be lowercased.</param>
        /// <returns>The string with the first letter lowercased.</returns>
        public static string LowerFirstChar(this string s)
        {
            if (string.IsNullOrEmpty(s)) { return s; }
            if (s.Length == 1)
            {
                return s.ToLower();
            }

            return s[0].ToString().ToLower() + s.Substring(1);
        }

        /// <summary>
        /// Encodes a string to Base64
        /// </summary>
        /// <param name="s">String to encode</param>
        /// <returns>Base64 encoded version of the string (returns null/empty if incomming string is null/empty)</returns>
        public static string Base64Encode(this string s)
        {
            string result = s;
            if (!string.IsNullOrEmpty(s))
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
                result = System.Convert.ToBase64String(bytes);
            }
            return result;
        }

        /// <summary>
        /// Encodes a string to Base64
        /// </summary>
        /// <param name="s">String to encode</param>
        /// <returns>Base64 encoded version of the string (returns null/empty if incomming string is null/empty)</returns>
        public static string Base64Decode(this string s)
        {
            string result = s;
            if (!string.IsNullOrEmpty(s))
            {
                byte[] bytes = System.Convert.FromBase64String(s);
                result = ASCIIEncoding.ASCII.GetString(bytes);
            }
            return result;
        }

        /// <summary>
        /// Ensures that the string ends with the specified ending. 
        /// </summary>
        /// <param name="s">String whose ending is being ensured.</param>
        /// <param name="ending">Ending to ensure on the string.</param>
        /// <returns>A string that ends with the specified ending.</returns>
        public static string EnsureEndsWith(this string s, string ending)
        {
            s = s == null ? "" : s;
            return !s.EndsWith(ending) ? s + ending : s;
        }

        /// <summary>
        /// Creates a memory stream containing the string.
        /// </summary>
        /// <param name="s">String to place into a memory stream.</param>
        /// <returns>a memory stream containing the string that is ready to be read from position 0.</returns>
        public static MemoryStream ToMemoryStream(this string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Converts the given string to its string representation
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static Task<string> ToStringContent(this Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEndAsync();
        }

        /// <summary>
        /// Replaces the file extension in the string with the specified extension.
        /// </summary>
        /// <param name="s">String in which to replace the file extension.</param>
        /// <param name="newExtension">Replacemtn extension value to apply.</param>
        /// <param name="removeDot">Flag indicating whether to remove the file extension dot along with the extension (true) or leave the dot (false)</param>
        /// <returns>a string with the extension replaced.</returns>
        public static string ReplaceExtension(this string s, string newExtension, bool removeDot = false)
        {
            string result = s;
            if (!string.IsNullOrEmpty(s))
            {
                result = s.Substring(0, s.Length - Path.GetExtension(s).Length + (removeDot ? 0 : 1))
                + newExtension;
            }
            return result;



        }
    }
}
