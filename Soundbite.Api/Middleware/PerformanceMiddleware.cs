using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Soundbite.Api.Middleware
{
    /// <summary>
    /// A middleware component that measures performance with custom logging
    /// </summary>
    public class PerformanceMiddleware
    {
        // Name of the Response Header, Custom Headers starts with "X-"  
        private const string TimeHeader = "X-Request-Time-sec";
        private const string MemoryHeader = "X-Request-Memory-mb";
        private const string SizeHeader = "X-Response-Size-kb";

        /// <summary>
        /// Then ext <see cref="RequestDelegate"/> in the middleware pipeline
        /// </summary>
        protected RequestDelegate Next { get; }

        /// <summary>
        /// <see cref="ILogger"/> for this middleware
        /// </summary>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets if this middleware is enabled
        /// </summary>
        /// <remarks>
        /// Consider disabling for PROD environments since it's more overhead
        /// </remarks>
        protected bool Enabled { get; }

        /// <summary>
        /// Flag that indicates if headers should return metadate
        /// </summary>
        protected bool EmitHeaders { get; }

        /// <summary>
        /// The <see cref="TelemetryClient"/> instance for this middleware
        /// </summary>
        protected TelemetryClient AppInsights { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        /// <param name="enabled"></param>
        /// <param name="emitHeaders"></param>
        /// <param name="appInsights"></param>
        public PerformanceMiddleware(RequestDelegate next, ILogger logger, bool enabled, bool emitHeaders, TelemetryClient appInsights)
        {
            Logger = logger;
            Next = next;
            Enabled = enabled;
            EmitHeaders = emitHeaders;
            AppInsights = appInsights;
        }

        /// <summary>
        /// Is invoked by ASP.Net MVC to measure the lifecycle of a request
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            string method = context.Request.Method;
            bool shouldBeCaptured = method != HttpMethods.Options && method != HttpMethods.Head;

            // If this isn't enabled OR should not be captured, then we just call next and end
            if (!Enabled || !shouldBeCaptured)
            {
                await Next(context);
                return;
            }
            else
            {
                await Process(context);
            }
        }

        /// <summary>
        /// Processes the given context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task Process(HttpContext context)
        {
            Stream originalStream = context.Response.Body;

            try
            {
                using MemoryStream stream = new MemoryStream();
                context.Response.Body = stream;

                await MeasurePerformance(context, stream);

                // Restore
                stream.Position = 0;
                await stream.CopyToAsync(originalStream);
            }
            finally
            {
                context.Response.Body = originalStream;
            }
        }

        /// <summary>
        /// Measures the performance from the request to response
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        private async Task MeasurePerformance(HttpContext context, MemoryStream stream)
        {
            // BEFORE
            Stopwatch watch = new Stopwatch();
            watch.Start();
            long startBytes = GC.GetTotalMemory(false);

            await Next(context);

            CapturePerformance(context, stream, watch, startBytes);
        }

        /// <summary>
        /// Captures the performance measures for this request into the log and app insights
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stream"></param>
        /// <param name="watch"></param>
        /// <param name="startBytes"></param>
        private void CapturePerformance(HttpContext context, MemoryStream stream, Stopwatch watch, long startBytes)
        {
            // AFTER

            // Time
            watch.Stop();
            string deltaSeconds = ((float)watch.ElapsedMilliseconds / 1000).ToString();

            // Memory
            long endBytes = GC.GetTotalMemory(false);
            string deltaMemoryMb = ((float)(endBytes - startBytes) / 1000000).ToString();

            // Size
            string responseSizeKb = ((float)stream.Length / 1000).ToString();

            // Request info
            PathString path = context.Request.Path;
            string method = context.Request.Method;

            // Log
            string msg = $"Request {method} {path} completed in time {deltaSeconds}s with memory delta {deltaMemoryMb}mB and response size {responseSizeKb}kB";
            Logger.LogDebug(msg);

            // App Insights
            Dictionary<string, string> properties = new Dictionary<string, string>
            {
                ["Path"] = path,
                ["Method"] = method,
                ["DeltaSeconds"] = deltaSeconds,
                ["DeltaMemoryMb"] = deltaMemoryMb,
                ["ResponseSizeKb"] = responseSizeKb
            };
            AppInsights.TrackEvent("RequestPerformance", properties);

            // Headers
            if (EmitHeaders)
            {
                context.Response.Headers[TimeHeader] = deltaSeconds;
                context.Response.Headers[MemoryHeader] = deltaMemoryMb;
                context.Response.Headers[SizeHeader] = responseSizeKb;
            }
        }
    }
}
