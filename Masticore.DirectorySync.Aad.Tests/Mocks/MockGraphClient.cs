using Masticore.Ad;
using Masticore.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masticore.DirectorySync.Aad.Tests
{
    public class MockGraphClient : IGraphClient
    {
        public int GetCount { get; protected set; } = 0;
        public int PostCount { get; protected set; } = 0;

        /// <summary>
        /// The list of groups returns when looking 
        /// </summary>
        public GraphGroup[] Groups { get; set; } = new GraphGroup[] { };

        /// <summary>
        /// Gets the mapping from Group ID to group member graphUser list
        /// </summary>
        public Dictionary<string, GraphUser[]> Members { get; set; }
        public GraphUser[] Users { get; set; } = new GraphUser[] { };

        #region IGraphClient

        public Task<string> GetAccessToken()
        {
            return Task.FromResult("FAKE_TOKEN");
        }

        public Task<HttpClient> GetHttpClient()
        {
            return null;
        }

        public Task<T> Get<T>(string url)
        {
            ++GetCount;

            // Get host part (host name or address and port). Returns "server:8080".
            // string hostpart = uri.Authority;

            // Get path and query string parts. Returns "/func2/SubFunc2?query=somevalue".
            // string pathpart = uri.PathAndQuery;

            // Get path components. Trailing separators. Returns { "/", "func2/", "sunFunc2" }.

            // Get query string. Returns "?query=somevalue".
            // string querystring = uri.Query;

            // Looking for members
            if (url.EndsWith("members?" + GraphUser.SelectClause))
            {
                string groupId = GetSegment(url, 3);
                GraphUser[] users = Members[groupId];
                return ToReturn<T>(users);
            }
            else if (
                url.StartsWith(GraphUser.AllUsersUrl) ||
                url == GraphUser.AllUsersDeltaUrl ||
                url == GraphUser.AllUserTargetUrl ||
                url == GraphUser.FirstUserUrl ||
                (url.StartsWith(GraphGroup.AllGroupsUrl) &&
                    url.Contains(GraphUser.MembersUrlSuffix))
                )
            {
                return ToReturn<T>(Users);
            }
            else if (
                url == GraphGroup.AllGroupsDeltaUrl ||
                url == GraphGroup.AllGroupsTargetUrl ||
                url.StartsWith(GraphGroup.AllGroupsUrl))
            {
                return ToReturn<T>(Groups);
            }
            else
            {
                throw new UserSafeException($"Unrecognized GET URL in MockGraphClient: {url}");
            }
        }

        /// <summary>
        /// Gets the given segment in the given url
        /// NOTE: the 0 segment is the "/" in the URL, so go 1 more than you think you need
        /// </summary>
        /// <param name="url"></param>
        /// <param name="segment"></param>
        /// <returns></returns>
        private static string GetSegment(string url, int segment)
        {
            Uri uri = new Uri(url);
            string groupId = uri.Segments[segment];
            groupId = groupId.Substring(0, groupId.LastIndexOf('/'));
            return groupId;
        }

        private Task<T> ToReturn<T>(GraphUser[] users)
        {
            GraphODataContext<GraphUser> ctx = new GraphODataContext<GraphUser> { Value = users };
            return Task.FromResult(Convert<T>(ctx));
        }

        private Task<T> ToReturn<T>(GraphGroup[] groups)
        {
            GraphODataContext<GraphGroup> ctx = new GraphODataContext<GraphGroup> { Value = groups };
            return Task.FromResult(Convert<T>(ctx));
        }

        public Task<T> Post<T>(string url, string json)
        {
            ++PostCount;
            // Check if it's an OID request by ID
            string responseJson = OidResponseJson(url, json);
            if (responseJson == null)
            {
                throw new Exception($"Unrecognized POST body in MockGraphClient for URL {url}; May be a bug, but maybe also need more mock data examples");
            }

            T response = JsonConvert.DeserializeObject<T>(responseJson);
            return Task.FromResult<T>(response);
        }

        public void Reset()
        {
        }

        public void Dispose()
        {
        }

        #endregion

        protected string OidResponseJson(string url, string json)
        {
            if (url != OidRequest.OidRequestUrl)
            {
                return null;
            }

            OidRequest oidRequest = OidRequest.Deserialize(json);
            if (oidRequest.types[0] == OidRequest.Groups)
            {
                GraphODataContext<GraphGroup> ctx = new GraphODataContext<GraphGroup>
                {
                    NextLink = null,
                    DeltaLink = null,
                    Value = Groups
                };
                return JsonConvert.SerializeObject(ctx);
            }
            else if (oidRequest.types[0] == OidRequest.Users)
            {
                GraphODataContext<GraphUser> ctx = new GraphODataContext<GraphUser>
                {
                    NextLink = null,
                    DeltaLink = null,
                    Value = Users
                };
                return JsonConvert.SerializeObject(ctx);
            }

            return null;
        }

        /// <summary>
        /// Converts the given object to the given type via JSON serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        protected T Convert<T>(object o)
        {
            string json = JsonConvert.SerializeObject(o);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}