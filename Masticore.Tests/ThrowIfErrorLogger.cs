using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Xunit;

namespace Masticore.Tests
{
    /// <summary>
    /// <see cref="ILogger{TCategoryName}"/> intended for unit tests to expose logged errors
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public class ThrowIfErrorLogger<TType> : ILogger<TType>
    {
        public static Dictionary<string, ThrowIfErrorLogger<TType>> _instances = new Dictionary<string, ThrowIfErrorLogger<TType>>();

        /// <summary>
        /// Static instance for any type based on cached instances
        /// </summary>
        public static ThrowIfErrorLogger<TType> Instance
        {
            get
            {
                string key = nameof(TType);
                if (!_instances.ContainsKey(key))
                {
                    _instances[key] = new ThrowIfErrorLogger<TType>();
                }

                return _instances[key];
            }
        }

        public bool DidError { get; protected set; } = false;
        public Exception LastException { get; protected set; } = null;

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel == LogLevel.Error || logLevel == LogLevel.Critical)
            {
                LastException = exception;
                DidError = true;
                throw new Exception($"Error thrown by {nameof(ThrowIfErrorLogger<TType>)}: {exception.Message}");
            }
        }

        public void AssertNoThrow()
        {
            Assert.False(DidError);
        }
    }
}
