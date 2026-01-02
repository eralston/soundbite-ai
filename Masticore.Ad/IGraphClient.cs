using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masticore.Ad
{
    /// <summary>
    /// Defines an object useful for making http requests to outside services
    /// </summary>
    public interface IGraphClient : IDisposable
    {
        Task<string> GetAccessToken();
        Task<HttpClient> GetHttpClient();
        Task<TResponse> Get<TResponse>(string url);
        Task<TResponse> Post<TResponse>(string url, string json);
        void Reset();
    }
}
