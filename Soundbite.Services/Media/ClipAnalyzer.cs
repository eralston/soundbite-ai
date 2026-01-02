using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Types;
using Masticore;
using Soundbite.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static TagLib.File;

namespace Soundbite.Services
{
    /// <summary>
    /// Read-only implementation of TagLib's IFileAbstraction interface, which should allow for parsing metadata
    /// </summary>
    public class TagLibStreamReader : IFileAbstraction
    {
        protected Stream _stream;
        protected string _extension;

        public TagLibStreamReader(Stream stream, string extension)
        {
            stream.Seek(0, SeekOrigin.Begin);
            _stream = stream;
            _extension = extension;
        }

        public string Name => $"Clip.${_extension}";

        public Stream ReadStream => _stream;

        public Stream WriteStream => throw new NotImplementedException();

        public void CloseStream(Stream stream)
        {
            // Do nothing since we'll manage our own stream lifecycle
        }
    }

    /// <summary>
    /// Reads a <see cref="Models.NewClip"/> to determine key metadata
    /// This most critically includes the multi-factor estimate of its length, rounded to the minute
    /// </summary>
    /// <remarks>
    /// If <see cref="NewClip.Stream"/> is empty, then this just passes through data from the <see cref="Models.NewClip"/> instance
    /// </remarks>
    public class ClipAnalyzer
    {
        // TODO: Using the speech-to-text transcription feature to likely determine length as accurately as possible

        /// <summary>
        /// Converts the given length in bytes to a duration in seconds as if a MB is one minute
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public static long ByteCountAsSeconds(decimal byteCount)
        {
            decimal fractionsOfAMegabytes = byteCount / BytesPerMegabyte;
            decimal secondsLikeFactor = Math.Ceiling(fractionsOfAMegabytes * 60);
            return secondsLikeFactor > 0 ? (long)secondsLikeFactor : 1; // Return min 1
        }

        /// <summary>
        /// The number of bytes in a megabyte
        /// </summary>
        private const decimal BytesPerMegabyte = 1024000;

        /// <summary>
        /// A number that is used as a multiple for MPEG file type durations
        /// This is necessary because React Mic actually generate Variable Bit Rate (VBR) MPEGs, not actually MP3s
        /// This means the header for these files contain inaccurate data
        /// </summary>
        private static readonly int MpgUncertaintyFactor = int.Parse(Environment.GetEnvironmentVariable("SB_MPG_UNCERTAINTY_FACTOR") ?? "3");

        /// <summary>
        /// The NewClip object to be analyzed
        /// </summary>
        public NewClip NewClip { get; private set; }

        /// <summary>
        /// The Soundbite FileType for this clip
        /// </summary>
        public Masticore.FileType FileType { get; private set; }

        /// <summary>
        /// Reads the header of the file to estimate the length in seconds
        /// </summary>
        public int HeaderSeconds { get; private set; }

        /// <summary>
        /// Gets the file size in megabytes for the file, divided by a pro-ration of time such that this is a seconds-like value for space in the system
        /// These are treated as being analogous to minutes given the prices for storage ingress/egress
        /// </summary>
        public int SizeSeconds { get; private set; }

        /// <summary>
        /// Gets the name of the temp file created to ensure the duration was accurately acquired.
        /// This file should be used over the one missing duratio because it makes it easy to
        /// display the end time of a clip without having to read through the file.
        /// </summary>
        public string PathToFileWithUpdatedDuration { get; private set; }

        /// <summary>
        /// Returns the duration the clip uploader claims in seconds
        /// In the future, this can be enforced by limiting transcription by it, which means users will be crippling themselves if they don't report an accurate number
        /// </summary>
        public int IndicatedSeconds { get; private set; }

        public bool IsHeaderUncertain { get; private set; }

        /// <summary>
        /// Constructor; will throw an exception if the stream on the given NewClip object is invalid
        /// </summary>
        /// <param name="newClip"></param>
        private ClipAnalyzer()
        {
        }

        public static async Task<ClipAnalyzer> CreateNew(NewClip newClip, bool isHeaderUncertain = false)
        {
            ClipAnalyzer result = new ClipAnalyzer();
            await result.Initialize(newClip, isHeaderUncertain);
            return result;
        }

