using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace Soundbite.Api.Middleware
{
    /// <summary>
    /// Extension methods interacting with <see cref="IApplicationBuilder"/> to add middleware components
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds a middleware handler that transform and logs all exceptions
        /// </summary>
        /// <param name="app"></param>
        /// <param name="logger"></param>
        public static void UseSbErrorDetails(this IApplicationBuilder app, ILogger logger)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async httpCtx =>
                {
                    try
                    {
                        IExceptionHandlerFeature handler = httpCtx.Features.Get<IExceptionHandlerFeature>();
                        if (handler != null && handler.Error != null)
                        {
                            ApiError error = new ApiError(httpCtx, handler, logger);
                            await error.Apply(httpCtx);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error trying to handle error in {nameof(UseSbErrorDetails)} '{ex.Message}' with stacktrace {ex.StackTrace}");
                    }
                });
            });
        }

        /// <summary>
        /// Adds <see cref="PerformanceMiddleware"/> to the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <param name="logger"></param>
        /// <param name="env"></param>
        public static void UsePerfLogging(this IApplicationBuilder app, ILogger logger, IWebHostEnvironment env)
        {
            bool isEnabled = true;
            bool shouldEmitHeaders = env.IsDevelopment() || env.IsEnvironment(Startup.EnvTest) || env.IsEnvironment(Startup.EnvNgrok);
            app.UseMiddleware(typeof(PerformanceMiddleware), logger, isEnabled, shouldEmitHeaders);
        }
    }
}
