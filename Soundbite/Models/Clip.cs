using Masticore;
using Masticore.Media;
using Masticore.Models;
using Newtonsoft.Json;


namespace Soundbite.Models
{
    /// <summary>
    /// Models for a piece of media
    /// </summary>
    public class Clip : ResourceBase
    {
        #region Fields

        private IMediaEffect[] _mediaEffects;
        private string _mediaEffectsJson;

        #endregion

        /// <summary>
        /// Specifies the <see cref="ClipType"/> of the clip, EG <see cref="ClipType.Prompt"/>, etc
        /// </summary>
        public ClipType ClipType { get; set; }

        /// <summary>
        /// Specifies how the clip file is hosted.
        /// </summary>
        public ClipHostingType HostingType { get; set; }

        public string HostingData { get; set; }

        /// <summary>
        /// Specifies the <see cref="FileType"/> format of the clip, EG <see cref="FileType.Mp3"/>, etc
        /// </summary>
        public FileType FileType { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds in duration for this clip
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Gets or sets the state of the transcript for the clip.
        /// </summary>
        public TranscriptState TranscriptState { get; set; }

        /// <summary>
        /// Gets or sets the media processing state for the clip.
        /// </summary>
        public MediaProcessingState MediaProcessingState { get; set; }

        /// <summary>
        /// Gets or sets a JSON string representing the media effects to apply to the clip.
        /// </summary>
        [JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
        public string MediaOperationsJson
        {
            get
            {
                if (_mediaEffects != null)
                {
                    _mediaEffectsJson = _mediaEffects.Length > 0
                        ? _mediaEffects.ToLowerCamelJson()
                        : null;
                }
                return _mediaEffectsJson;
            }
            set
            {
                _mediaEffects = null;
                _mediaEffectsJson = value;
            }
        }

        /// <summary>
        /// Gets the media effects associated with this clip.
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public IMediaEffect[] MediaEffects
        {
            get
            {
                if (_mediaEffects == null)
                {
                    _mediaEffects = string.IsNullOrEmpty(MediaOperationsJson)
                        ? new IMediaEffect[0]
                        : JsonUtils.FromLowerCamelJson<IMediaEffect[]>(MediaOperationsJson);
                }
                return _mediaEffects;
            }
            set
            {
                value = value ?? new IMediaEffect[0];
                _mediaEffects = value;
                _mediaEffectsJson = value.ToLowerCamelJson();
            }
        }
    }
}