namespace Masticore.Ad
{
    /// <summary>
    /// Interface for holding credentials
    /// </summary>
    public interface IAdAppSettings
    {
        string AppId { get; set; }
        string AppSecret { get; set; }
    }
}