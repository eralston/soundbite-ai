using System;
using System.Net;

namespace Masticore.Ad
{
    /// <summary>
    /// A special exception type for the graph client that carries back the status code
    /// </summary>
    public class GraphClientException : Exception
    {
        public GraphClientException(string msg) : base(msg) { }

        public HttpStatusCode? StatusCode { get; set; } = null;
    }
}