        public Task Initialize(NewClip newClip, bool isHeaderUncertain = false)
        {
            NewClip = newClip;
            FileType = GetFileType(newClip.FileType);

            //NOTE: Video file duration info is processed in an azure function.

            if (FileType.IsAudio())
            {
                HeaderSeconds = GetHeaderSeconds();
                SizeSeconds = GetSizeSeconds();
                IndicatedSeconds = GetIndicatedSeconds();
                IsHeaderUncertain = FileType == Masticore.FileType.Mpg || FileType == Masticore.FileType.MpgAudio || isHeaderUncertain;
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// Pulls the duration claimed by the clip when it was uploaded
        /// WARNING: Since this came from a request to this API, it should be regarded with suspicion
        /// </summary>
        /// <returns></returns>
        protected int GetIndicatedSeconds()
        {
            return NewClip.Seconds;
        }

        /// <summary>
        /// Deterime filetype via FileTypeChecker library
        /// </summary>
        /// <returns></returns>
        protected Masticore.FileType GetFileType(Masticore.FileType unverifiedFileType)
        {
            if (NewClip.Stream == null)
            {
                return NewClip.FileType;
            }

            IFileType fileType = FileTypeValidator.GetFileType(NewClip.Stream);

            // TODO: Resolve why native captured video files are not being recognized as MP4s
            // For now, assume it's a video and hope the pipeline can resuce us
            if (fileType == null)
            {
                return Masticore.FileType.Mp4;
            }

            if (fileType is Mp3)
            {
                return Masticore.FileType.Mp3;
            }

            if (fileType is MpegAudio)
            {
                return Masticore.FileType.MpgAudio;
            }

            if (fileType is Mp4 || fileType is M4v)
            {
                return Masticore.FileType.Mp4;
            }

            if (fileType == null)
            {
                // When dealing with a WEBM file the file type will be null so use webm
                // if the unverified file type is WEBM
                if (unverifiedFileType == Masticore.FileType.Webm)
                {
                    return Masticore.FileType.Webm;
                }

                // Technically this means the file type was not recognized but
                // we make a BAD assumption here
                return Masticore.FileType.Mp4;
            }

            // If the file is not null then it is some other type of file like
            // a zip or word doc so we throw an exception.
            throw new BadClipFileTypeException("File type cannot be determine; not a recognized audio file");
        }

        /// <summary>
        /// Determine length described in the header of the file using TagLib library
        /// WARNING: For mpeg files, this number is usually well under reality due to Variable Bit Rate (VBR) encoding
        /// </summary>
        /// <returns></returns>
        protected int GetRawHeaderLengthInSeconds()
        {
            if (NewClip.Stream == null)
            {
                // If there is no stream, then there is no header
                return 0;
            }

            // Determine length
            TagLibStreamReader file = new TagLibStreamReader(NewClip.Stream, Extension);
            TagLib.Mpeg.AudioFile audioFile = new TagLib.Mpeg.AudioFile(file);
            TimeSpan timeSpan = audioFile.Properties.Duration;
            int headerLength = (int)Math.Ceiling(timeSpan.TotalSeconds);
            return headerLength > 0 ? headerLength : 1; // Return min 1
        }

        /// <summary>
        /// The HeaderLength scale by an uncertainty factor driven by file type
        /// </summary>
        protected int GetHeaderSeconds()
        {
            if (NewClip.Stream == null)
            {
                // If there is no stream, then there is no header
                return 0;
            }

            int lengthInSeconds = GetRawHeaderLengthInSeconds();

            if (IsHeaderUncertain)
            {
                lengthInSeconds *= MpgUncertaintyFactor;
            }

            return lengthInSeconds;
        }

        /// <summary>
        /// Calculate file size in megabytes/60 rounded to an integer
        /// </summary>
        /// <returns></returns>
        protected int GetSizeSeconds()
        {
            if (NewClip.Stream == null)
            {
                // If there is no stream, then there is no header
                return 0;
            }

            decimal bytesInDecimal = NewClip.Stream.Length;
            return (int)ByteCountAsSeconds(bytesInDecimal);
        }

        /// <summary>
        /// Uses the FileType to determine the real file extension
        /// </summary>
        protected string Extension => FileType switch
        {
            Masticore.FileType.Mp3 => "mp3",
            Masticore.FileType.Mp4 => "mp4",
            Masticore.FileType.Wav => "wav",
            Masticore.FileType.Mpg => "mpg",
            Masticore.FileType.MpgAudio => "mpg",
            _ => throw new BadClipFileTypeException("Extension cannot be determine; not a recognized audio file"),
        };

        /// <summary>
        /// Returns the most accurate measurement of minutes we can determine.
        /// This is based on a combination of FileSize, IndicatedDuration, and HeaderLengthWithUncertainty.
        /// In the future, this may include information returned by the transcription system.
        /// </summary>
        public int Seconds
        {
            get
            {
                // TODO: Use potential values returned when a clip is transcribed

                // A list of candidate values for the clip length
                // This enables taking the most pessimistic estimate from many possible factors
                int[] candidates = new int[]
                {
                    SizeSeconds,
                    IndicatedSeconds,
                    HeaderSeconds
                };

                int max = candidates.Max(s => s);
                return max;
            }
        }
    }
}
