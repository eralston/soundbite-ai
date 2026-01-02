using Masticore;
using Masticore.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Soundbite.Api.Middleware
{
    /// <summary>
    /// Converts an <see cref="Exception"/> during a request into helpful logs and response body format
    /// </summary>
    [CodeGenModel]
    public class ApiError
    {
        /// <summary>
        /// Reads the error flag state from environment variables
        /// </summary>
        /// <remarks> This is a utility property and does NOT intrinsically set the system to enabled unit you call <see cref="Init(bool)"/></remarks>
        public static bool IsEnvirionmentVariableEnabled { get; } = bool.Parse(Environment.GetEnvironmentVariable("SB_DEVELOPER_ERROR_ENBABLED") ?? "false");

        /// <summary>
        /// A flag indicating if the feature is enabled
        /// </summary>
        public static bool IsDevErrorMode { get; protected set; } = false;

        /// <summary>
        /// Initializes this error system as enabled or disabled
        /// </summary>
        /// <param name="enabled"></param>
        public static void Init(bool enabled)
        {
            IsDevErrorMode = enabled;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="Exception"/>
        /// </summary>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public Exception Exception { get; set; } = new Exception("Unknown Error");

        #endregion

        #region JSON Properties

        /// <summary>
        /// A sentinel property in the JSON to indicate this is an error details object.
        /// </summary>
        /// <remarks>
        /// Check for this property to determine if it's an error details with extra info
        /// </remarks>
        [CodeGenField(IsNullable = true)]
        public bool IsApiError { get; } = true;

        /// <summary>
        /// A sentinel property in the JSON to indicate the message can be shown raw to the user
        /// </summary>
        /// <remarks>
        /// Check for this property to determine if the error's message has anything useful for the user
        /// </remarks>
        [CodeGenField(IsNullable = true)]
        public bool IsUserSafe { get; set; } = false;

        /// <summary>
        /// Gets or sets the HTTP Method (EG, GET)
        /// </summary>
        public string Method { get; set; } = "Unknown";

        /// <summary>
        /// Gets or sets the path for this error's request
        /// </summary>
        public string Path { get; set; } = "Unknown";

        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode"/> for this error
        /// </summary>
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets or sets the error message for this error
        /// </summary>
        public string Message { get; set; } = "Internal server error; unknown details";

        /// <summary>
        /// The name for this error, enabling the client-side type to be compatible with JavaScript's native error; defaults to simply "Error"
        /// </summary>
        public string Name { get; set; } = "Error";

        /// <summary>
        /// Implements JSON serialization
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToLowerCamelJson();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpCtx"></param>
        /// <param name="handler"></param>
        /// <param name="logger"></param>
        public ApiError(HttpContext httpCtx, IExceptionHandlerFeature handler, ILogger logger)
        {
            ExtractDetails(httpCtx, handler);
            LogError(logger);
        }

        /// <summary>
        /// Load up this object with information about the <see cref="Exception"/>
        /// </summary>
        /// <param name="httpCtx"></param>
        /// <param name="handler"></param>
        protected void ExtractDetails(HttpContext httpCtx, IExceptionHandlerFeature handler)
        {
            Path = httpCtx.Request.Path;
            Method = httpCtx.Request.Method;
            Exception = handler.Error;

            // Map based on exception type
            if (Exception is NotFoundException || Exception is ForbiddenException)
            {
                // Obscure lack of access as 404
                Name = "Not Found";
                StatusCode = HttpStatusCode.NotFound;
            }
            else if (Exception is SqlException)
            {
                // SQL Exceptions sometimes come back as 504, so override
                Name = "Database Error";
                StatusCode = HttpStatusCode.InternalServerError;
                Message = "Issue contacting data store";
            }
            else if (Exception is NotImplementedException)
            {
                Name = "Not Implemented";
                StatusCode = HttpStatusCode.NotFound;
                Message = "Missing or disabled functionality";
            }
            else if (Exception is InvalidOperationException)
            {
                Name = "Invalid Operation";
                StatusCode = HttpStatusCode.UnprocessableEntity;
            }
            else if (Exception is BadRequestException)
            {
                Name = "Bad Request";
                StatusCode = HttpStatusCode.BadRequest;
            }
            else if (Exception is ResourceGoneException)
            {
                Name = "Resource Missing";
                StatusCode = HttpStatusCode.Gone;
            }

            // Only expose error message from exception if safe OR we're in dev error mode
            if (Exception is UserSafeException || Exception is InvalidOperationException || IsDevErrorMode)
            {
                IsUserSafe = true;
                Message = Exception.Message;
            }
        }

        /// <summary>
        /// Given an <see cref="ILogger"/>, log the full context of the <see cref="Exception"/> and optionally its inner exception
        /// </summary>
        /// <param name="logger"></param>
        protected void LogError(ILogger logger)
        {
            try
            {
                logger.LogError($"Error {StatusCode} processing {Method} '{Path}' '{Exception.Message}' with trace {Exception.StackTrace}");
                if (Exception.InnerException != null)
                {
                    logger.LogError($"Inner error {Method} '{Path}' '{Exception.InnerException.Message}' with trace {Exception.InnerException.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error trying to {nameof(LogError)} in {nameof(ApiError)} '{ex.Message}' with trace {ex.StackTrace}");
                // Suppress Exception
            }
        }

        /// <summary>
        /// Applies the details of the given <see cref="ApiError"/> to the given <see cref="HttpContext"/>
        /// </summary>
        /// <param name="httpCtx"></param>
        /// <returns></returns>
        public async Task Apply(HttpContext httpCtx)
        {
            httpCtx.Response.StatusCode = (int)StatusCode;
            httpCtx.Response.ContentType = "application/json";
            await httpCtx.Response.WriteAsync(ToString());
        }

        #endregion
    }
}
