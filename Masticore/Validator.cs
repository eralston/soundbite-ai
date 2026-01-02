using Masticore.Exceptions;
using Masticore.Security;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;

namespace Masticore
{
    /// <summary>
    /// Helper class that provides consistent messaging and simplified syntax for validation.
    /// </summary>
    public static class Validator
    {
        #region Methods - Arguments

        /// <summary>
        /// Validates that an argument is not <c>null</c>.
        /// </summary>
        /// <param name="name">Name of the argument (use nameof({arg}) to ensure name matches argument name)</param>
        /// <param name="value">Value associated with the agrument.</param>
        /// <param name="message">(Optional) Message associated with the exception when the argument is null. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void ArgNotNull(string name, [NotNull] object value, string message = null, params object[] messageArg)
        {
            if (value == null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = $"{name} parameter cannot be null.";
                }

                throw new ArgumentException(name, string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Valies that an arguments is not <c>null</c> or zero length
        /// </summary>
        /// <param name="name">Name of the argument (use nameof({arg}) to ensure name matches argument name)</param>
        /// <param name="value">Value associated with the agrument.</param>
        /// <param name="message">(Optional) Message associated with the exception when the argument is null. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void ArgNotNullOrEmpty(string name, [NotNull] string value, string message = null, params object[] messageArg)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = $"{name} parameter cannot be null.";
                }

                throw new ArgumentException(name, string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Validates that an argument is not <c>null</c>.
        /// </summary>
        /// <param name="name">Name of the item (use nameof({item})</param>
        /// <param name="value">Value associated with the item.</param>
        /// <param name="message">(Optional) Message associated with the exception when the item is null. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void NotNull(string name, [NotNull] object value, string message = null, params object[] messageArg)
        {
            if (value == null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = $"{name} cannot be null.";
                }

                throw new NullReferenceException(string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Validates that an argument is not <c>null</c> or empty.
        /// </summary>
        /// <param name="name">Name of the item (use nameof({item})</param>
        /// <param name="value">Value associated with the item.</param>
        /// <param name="message">(Optional) Message associated with the exception when the item is null. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void NotNullOrEmpty(string name, [NotNull] string value, string message = null, params object[] messageArg)
        {
            if (value == null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = $"{name} cannot be null.";
                }

                throw new Exception(string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Validates that an argument is not <c>null</c> or whitespace
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="message"></param>
        /// <param name="messageArg"></param>
        /// <exception cref="Exception"></exception>
        public static void NotNullOrWhitespace(string name, [NotNull] string value, string message = null, params object[] messageArg)
        {
            if (value == null)
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = $"{name} cannot be null or whitespace.";
                }

                throw new Exception(string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Validates that a string argument is not <c>null</c> or <see cref="string.Empty"/>.
        /// </summary>
        /// <param name="name">Name of the argument (use nameof({arg}) to ensure name matches argument name)</param>
        /// <param name="value">Value associated with the agrument.</param>
        /// <param name="message">(Optional) Message associated with the exception when the argument is null or empty. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void ArgNotNull(string name, [NotNull] string value, string message = null, params object[] messageArg)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = $"{name} parameter cannot be null or empty.";
                }

                throw new ArgumentException(name, string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Validates a parameter
        /// </summary>
        /// <typeparam name="T">.NET type of the parameter to validate.</typeparam>
        /// <param name="name">Name of the argument (use nameof({arg}) to ensure name matches argument name)</param>
        /// <param name="value">Value associated with the agrument.</param>
        /// <param name="predicate">Predicate used to test the argument for validitiy. Return <c>true</c> for valid and <c>false</c> for invalid.</param>
        /// <param name="message">(Optional) Message associated with the exception when the argument is null or empty. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void ArgValidate<T>(string name, T value, Func<T, bool> predicate, string message, params object[] messageArg)
        {
            if (!predicate(value))
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = $"{name} parameter is invalid.";
                }

                throw new ArgumentException(name, string.Format(message, messageArg));
            }
        }

        #endregion

        #region Methods - Other

        /// <summary>
        /// Asserts that the given <see cref="ISecurityContext"/> has a valid tenant
        /// </summary>
        /// <param name="securityContext"></param>
        public static void SecurityContextHasTenant(ISecurityContext securityContext)
        {
            if (securityContext == null || securityContext.CurrentTenantId <= 0)
            {
                throw new SecurityException("Security context is not associated with a tenant.");
            }
        }

        /// <summary>
        /// Asserts that the given <see cref="ISecurityContext"/> has a valid UniversalId
        /// </summary>
        /// <param name="securityContext"></param>
        public static void SecurityContextUniversalIdRequired(ISecurityContext securityContext)
        {
            if (securityContext == null || string.IsNullOrEmpty(securityContext.UniversalId))
            {
                throw new SecurityException("Security context must have a universal id.");
            }
        }

        /// <summary>
        /// Validates that an argument is not <c>null</c>.
        /// </summary>
        /// <param name="value">Value associated with the argument.</param>
        /// <param name="message">(Optional) Message associated with the exception when the argument is null. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void NotNull([NotNull] object value, string message, params object[] messageArg)
        {
            if (value == null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = $"Value cannot be null.";
                }

                throw new Exception(string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Validates that a string argument is not <c>null</c> or <see cref="string.Empty"/>.
        /// </summary>
        /// <param name="value">Value associated with the agrument.</param>
        /// <param name="message">(Optional) Message associated with the exception when the argument is null or empty. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void NotNull([NotNull] string value, string message, params object[] messageArg)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = $"Value cannot be null or empty.";
                }

                throw new Exception(string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Validates a parameter
        /// </summary>
        /// <typeparam name="T">.NET type of the parameter to validate.</typeparam>
        /// <param name="value">Value associated with the agrument.</param>
        /// <param name="predicate">Predicate used to test the argument for validitiy. Return <c>true</c> for valid and <c>false</c> for invalid.</param>
        /// <param name="message">(Optional) Message associated with the exception when the argument is null or empty. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void Validate<T>(T value, Func<T, bool> predicate, string message, params object[] messageArg)
        {
            if (!predicate(value))
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "Value is invalid.";
                }

                throw new Exception(string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Validates that a value is true.
        /// </summary>
        /// <param name="value">Value associated with the agrument.</param>
        /// <param name="message">(Optional) Message associated with the exception when the argument is null or empty. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void IsTrue(bool value, string message, params object[] messageArg)
        {
            if (!value)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "Value was not true.";
                }

                throw new Exception(string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Validates that a value is false.
        /// </summary>
        /// <param name="value">Value associated with the agrument.</param>
        /// <param name="message">(Optional) Message associated with the exception when the argument is null or empty. String may contain formatting tokens.</param>
        /// <param name="messageArg">String format values.</param>
        public static void IsFalse(bool value, string message, params object[] messageArg)
        {
            if (!value)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "Value was not false.";
                }

                throw new Exception(string.Format(message, messageArg));
            }
        }

        /// <summary>
        /// Determines whether the specified item is null and if so throws a <see cref="NotFoundException"/>.
        /// </summary>
        /// <typeparam name="T">.NET type of the item to check.</typeparam>
        /// <param name="item">Item to check.</param>
        /// <param name="message">Message to display in the exception if the item is <c>null</c>.</param>
        /// <param name="messageArgs">String format values.</param>
        public static void EnsureFound<T>(T item, string message, params object[] messageArgs)
            where T : class
        {
            if (item == null)
            {
                if (string.IsNullOrEmpty(message))
                {
                    message = "Item was not found";
                }

                throw new NotFoundException(string.Format(message, messageArgs));
            }
        }

        #endregion
    }
}