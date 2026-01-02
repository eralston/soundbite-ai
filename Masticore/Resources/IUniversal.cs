namespace Masticore.Resources
{
    /// <summary>
    /// Holds a GUID-based universal ID, EG OID in AAD
    /// This should normally never be sent to the client, with some thoughtful exceptions
    /// </summary>
    [CodeGenModel(Ignore = true)]
    public interface IUniversal
    {
        /// <summary>
        /// Gets or sets an ID linking the item to an external third-party resource
        /// </summary>
        string UniversalId { get; set; }
    }
}
