namespace Masticore.CodeGen
{
    public static class StringExtensions
    {
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

            return s[0].ToString().ToLower() + s[1..];
        }
    }
}