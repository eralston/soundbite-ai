namespace Masticore.DirectorySync.Interact
{
    public class PersonResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string PrimaryDepartment { get; set; }
        public string PrimaryLocation { get; set; }
        public string PrimaryCompany { get; set; }
        public string WorkPhone { get; set; }
        public string Mobile { get; set; }
        public string Extension { get; set; }
        public string Email { get; set; }
        public bool IsFollowing { get; set; }
        public int Followers { get; set; }
        public int Following { get; set; }
        public int InfluenceScore { get; set; }
        public bool IsMe { get; set; }
        public int AssetId { get; set; }
        public string ImageUrl { get; set; }
        public string ExpertiseUrl { get; set; }
        public string InterestsUrl { get; set; }
        public string FollowersUrl { get; set; }
    }
}
