using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Services;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Exposes endpoints for the Streaming Media Proxy.
    /// </summary>
    [ApiController]
    [Authorize]
    public class MediaStreamingController
        : ControllerBase
    {
        #region Fields

        private IMediaStreamingService MediaStreamingService { get; }
        private IClipFileService ClipFileService { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="MediaStreamingController"/> instance.
        /// </summary>
        /// <param name="mediaStreamingService">DI service reference.</param>
        /// <param name="clipFileService">DI service reference.</param>
        public MediaStreamingController(
            IMediaStreamingService mediaStreamingService,
            IClipFileService clipFileService)
        {
            MediaStreamingService = mediaStreamingService;
            ClipFileService = clipFileService;
        }

        #endregion

        #region EndPoint for Soundbite Hosted Media

        /// <summary>
        /// Responsible for getting the m3u8 multivariant manifest for a SB hosted streaming clip.
        /// The SAS token from this call can be appended to the URLs for all variant playlist files
        /// and vieo segements within the multivariant playlists.
        /// </summary>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <param name="clipRoute">Route of the clip.</param>
        /// <returns>a <see cref="SbClipManifestResult"/> containing the URL of the manifest + SAS token</returns>
        [HttpGet]
        [Route("/mediaproxy/sbhosted/manifest")]
        public async Task<SbClipManifestResult> SbClipManifest(string orgRoute, string clipRoute)
        {
            // We break this up here to make it slightly easier to use
            string url = await ClipFileService.SbMediaManifestDownloadUrlAsync(orgRoute, clipRoute);
            string[] parts = url.Split("?");
            SbClipManifestResult result = new SbClipManifestResult() { Url = parts[0], Token = parts.Length > 1 ? parts[1] : string.Empty };

            // Not sure why but parts of the URL are encoded and some are not but the partially URL encoded value
            // was messing stuff up so I am sending it fully decoded.
            result.Url = HttpUtility.UrlDecode(result.Url);
            return result;
        }

        #endregion

        #region EndPoint for Media Proxy Calls

        //NOTE: these are accessible anonymously because that is the way that AMS handles calls
        //      for the manifest file and the behavior was mimicked in this controller because there
        //      was seemingly no way to tell the player to provide authentication when requesting a
        //      media file.  However, it just dawned on me that we have a valid token right here and
        //      while we cannot authenticate it using the built in bearer token security middleware,
        //      we can just validate it manually in the anonymous method. Technically a third-party
        //      COULD use this controller method to update ad-hoc manifests, so if we want to avoid
        //      that possibility we can verify the token.
        //      Work item: https://dev.azure.com/soundbiteai/Soundbite/_workitems/edit/1518
        /// <summary>
        /// Responsible for making proxy calls for top-level M3U8 playlist manifest files served 
        /// from Azure Media Services (AMS). 
        /// </summary>
        /// <param name="sourceUrl">URL from which to acquire the top-level M3U8 playlist.</param>
        /// <param name="token">Token to inject into the M3U8 playlist manifest.</param>
        /// <returns>content from the specified <paramref name="sourceUrl"/> with updated media links containing embedded <paramref name="token"/>.</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("/mediaproxy/manifest")]
        public async Task<ContentResult> Manifest(string sourceUrl, string token)
        {
            // Configure Response Headers
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("X-Content-Type-Options", "nosniff");
            Response.Headers.Add("Cache-Control", "no-cache");
            // NEED TO FIGURE THIS OUT -- Response.Headers.Add("Cache-Control", "max-age=259200");

            // Create Proxy Manfiest
            // WARNING: the route of this URL must match the route in the ManifestPart action in the StreamingMediaProxyController in the API project
            Soundbite.Services.MediaStreamingService.ManifestProxyUrl = $"{Request.Scheme}://{Request.Host.Value}/mediaproxy/manifestpart";

            string content = await MediaStreamingService.UpdateManifest(sourceUrl, token);

            ContentResult contentResult = new ContentResult()
            {
                Content = content,
                ContentType = "application/vnd.apple.mpegurl",
                StatusCode = 200
            };

            return contentResult;
        }

        /// <summary>
        /// Responsible for making proxy calls for quality-stream-level M3U8 playlist manifest 
        /// files served from Azure Media Services (AMS). The "quality stream" playlist is
        /// referenced by the top-level manifest and points to all of the "files" associated
        /// with a specific encoding quality / bitrate supported by the streaming media source.
        /// </summary>
        /// <param name="sourceUrl">URL from which to acquire the quality-stream-level M3U8 playlist.</param>
        /// <param name="token">Token to inject into the M3U8 playlist manifest.</param>
        /// <returns>content from the specified <paramref name="sourceUrl"/> with updated media links containing embedded <paramref name="token"/>.</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("/mediaproxy/manifestpart")]
        public async Task<ContentResult> ManifestPart(string sourceUrl, string token)
        {
            // Create Proxy Manfiest
            string content = await MediaStreamingService.UpdateManifestQualityStream(sourceUrl, token);

            // Configure Response Headers
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Cache-Control", "no-cache");

            ContentResult result = new()
            {
                Content = content,
                ContentType = "application/vnd.apple.mpegurl",
                StatusCode = 200
            };

            return result;
        }


        /// <summary>
        /// Async read the laylist for the given clip, offering it as an API-based content with relative paths to the level playlists
        /// </summary>
        /// <remarks>To the outside world this looks like an m3u8 file, but really it's a dynamic mapping of one</remarks>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/media/{orgRoute}/{sessionRoute}/{promptRoute}/{clipRoute}/playlist.m3u8")]
        public async Task<ContentResult> ReadVariantPlaylist(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            string content = await MediaStreamingService.ReadVariantPlaylist(orgRoute, sessionRoute, promptRoute, clipRoute);

            // Configure Response Headers
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Cache-Control", "no-cache");

            ContentResult result = new()
            {
                Content = content,
                ContentType = "application/vnd.apple.mpegurl",
                StatusCode = 200
            };

            return result;
        }

        /// <summary>
        /// Async read by given clip route
        /// </summary>
        /// <param name="clipRoute"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/media/clip/{clipRoute}/playlist.m3u8")]
        [Obsolete("For client compatibility only; use other ReadVariantPlaylist method in the future")]
        public async Task<ContentResult> ReadVariantPlaylistByClip(string clipRoute)
        {
            string content = await MediaStreamingService.ReadVariantPlaylist(clipRoute);

            // Configure Response Headers
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Cache-Control", "no-cache");

            ContentResult result = new()
            {
                Content = content,
                ContentType = "application/vnd.apple.mpegurl",
                StatusCode = 200
            };

            return result;
        }

        /// <summary>
        /// Async read the playlist file for the given clip at the given level, offering API-based content that behaves like an HLS m38 files with absolute paths into content storage with secure tokens
        /// </summary>
        /// <remarks>To the outside world this looks like an m3u8 file; however, it's actually a dynamic mapping with security tokens</remarks>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <param name="promptRoute"></param>
        /// <param name="clipRoute"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/media/{orgRoute}/{sessionRoute}/{promptRoute}/{clipRoute}/{level}/playlist.m3u8")]
        public async Task<ContentResult> ReadLevelPlaylist(string orgRoute, string sessionRoute, string promptRoute, string clipRoute, string level)
        {
            string content = await MediaStreamingService.ReadLevelPlaylist(orgRoute, sessionRoute, promptRoute, clipRoute, level);

            // Configure Response Headers
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Cache-Control", "no-cache");

            ContentResult result = new()
            {
                Content = content,
                ContentType = "application/vnd.apple.mpegurl",
                StatusCode = 200
            };

            return result;
        }

        /// <summary>
        /// Async read by the given clip route and level m3u8 file contents
        /// </summary>
        /// <param name="clipRoute"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/media/clip/{clipRoute}/{level}/playlist.m3u8")]
        [Obsolete("For client compatibility only; use other ReadLevelPlaylist method in the future")]
        public async Task<ContentResult> ReadLevelPlaylistByClip(string clipRoute, string level)
        {
            string content = await MediaStreamingService.ReadLevelPlaylist(clipRoute, level);

            // Configure Response Headers
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
            Response.Headers.Add("Cache-Control", "no-cache");

            ContentResult result = new()
            {
                Content = content,
                ContentType = "application/vnd.apple.mpegurl",
                StatusCode = 200
            };

            return result;
        }


        #endregion        
    }
}
