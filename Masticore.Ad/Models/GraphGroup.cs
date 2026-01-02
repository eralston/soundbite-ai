using Masticore.Models;
using Newtonsoft.Json;

namespace Masticore.Ad
{
    /// <summary>
    /// Represents a group from <seealso href="https://developer.microsoft.com/en-us/graph/graph-explorer">MS Graph</seealso>
    /// </summary>
    public class GraphGroup : GraphBase, IGroupFields
    {
        public const string GroupDataType = "#microsoft.graph.group";
        public const string SelectFields = "id,deletedDateTime,displayName,description,mail";
        // As of Oct 2021, there was no discernable way to include Security Groups and Distros alongside unified and ignoring all other types; thus we must allow querying for all groups
        public static readonly string AllGroupsTargetUrl = $"https://graph.microsoft.com/v1.0/groups?$select={SelectFields}";
        public const string AllGroupsUrl = "https://graph.microsoft.com/v1.0/groups/";
        public const string AllGroupsDeltaUrl = "https://graph.microsoft.com/v1.0/groups/delta";

        /// <summary>
        /// Builds a filter clause for the given groupId
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public static string FilterById(string groupId)
        {
            return $"id eq '{groupId}'";
        }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("mail")]
        public string Mail { get; set; }

        [JsonProperty("members@delta")]
        public GraphMemberDelta[] Members { get; set; }

        public string NameWithMail()
        {
            string name = Mail != null ? $"{Name} - {Mail}" : Name;
            return name;
        }
    }
}