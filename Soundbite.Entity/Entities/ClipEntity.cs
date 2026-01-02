using Masticore;
using Masticore.Entity;
using Masticore.Media;
using Newtonsoft.Json;
using Soundbite.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Soundbite.Entity
{
    /// <summary>
    /// Media files connected to sessions, enabling people to collaborate
    /// </summary>
    public class ClipEntity : ResourceEntityBase
    {
        #region Fields

        private IMediaEffect[] _mediaEffects;
        private string _mediaEffectsJson;

        #endregion

        // IClipFields

        [JsonProperty]
        public ClipType ClipType { get; set; }

        [JsonProperty]
        public FileType FileType { get; set; }

        /// <summary>
        /// Specifies how the clip file is hosted.
        /// </summary>
        [JsonProperty]
        public ClipHostingType HostingType { get; set; }

        /// <summary>
        /// Gets or sets a string containing information that in conjunction with the 
        /// <see cref="ClipHostingType"/> identifies how to locate the clip file.
        /// </summary>
        public string HostingData { get; set; }

        /// <summary>
        /// Stores metadata about the clip (e.g. video dimensions, etc)
        /// </summary>                
        public string MetaData { get; set; }

        /// <summary>
        /// Gets or sets how this clip came into being
        /// </summary>
        [JsonProperty]
        public ClipSource ClipSource { get; set; }

        /// <summary>
        /// Gets or sets the current step in the processing of the clip
        /// </summary>
        [JsonProperty]
        public ClipState ClipState { get; set; }

        /// <summary>
        /// Gets or sets the transcript state for the clip.
        /// </summary>
        //[JsonProperty]
        public TranscriptState TranscriptState { get; set; }

        /// <summary>
        /// Gets or sets the media processing state for the clip.
        /// </summary>        
        public MediaProcessingState MediaProcessingState { get; set; }

        /// <summary>
        /// Length in seconds used for billing.
        /// This is a behind-the-scenes assessment of the real cost for this clip
        /// </summary>
        /// <remarks>
        /// This should NEVER come back out of the API
        /// </remarks>
        [JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public int BillingSeconds { get; set; }

        /// <summary>
        /// The length to display in the UI
        /// </summary>
        public int DisplaySeconds { get; set; }


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
                        ? _mediaEffects.ToLowerCamelJson(true, true)
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
        [NotMapped]
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
                _mediaEffectsJson = value.ToLowerCamelJson(true, true);
            }
        }

        // Ordinal?

        // Relationships

        // The IClip.Contributor is derived from the CreatedBy property in EntityBase

        /// <summary>
        /// To on on Prompt
        /// Clips attached to prompts are Org-scoped
        /// Clips without prompts are probably User-scoped
        /// </summary>
        public int? PromptId { get; set; }
        public virtual PromptEntity Prompt { get; set; }
        public ICollection<ClipOperationEntity> ClipOperations { get; set; }
    }
}
