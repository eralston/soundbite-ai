using System;
using System.Collections.Generic;

namespace Masticore.DirectorySync.Okta
{
    public class OktaGroupResponse
    {
        public string id { get; set; }
        public DateTime created { get; set; }
        public DateTime lastUpdated { get; set; }
        public DateTime lastMembershipUpdated { get; set; }
        public List<string> objectClass { get; set; }
        public string type { get; set; }
        public OktaGroupProfile profile { get; set; }
        public GroupLinks _links { get; set; }
    }

    public class GroupLinks
    {
        public List<Logo> logo { get; set; }
        public Users users { get; set; }
        public Apps apps { get; set; }
    }

    public class Apps
    {
        public string href { get; set; }
    }

    public class Logo
    {
        public string name { get; set; }
        public string href { get; set; }
        public string type { get; set; }
    }

    public class OktaGroupProfile
    {
        public string name { get; set; }
        public string description { get; set; }
    }

    public class Users
    {
        public string href { get; set; }
    }
}
