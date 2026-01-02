namespace Masticore.DirectorySync.Interact
{
    public class PeopleResponse
    {
        public int TotalResults { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
        public PersonResponse[] Results { get; set; }
    }
}
