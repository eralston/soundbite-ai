using Masticore;
using Masticore.Media;
using System;
using System.IO;

namespace Soundbite.Models
{
    /// <summary>
    /// Model for creating a new clip in the system based on a binary stream
    /// </summary>
    [CodeGenModel]
    public class NewClip : IDisposable
    {
        /// <summary>
        /// This is for carrying the Stream around the front and back-end, but remember that it won't be JSON serialized, so one must move it between server and client someo other way (EG, form file)
        /// </summary>
        [CodeGenField(IsNullable = true, TypeName = "File")]
        public Stream Stream { get; set; }

        public ClipType ClipType { get; set; }

        public ClipHostingType HostingType { get; set; }

        [CodeGenField(IsNullable = true)]
        public string HostingData { get; set; }

        public FileType FileType { get; set; }

        public ParticipantRole ParticipantRole { get; set; }

        [CodeGenField(IsNullable = true)]
        public IMediaEffect[] MediaEffects { get; set; }

        [CodeGenField(IsNullable = true)]
        public string MetaData { get; set; }

        /// <summary>
        /// The duration claimed by the uploader that must be regarded with suspicion by the back-end, even if most of the time its measured by Soundbite's own front-end code
        /// </summary>
        [CodeGenField(IsNullable = true)]
        public int Seconds { get; set; }

        /// <summary>
        /// Passthrough to the <see cref="Stream"/> object if set
        /// </summary>
        public void Dispose()
        {
            Stream?.Dispose();
        }
    }
}