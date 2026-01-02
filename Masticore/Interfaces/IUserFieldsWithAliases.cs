namespace Masticore.Models
{
    /// <summary>
    /// An object that describes a user for synchronization
    /// </summary>
    public interface IUserFieldsWithAliases : IUserFields
    {
        /// <summary>
        /// Alt emails are aliases for identifying the same user and should translate into
        /// </summary>
        public string[] AliasEmails { get; }
    }
}
