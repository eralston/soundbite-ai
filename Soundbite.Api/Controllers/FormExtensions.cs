using Microsoft.AspNetCore.Http;
using System;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Extension methods for handling HTTP context
    /// </summary>
    public static class FormExtensions
    {
        /// <summary>
        /// Converts the given form field to the given enum type
        /// </summary>
        /// <typeparam name="TEnum">.NET type of the enum </typeparam>
        /// <param name="formData">Form data from an HTTP request.</param>
        /// <param name="fieldName">Name of the field containing the enum.</param>
        /// <param name="defaultValue">Default value to use if the form data is missing.</param>
        /// <returns>the parsed enum value</returns>
        public static TEnum ToEnum<TEnum>(this IFormCollection formData, string fieldName, TEnum defaultValue)
        {
            string value = formData[fieldName];
            TEnum clipType = string.IsNullOrEmpty(value)
                ? defaultValue
                : (TEnum)Enum.Parse(typeof(TEnum), formData[fieldName].ToString());
            return clipType;
        }

        /// <summary>
        /// Converts the given form field to the given enum type
        /// </summary>
        /// <typeparam name="TEnum">.NET type of the enum </typeparam>
        /// <param name="formData">Form data from an HTTP request.</param>
        /// <param name="fieldName">Name of the field containing the enum.</param>
        /// <returns>the parsed enum value</returns>
        public static TEnum ToEnum<TEnum>(this IFormCollection formData, string fieldName)
        {
            string value = formData[fieldName];
            TEnum clipType = string.IsNullOrEmpty(value)
                ? throw new Exception($"Cannot parse enum from form because field '{fieldName}' is missing.)")
                : (TEnum)Enum.Parse(typeof(TEnum), formData[fieldName].ToString());
            return clipType;
        }

        /// <summary>
        /// Extension for <see cref="IFormCollection"/> that gives back the parsed <see cref="int"/> value of the given field name
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static int ToInt(this IFormCollection formData, string fieldName)
        {
            float floatVal = formData.ToFloat(fieldName);
            int ret = (int)Math.Round(floatVal, 0);
            return ret;
        }

        /// <summary>
        /// Extension for <see cref="IFormCollection"/> that parses the given form field into a <see cref="float"/>
        /// </summary>
        /// <param name="formData"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static float ToFloat(this IFormCollection formData, string fieldName)
        {
            if (!formData.Keys.Contains(fieldName))
            {
                throw new ArgumentNullException($"Field '{fieldName}' is null in form data");
            }

            string rawValue = formData[fieldName].ToString();
            float ret = float.Parse(rawValue);
            return ret;
        }
    }
}
