using Newtonsoft.Json;
using System.Collections.Generic;

namespace Masticore.DirectorySync.Aad
{
    /// <summary>
    /// Helper class for generating OID Requests against graph
    /// </summary>
    public class OidRequest
    {
        /// <summary>
        /// Generates a request body for the given OIDs and entity type
        /// </summary>
        /// <param name="oids"></param>
        /// <param name="responseType"></param>
        /// <returns></returns>
        public static string Serialize(IEnumerable<string> oids, string responseType)
        {
            OidRequest body = new OidRequest
            {
                ids = oids,
                types = new string[] { responseType }
            };
            string bodyJson = JsonConvert.SerializeObject(body);
            return bodyJson;
        }

        /// <summary>
        /// Parses the given JSON to determine the oidRequest payload
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static OidRequest Deserialize(string json)
        {
            OidRequest oidRequest = JsonConvert.DeserializeObject<OidRequest>(json);
            return oidRequest;
        }

        /// <summary>
        /// The OID request type for groups
        /// </summary>
        public const string Groups = "group";

        /// <summary>
        /// The OID request type for users
        /// </summary>
        public const string Users = "user";

        /// <summary>
        /// The Graph URL for requesting a list of object by OID
        /// </summary>
        public const string OidRequestUrl = "https://graph.microsoft.com/v1.0/directoryObjects/getByIds";

        /// <summary>
        /// The list of OIDs to request
        /// </summary>
        public IEnumerable<string> ids;

        /// <summary>
        /// The type of entity for the response
        /// </summary>
        public string[] types;
    }
}
