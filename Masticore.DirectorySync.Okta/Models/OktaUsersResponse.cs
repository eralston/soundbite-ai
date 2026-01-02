namespace Masticore.DirectorySync.Okta
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class OktaUsersResponse
    {
        public string NextLink { get; set; }
        public OktaUsersResponse[] Results { get; set; }
    }

}
