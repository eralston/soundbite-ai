using Microsoft.Extensions.Logging;
using System;

namespace Soundbite.Api
{
    /// <summary>
    /// Utility class for logging
    /// </summary>
    public static class LogUtils
    {
        /// <summary>
        /// Creates a new <see cref="ILoggerFactory"/> configured for an App Insights connected Azure App
        /// </summary>
        /// <returns></returns>
        public static ILoggerFactory CreateFactory()
        {
            return LoggerFactory.Create((builder) => { builder.AddDebug(); builder.AddConsole(); });
        }

        /// <summary>
        /// Creates a new <see cref="ILogger"/>, based on a one-off <see cref="ILoggerFactory"/> from <see cref="CreateFactory"/>
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public static (ILogger, ILoggerFactory) CreateLogger(string categoryName = "Error")
        {
            ILoggerFactory factory = CreateFactory();
            ILogger logger = factory.CreateLogger(categoryName);
            return (logger, factory);
        }

        /// <summary>
        /// Makes a one-off critical log of the given message and optional <see cref="Exception"/>
        /// </summary>
        /// <remarks>This should only be used in places where using an injected <see cref="ILogger"/> is impractical</remarks>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void LogCritical(string message, Exception exception = null)
        {
            (ILogger logger, ILoggerFactory loggerFactory) = CreateLogger();
            using (loggerFactory)
            {
                logger.LogCritical(exception, message);
            }
        }
    }
}
