using Masticore.Resources;
using System;

namespace Masticore
{
    /// <summary>
    /// Complete general-purpose extensions for existing C# Classes
    /// </summary>
    public static class MasticoreExtensions
    {
        /// <summary>
        /// Converts the given string to a redacted e-mail address if it really is an email address
        /// </summary>
        /// <remarks>Conceals PII while enabling easy logging</remarks>
        /// <param name="possibleEmail"></param>
        /// <returns></returns>
        public static string RedactEmail(this string possibleEmail)
        {
            if (possibleEmail.IsEmail())
            {
                string maskedEmail = string.Format(format: "{0}*{1}",
                possibleEmail.Substring(0, 1),
                possibleEmail[(possibleEmail.IndexOf('@') - 1)..]);
                return maskedEmail;
            }
            else
            {
                return possibleEmail;
            }
        }

        /// <summary>
        /// Returns the last N number of the characters in the given string, defaulting to 4
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lastNumber"></param>
        /// <returns></returns>
        public static string LastChars(this string value, int lastNumber = 4)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }
            else if (value.Length < lastNumber)
            {
                return value;
            }
            else
            {
                return value.Substring(value.Length - lastNumber);
            }
        }

        /// <summary>
        /// Retrieves a bool from the given <see cref="Environment"/> by the given name and falling back to the given default value
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static bool GetEnvBool(string variableName, bool defaultValue)
        {
            string defaultValueName = defaultValue ? "true" : "false";
            return bool.Parse(Environment.GetEnvironmentVariable(variableName) ?? defaultValueName);
        }

        /// <summary>
        /// Retrieves a parsed Enum from the <see cref="Environment"/> using the given fallback value
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="variableName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TEnum GetEnvEnum<TEnum>(string variableName, TEnum defaultValue)
            where TEnum : Enum
        {
            string valueString = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrEmpty(valueString))
            {
                return defaultValue;
            }
            else
            {
                TEnum clipType = (TEnum)Enum.Parse(typeof(TEnum), valueString);
                return clipType;
            }
        }
    }
}
