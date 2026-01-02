using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Masticore.Storage;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Soundbite.Services
{
    /// <summary>
    /// Basic implementation of the <see cref="IMediaStreamingService"/> interface
    /// </summary>
    public class MediaStreamingService : InfrastructureServiceBase<ISbInfrastructure>, IMediaStreamingService
    {
        #region Inner Types

        /// <summary>
        /// Instances of this object will track either a line of the playlist or a task to get the blob URL for the line.
        /// </summary>
        protected class FileLine
        {
            public string Value { get; set; }
            public Task<string> BlobUrlTask { get; set; }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Specifies the number of retries that occur if the HTTP request for the source proxy content fails.
        /// </summary>
        private const int MaxHttpRetry = 1;

        /// <summary>
        /// Specifies the template used for replacing Quality 
        /// </summary>
        public static string ManifestProxyUrl = "SHOULD_BE_SET_AT_RUNTIME";

        /// <summary>
        /// Supported levels for media quality
        /// </summary>
        public static string[] SupportedLevels = new string[] { "1080p", "720p", "480p", "360p", "240p", "144p", "orig" };

        #endregion

        #region Static Methods

        /// <summary>
        /// Throw an error if the given level is supported or not - this is case sensitive
        /// </summary>
        /// <param name="mediaQualityLevel"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void AssertValidMediaQualityLevel(string mediaQualityLevel)
        {
            if (string.IsNullOrEmpty(mediaQualityLevel)) { throw new ArgumentNullException(nameof(mediaQualityLevel)); }

            if (!SupportedLevels.Contains(mediaQualityLevel))
            {
                throw new ArgumentException("Invalid media quality level specified. Must be one of the following: 1080p, 720p, 480p, 360p, 240p, 144p, orig", nameof(mediaQualityLevel));
            }
        }

        /// <summary>
        /// Builds out the path to the clip specified in the <paramref name="request"/>.
        /// </summary>
        /// <param name="request">Encode clip request identifying the clip to encode.</param>
        /// <returns>a path to the clip specified in the <paramref name="request"/>.</returns>
        public static string GetAzureStoragePathToClipFolder(string sessionRoute, string promptRoute, string clipRoute)
        {
            return $"ses${sessionRoute}\\pro{promptRoute}\\clp{clipRoute}\\";
        }

        /// <summary>
        /// Builds out the azure container name for the specified <paramref name="request"/>.
        /// </summary>
        /// <param name="request">Encode clip request identifying the clip to encode.</param>
        /// <returns>the azure container name for the specified <paramref name="request"/>.</returns>
        public static string GetContainerName(string universalId)
        {
            return $"org{universalId}";
        }

        private static async Task<string> TransformLevelPlaylistContent(IBlobs blobs, string orgUid, string basePathToLevel, string levelPlaylistContent)
        {
            // Example value for levelPlaylistContent right now should be as follows:
            //#EXTM3U
            //#EXT-X-VERSION:4
            //#EXT-X-PLAYLIST-TYPE:VOD
            //#EXT-X-TARGETDURATION:11.075911
            //#EXT-X-MEDIA-SEQUENCE:0
            //#EXTINF:11.075911
            //seg_0000.ts{TOKEN}
            //#EXTINF:10.031011
            //seg_0001.ts{TOKEN}
            //#EXTINF:9.171878
            //seg_0002.ts{TOKEN}
            //#EXT-X-ENDLIST

            // We want to transform it into something as follows:
            //#EXTM3U
            //#EXT-X-VERSION:4
            //#EXT-X-PLAYLIST-TYPE:VOD
            //#EXT-X-TARGETDURATION:11.075911
            //#EXT-X-MEDIA-SEQUENCE:0
            //#EXTINF:11.075911
            //{basePathToLevel}seg_0000.ts{tokenValueForThisFile}
            //#EXTINF:10.031011
            //{basePathToLevel}seg_0001.ts{tokenValueForThisFile}
            //#EXTINF:9.171878
            //{basePathToLevel}seg_0002.ts{tokenValueForThisFile}
            //#EXT-X-ENDLIST

            // Make a collection that represents each line in the file as either a real string value or a task we can turn into one later as a batch
            List<FileLine> lines = new();
            string[] levelPlaylistLines = levelPlaylistContent.Split("\n");
            for (int i = 0; i < levelPlaylistLines.Length; i++)
            {
                string line = levelPlaylistLines[i];
                if (line.StartsWith("#EXTINF:"))
                {
                    // Add the current line since it's the marker then move up to the next line
                    lines.Add(new FileLine() { Value = line });
                    ++i;

                    // This is a line that contains a segment
                    string segmentName = levelPlaylistLines[i];
                    // We don't need the token in the segment name anymore, we're going to make our own token
                    segmentName = segmentName.Replace("{TOKEN}", "").Trim();
                    string segmentBlobPath = $"{basePathToLevel}\\{segmentName}";
                    // Generates an absolute URL with fresh token on the end
                    Task<string> segmentBlobUrlWithTokenTask = blobs.DownloadUrlAsync(GetContainerName(orgUid), segmentBlobPath);
                    lines.Add(new FileLine() { BlobUrlTask = segmentBlobUrlWithTokenTask });

                    // Top of the loop will increment past this line for us, so we're effectively skipped the segment name line
                }
                else
                {
                    lines.Add(new FileLine() { Value = line });
                }
            }

            // Complete all of the blob tasks
            await Task.WhenAll(lines.Where(l => l.BlobUrlTask != null).Select(l => l.BlobUrlTask));

            // Convert line objects back into file content
            StringBuilder levelPlaylistContentWithTokens = new();
            foreach (FileLine line in lines)
            {
                if (line.BlobUrlTask != null)
                {
                    string segmentBlobUrl = line.BlobUrlTask.Result;
                    levelPlaylistContentWithTokens.AppendLine(segmentBlobUrl);
                }
                else
                {
                    levelPlaylistContentWithTokens.Append(line.Value);
                }
            }

            return levelPlaylistContentWithTokens.ToString();
        }

        #endregion

        #region Fields

        ////////////////////////////////////////////////////////////////////////////////////////////
        // NOTE: regular expressions are static instances that are compiled to avoid the costs of
        //       creating and parsing regular expressions on each request. They are thread-safe.
        ////////////////////////////////////////////////////////////////////////////////////////////
        private static readonly Regex _qualityLevelManifestRegex = new Regex(@"(QualityLevels\(\d+\)/Manifest\(.+\))", RegexOptions.Compiled);
        private static readonly Regex _qualityLevel = new Regex(@"(QualityLevels\(\d+\))", RegexOptions.Compiled);
        private static readonly Regex _fragments = new Regex(@"Fragments\([\w\d,=-]+\)");
        private static readonly Regex _urlRegex = new Regex(@"("")(https?:\/\/[\da-z\.-]+\.[a-z\.]{2,6}[\/\w \.-]*\/?[\?&][^&=]+=[^&=#]*)("")", RegexOptions.Compiled);

        // DI Reference Fields
        protected IHttpClientFactory HttpClientFactory;
        protected IRbac Rbac { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="MediaStreamingService"/> instance.
        /// </summary>
        /// <param name="httpClientFactory">DI service reference.</param>
        public MediaStreamingService(
            IHttpClientFactory httpClientFactory,
            IRbac rbac,
            ISbInfrastructure infrastructure,
            IMapper mapper,
            ILogger<MediaStreamingService> logger
            )
            : base(infrastructure, logger, null, mapper)
        {
            Rbac = rbac ?? throw new ArgumentNullException(nameof(rbac));
            HttpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Async assert access for the current user to the given session
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="sessionRoute"></param>
        /// <returns></returns>
        protected async Task<string> AssertSessionAccess(string orgRoute, string sessionRoute)
        {
            // Check access
            SbDb db = await Infrastructure.DbAsync();

            // TODO: Permissions for sessions in IRbac or some SB variant
            UserEntity currentUser = await db.UserWhereUid(orgRoute, Rbac.UniversalIdForCurrentUser, PersonRole.Person);
            currentUser.AssertFound();

            // Pull session and update
            int[] groupIds = await db.GroupIdsAsync(orgRoute, Rbac.UniversalIdForCurrentUser);
            SessionEntity session = await db.SessionAsync(Rbac.UniversalIdForCurrentUser, orgRoute, sessionRoute, groupIds, ParticipantRole.Audience, true);
            session.AssertFound();

            return session.Organization.UniversalId;
        }

        /// <summary>
        /// Creates an HTTP client configured and ready for use.
        /// </summary>
        /// <returns>an HttpClient instance.</returns>
        private HttpClient GetHttpClient()
        {
            HttpClient httpClient = HttpClientFactory.CreateClient();
            httpClient.Timeout = new TimeSpan(0, 0, 0, 30 /*Seconds*/);
            httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true, NoStore = true };
            return httpClient;
        }

        /// <summary>
        /// Responsible for retrieving the HTTP response from the specified <paramref name="url"/>.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="httpClient">Optional. Pass in to use existing instance or a new instance will be created.</param>
        /// <param name="retryIndex">Optional. Pass in to indicate which retry this call represents.</param>
        /// <returns>a string containing the body response of the HTTP call.</returns>
        /// <exception cref="Exception">Thrown if the HTTP call fails.</exception>
        private async Task<string> GetHttpContent(string url, HttpClient? httpClient = null, int? retryIndex = 0)
        {
            // Initialize the http client if not specified
            httpClient = httpClient ?? GetHttpClient();

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                else
                {
                    throw new Exception($"Invalid HTTP Response:\r\nCode:{response.StatusCode}\r\nReason:{response.ReasonPhrase}\r\nBody:{response.Content}");
                }
            }
            catch (Exception ex)
            {
                // Retry the call but only up the max retry limit
                if (retryIndex <= MaxHttpRetry)
                {
                    // Retry
                    await Task.Delay(2000);
                    return await GetHttpContent(url, httpClient, retryIndex++);
                }
                else
                {
                    // Too many retries so throw an exception
                    throw new Exception("Media Proxy HTTP call to retrieve content from '' failed.", ex);
                }
            }
        }

        protected async Task<ClipEntity> ReadClipWithChildren(string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();
            ClipEntity clip = await db.ClipByRouteAsync(clipRoute);
            clip.AssertFound();
            return clip;
        }

        #endregion

        #region IMediaStreamingService Implementation

        /// <inheritdoc />
        public async Task<string> UpdateManifest(string sourceUrl, string token)
        {
            // Prepare Inputs
            sourceUrl = sourceUrl?.Trim() ?? string.Empty;
            token = token?.Trim() ?? string.Empty;

            // Validate Inputs
            if (string.IsNullOrEmpty(sourceUrl)) { throw new ArgumentNullException(nameof(sourceUrl)); }
            if (string.IsNullOrEmpty(token)) { throw new ArgumentNullException(nameof(token)); }

            // Acquire manifest content
            string manifestContent = await GetHttpContent(sourceUrl);

            // Update manifest content
            string manifestBaseUrl = sourceUrl.Substring(0, sourceUrl.IndexOf(".ism", StringComparison.OrdinalIgnoreCase)) + ".ism";
            string urlEncodedManifestBaseUrl = HttpUtility.UrlEncode(manifestBaseUrl);
            string urlEncodedToken = HttpUtility.UrlEncode(token);
            manifestContent = _qualityLevelManifestRegex.Replace(manifestContent,
                string.Format(CultureInfo.InvariantCulture,
                    "{0}?sourceUrl={1}/$1&token={2}",
                    ManifestProxyUrl,
                    urlEncodedManifestBaseUrl,
                    urlEncodedToken));

            return manifestContent;
        }

        /// <inheritdoc />
        public async Task<string> UpdateManifestQualityStream(string sourceUrl, string token)
        {
            // Prepare Inputs
            sourceUrl = sourceUrl?.Trim() ?? string.Empty;
            token = token?.Trim() ?? string.Empty;

            // Validate Inputs
            if (string.IsNullOrEmpty(sourceUrl)) { throw new ArgumentNullException(nameof(sourceUrl)); }
            if (string.IsNullOrEmpty(token)) { throw new ArgumentNullException(nameof(token)); }

            // Acquire manifest content
            string manifestContent = await GetHttpContent(sourceUrl);

            // Update manifest content
            string baseUrl = sourceUrl.Substring(0, sourceUrl.IndexOf(".ism", StringComparison.OrdinalIgnoreCase)) + ".ism";
            string newContent = _urlRegex.Replace(manifestContent, string.Format(CultureInfo.InvariantCulture, "$1$2&token={0}$3", token));
            Match match = _qualityLevel.Match(sourceUrl);
            if (match.Success)
            {
                string qualityLevel = match.Groups[0].Value;
                newContent = _fragments.Replace(newContent,
                    m => string.Format(CultureInfo.InvariantCulture,
                        baseUrl + "/" + qualityLevel + "/" + m.Value));
            }

            return newContent;
        }

        public async Task<string> ReadVariantPlaylist(string orgRoute, string sessionRoute, string promptRoute, string clipRoute)
        {
            Validator.ArgNotNullOrEmpty(orgRoute, nameof(orgRoute));
            Validator.ArgNotNullOrEmpty(sessionRoute, nameof(sessionRoute));
            Validator.ArgNotNullOrEmpty(promptRoute, nameof(promptRoute));
            Validator.ArgNotNullOrEmpty(clipRoute, nameof(clipRoute));

            string orgUid = await AssertSessionAccess(orgRoute, sessionRoute);

            IBlobs blobs = await Infrastructure.BlobsAsync();
            string basePath = GetAzureStoragePathToClipFolder(sessionRoute, promptRoute, clipRoute);
            Stream file = await blobs.DownloadAsync(GetContainerName(orgUid), $"{basePath}playlist.m3u8");
            // Convert stream to string
            string content = await file.ToStringContent();
            content = content.Replace("{TOKEN}", "");
            return content;
        }

        public async Task<string> ReadLevelPlaylist(string orgRoute, string sessionRoute, string promptRoute, string clipRoute, string level)
        {
            Validator.ArgNotNullOrEmpty(orgRoute, nameof(orgRoute));
            Validator.ArgNotNullOrEmpty(sessionRoute, nameof(sessionRoute));
            Validator.ArgNotNullOrEmpty(promptRoute, nameof(promptRoute));
            Validator.ArgNotNullOrEmpty(clipRoute, nameof(clipRoute));
            Validator.ArgNotNullOrEmpty(level, nameof(level));

            AssertValidMediaQualityLevel(level);

            string orgUid = await AssertSessionAccess(orgRoute, sessionRoute);

            // Read in the file as a string
            IBlobs blobs = await Infrastructure.BlobsAsync();
            string basePath = GetAzureStoragePathToClipFolder(sessionRoute, promptRoute, clipRoute);
            string basePathToLevel = $"{basePath}{level}";
            Stream levelPlaylistStream = await blobs.DownloadAsync(GetContainerName(orgUid), $"{basePathToLevel}\\playlist.m3u8");
            string levelPlaylistContent = await levelPlaylistStream.ToStringContent();

            // Apply absolute URLs with fresh tokens to each segment line
            string levelPlaylistContentWithTokens = await TransformLevelPlaylistContent(blobs, orgUid, basePathToLevel, levelPlaylistContent);
            return levelPlaylistContentWithTokens;
        }

        [Obsolete("Only for backward compatibility with clients; Use the other ReadVariantPlaylist method")]
        public async Task<string> ReadVariantPlaylist(string clipRoute)
        {
            Validator.ArgNotNullOrEmpty(clipRoute, nameof(clipRoute));

            ClipEntity clip = await ReadClipWithChildren(clipRoute);
            return await ReadVariantPlaylist(clip.Prompt.Session.Organization.Route, clip.Prompt.Session.Route, clip.Prompt.Route, clip.Route);

        }

        [Obsolete("Only for backward compatibility with clients; Use the other ReadLevelPlaylist method")]
        public async Task<string> ReadLevelPlaylist(string clipRoute, string level)
        {
            Validator.ArgNotNullOrEmpty(clipRoute, nameof(clipRoute));
            Validator.ArgNotNullOrEmpty(level, nameof(level));

            ClipEntity clip = await ReadClipWithChildren(clipRoute);
            return await ReadLevelPlaylist(clip.Prompt.Session.Organization.Route, clip.Prompt.Session.Route, clip.Prompt.Route, clip.Route, level);
        }

        #endregion
    }
}
