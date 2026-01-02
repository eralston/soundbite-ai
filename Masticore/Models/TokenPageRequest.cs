namespace Masticore.Models
{
    /// <summary>
    /// A paging request where skipping is based on tokens
    /// </summary>
    public class TokenPageRequest : PageRequestBase
    {
        /// <summary>
        /// A continuation token used by some endpoints to do paging
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public string SkipToken { get; set; }
    }
}
