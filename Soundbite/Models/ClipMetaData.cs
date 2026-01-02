using Masticore;
using System.Collections.Generic;

namespace Soundbite.Models
{
    /// <summary>
    /// Metadata stored along with a clip as a JSON blob in the database.
    /// </summary>
    public class ClipMetadata
    {
        public static ClipMetadata FromJson(string json)
        {
            return string.IsNullOrEmpty(json)
                ? new ClipMetadata()
                : JsonUtils.FromLowerCamelJson<ClipMetadata>(json);
        }

        public IList<ClipMetadataVideoTrack> _videoTracks;

        public IList<ClipMetadataVideoTrack> VideoTracks
        {
            get
            {
                _videoTracks = _videoTracks ?? new List<ClipMetadataVideoTrack>();
                return _videoTracks;
            }
            set => _videoTracks = value;
        }

        /// <summary>
        /// Gets or sets the name of the JOB associated with encoding the clip. After a job 
        /// completes successfully, it is deleted from Azure. After this job has been deleted,
        /// this value will be set to <c>null</c>.
        /// </summary>
        public string AmsEncodingJobName { get; set; }

        /// <summary>
        /// Stores off the transform name in case we need it for some reason in the future. This
        /// value is probably a duplicate of the billing code associated with the clip operation.
        /// </summary>
        public string AmsEncodingJobTransform { get; set; }

        /// <summary>
        /// Gets or sets the name of the Asset created to store raw media files before encoding 
        /// occurs. The raw file in this asset gets copied from the normal SB clip container so
        /// this asset is deleted after encoding occurs (because we have a copy of the file).  
        /// Once the raw encoding asset has been deleted, this value will be set to <c>null</c>.
        /// </summary>
        public string AmsRawAssetName { get; set; }

        /// <summary>
        /// Gets or sets the name of the Asset in AMS that contains the fully encoded clip. This 
        /// asset is used to setup the streaming locator for the clip.
        /// </summary>
        public string AmsHostAssetName { get; set; }

        /// <summary>
        /// Gets or sets the name of the Asset where transcription files are output.
        /// </summary>
        public string AmsTranscribeAssetName { get; set; }

        /// <summary>
        /// Gets or sets the name of the Asset where transcription files are output.
        /// </summary>
        public string AmsTranscribeJobName { get; set; }
    }
}