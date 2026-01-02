using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Defines the handling of HLS Playlist and metadata such that the API can provide secure access to the content
    /// </summary>
    public interface IMediaStreamingService
    {
        /// <summary>
        /// Updates the top-level M3U8 playlist file to support in-line AES JWT tokens.
        /// </summary>
        /// <param name="sourceUrl">URL of the content that needs to be udpated to support AES JWT tokens.</param>
        /// <param name="token">Token to inject into the content found at the <paramref name="sourceUrl"/>.</param>
        /// <returns>Content from the <paramref name="sourceUrl"/> updated with AES JWT token specified in <paramref name="token"/>.</returns>
        Task<string> UpdateManifest(string sourceUrl, string token);

        /// <summary>
        /// Updates the quality stream manifest for a specific "quality" level.
        /// </summary>
        /// <param name="playbackUrl">URL of the manifest to update with token information.</param>
        /// <param name="token">Token to append to media URLs.</param>
        /// <returns>Content from the <paramref name="playbackUrl"/> with updated media URLs containing the specified <paramref name="token"/>.</returns>
        Task<string> UpdateManifestQualityStream(string playbackUrl, string token);

        /// <summary>
        /// Reads the m3u8 playlist for the given clip, offering it as an API-based content with relative paths to another 
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        Task<string> ReadVariantPlaylist(string orgRoute, string sessionRoute, string promptRoute, string clipRoute);

        /// <summary>
        /// Reads thee given m3u8 playlist for the given clip; offering each level of quality
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        Task<string> ReadVariantPlaylist(string clipRoute);

        /// <summary>
        /// Reads the m3u8 playlist for the given clip at the given quality level, offering it as an API-based content with absolute paths to content annotated with tokens
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        Task<string> ReadLevelPlaylist(string orgRoute, string sessionRoute, string promptRoute, string clipRoute, string level);

        /// <summary>
        /// Read m3u8 for the given clip at the given quality level
        /// </summary>
        /// <param name="clipRoute"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        Task<string> ReadLevelPlaylist(string clipRoute, string level);
    }
}