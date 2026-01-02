using System.Text.RegularExpressions;

namespace Masticore.Mail
{
    public static class SendGridExtensions
    {
        /// <summary>
        /// Given an email string, redacts the content to match the pattern e*****n@address.com
        /// </summary>
        /// <param name="emailAddr"></param>
        /// <returns></returns>
        public static string RedactEmail(string emailAddr)
        {
            string pattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{1}@)";
            string result = Regex.Replace(emailAddr, pattern, m => new string('*', m.Length));
            return result;
        }

        public static string Redact(this Address emailAddr)
        {
            return RedactEmail(emailAddr.Email);
        }
    }
}
