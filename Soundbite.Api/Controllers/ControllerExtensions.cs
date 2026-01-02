using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Extensions to support <see cref="ControllerBase"/> and friends
    /// </summary>
    public static class ControllerExtensions
    {
        /// <summary>
        /// Converts the body of the given requests to a stream
        /// WARNIING: This should NOT be used for large files
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<MemoryStream> ToStream(this HttpRequest request)
        {
            MemoryStream stream = new MemoryStream(2048);
            await request.Body.CopyToAsync(stream);
            return stream;
        }

        /// <summary>
        /// Responsible for parsing out the bearer token from the Authorization HTTP header.
        /// </summary>
        /// <returns>a string containing the bearer token value.</returns>
        public static string GetBearerToken(this HttpRequest request)
        {
            string result = null;
            if (request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues authHeaderValue))
            {
                result = authHeaderValue[0].Length > 7 ? authHeaderValue[0][7..] : null;
            }
            return result;
        }
    }
}
