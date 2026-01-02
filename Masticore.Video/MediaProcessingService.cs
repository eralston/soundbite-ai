using Masticore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masticore.Media
{
    /// <summary>
    /// Service for encoding and manipulating video files.
    /// </summary>
    public class MediaProcessingService : IMediaProcessingService
    {
        #region Constructor

        protected ILogger<MediaProcessingService> Logger;

        /// <summary>
        /// Instantiates a new <see cref="MediaProcessingService"/> instance.
        /// </summary>
        public MediaProcessingService(
           ILogger<MediaProcessingService> logger
            )
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Fields

        private static string _ffmpegBinDir = Environment.GetEnvironmentVariable("FFMPEG_BIN_DIR") ?? "FFMpeg\\bin\\";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the path at which the FFMpeg / FFProbe binaries can be found.
        /// </summary>
        public static string FFMpegBinDir
        {
            get => _ffmpegBinDir;
            set
            {
                _ffmpegBinDir = value.EnsureEndsWith("\\");
                FFMpegPath = $"{_ffmpegBinDir}ffmpeg.exe";
                FFProbePath = $"{_ffmpegBinDir}ffprobe.exe";
            }
        }

        public static bool Validated { get; set; } = false;

        /// <summary>
        /// Gets the path for the ffmpeg.exe executable.
        /// </summary>
        public static string FFMpegPath { get; private set; } = $"{_ffmpegBinDir}ffmpeg.exe";

        /// <summary>
        /// Gets the path for the ffprobe.exe executable.
        /// </summary>
        public static string FFProbePath { get; private set; } = $"{_ffmpegBinDir}ffprobe.exe";

        #endregion

        #region Methods

        public async Task<MediaDetails> GetMediaDetails(string videoPath)
        {
            Logger.LogInformation($"Getting media details for {videoPath}.");

            StringBuilder errorDetails = new StringBuilder();
            MediaDetails results = new MediaDetails() { FileName = videoPath };

            string codecType = null;
            string codecName = null;
            string codecLongName = null;

            Process process = StartFFProbe(
                $"-hide_banner -of default=noprint_wrappers=1 -show_streams \"{new FileInfo(videoPath).FullName}\"",
                false,
                (msg) =>
                {
                    if (!string.IsNullOrEmpty(msg))
                    {
                        string[] parts = msg.Trim().Split("=");
                        if (parts.Length == 2)
                        {
                            switch (parts[0])
                            {
                                case "bit_rate":
                                    if (int.TryParse(parts[1], out int bitRateValue))
                                    {
                                        results.BitRate = bitRateValue;
                                    }
                                    break;
                                case "duration":
                                    if (double.TryParse(parts[1], out double durationValue))
                                    {
                                        results.DurationInSeconds = durationValue;
                                    }
                                    break;
                                case "index":
                                    codecType = null;
                                    codecName = null;
                                    codecLongName = null;
                                    break;
                                case "codec_name":
                                    codecName = parts[1];
                                    break;
                                case "codec_long_name":
                                    codecLongName = parts[1];
                                    break;
                                case "codec_type":
                                    codecType = parts[1];
                                    // Code names are set before the codec type is known.  Once the codec type
                                    // is known, we can commit the names to the appropriate properties.
                                    switch (codecType)
                                    {
                                        case "audio":
                                            results.AudioCodecName = codecName;
                                            results.AudioCodecLongName = codecLongName;
                                            break;
                                        case "video":
                                            results.VideoCodecName = codecName;
                                            results.VideoCodecLongName = codecLongName;
                                            break;
                                    }
                                    break;
                                case "width":
                                    results.Width = Convert.ToInt32(parts[1]);
                                    break;
                                case "height":
                                    results.Height = Convert.ToInt32(parts[1]);
                                    break;
                                case "display_aspect_ratio":
                                    results.DisplayAspectRatio = parts[1];
                                    break;
                                case "start_time":
                                    switch (codecType)
                                    {
                                        case "audio":
                                            results.AudioStartTime = parts[1];
                                            break;
                                        case "video":
                                            results.VideoStartTime = parts[1];
                                            break;
                                    }
                                    break;
                                case "rotation":
                                    results.Rotation = Convert.ToInt32(parts[1]);
                                    break;
                            }
                        }
                    }
                },
                (err) =>
                {
                    errorDetails.Append("E: ");
                    errorDetails.AppendLine(err);
                });

            await process.WaitForExitAsync();

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                throw new Exception($"{nameof(GetMediaDetails)} - ffprobe failed with exit code {exitCode}. Output: \r\n{errorDetails}");
            }

            // Determine bandwidth by acquiring file size and dividing by the duration
            // NOTE: everyone seems to say that this is "good enough" for determining bandwidth.
            if (results.DurationInSeconds > 0)
            {
                long fileSize = new FileInfo(videoPath).Length;
                results.Bandwidth = Convert.ToInt32(Math.Ceiling(fileSize / results.DurationInSeconds));
            }

            return results;
        }

        /// <summary>
        /// Retrieves media details from the specified video.
        /// </summary>
        /// <param name="videoPath"></param>
        public async Task EncodeSegment(string segmentPath, string outputPath, int width, int height, int rotation)
        {
            Logger.LogInformation($"Encoding segment {segmentPath} to {outputPath}; width {width}, height {height}, and rotation {rotation}");

            //Ensure the output folder exists
            //string size = width == 0 ? "" : $"-vf scale={width}:{height}";
            //string args = $"-i \"{segmentPath}\" -vcodec libx264 -acodec aac {size}-copyts -muxdelay 0 \"{outputPath}\"";
            string size = width == 0 ? "" : $"scale={width}:{height}";
            string transposeFilter = GetTransposeFilter(rotation);
            string vf = !string.IsNullOrEmpty(size) && !string.IsNullOrEmpty(transposeFilter)
                ? $"{size},{transposeFilter}"
                : !string.IsNullOrEmpty(size)
                    ? size
                    : !string.IsNullOrEmpty(transposeFilter)
                        ? transposeFilter
                        : "";
            string vfOption = !string.IsNullOrEmpty(vf) ? $"-vf \"{vf}\" " : "";
            string args = $"-i \"{segmentPath}\" -vcodec libx264 -acodec aac {vfOption} -copyts -muxdelay 0 \"{outputPath}\"";

            await RunFFMpeg(args);
        }

        /// <inheritdoc />
        public async Task<TimeSpan> GetVideoDuration(string videoPath)
        {
            //Arguments = "-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 -i pipe:0",

            Logger.LogInformation($"Getting video duration for {videoPath}");
            StringBuilder output = new StringBuilder();
            TimeSpan result = new TimeSpan();
            Process process = StartFFProbe(
                $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 -i \"{new FileInfo(videoPath).FullName}\"",
                false,
                (msg) =>
                {
                    output.Append("D: ");
                    output.AppendLine(msg);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        if (float.TryParse(msg, out float durationValue))
                        {
                            long ticks = Convert.ToInt64(durationValue * 10000000);
                            result = new TimeSpan(ticks);
                        }
                    }
                },
                (err) =>
                {
                    output.Append("E: ");
                    output.AppendLine(err);
                });

            await process.WaitForExitAsync();

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                throw new Exception($"{nameof(GetVideoDuration)} - ffprobe failed with exit code {exitCode}. Output: \r\n{output}");
            }

            return result;
        }

        public static string GetTransposeFilter(int rotation)
        {
            return rotation switch
            {
                90 => "\"transpose=2\"",
                -90 or 270 => "\"transpose=1\"",
                180 or -180 => "\"transpose=1, transpose=1\"",
                _ => null
            };
        }

        /// <inheritdoc />
        public async Task<string[]> SplitVideo(string inputFile, string outputMask, int rotation)
        {
            Logger.LogInformation($"Splitting video {inputFile} into segments using mask {outputMask} and rotation {rotation}");

            //Ensure the output folder exists
            string folder = Path.GetDirectoryName(outputMask);
            try
            {
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"SplitVideo failed to create folder named '{folder}' to contain video segements.", ex);
            }

            //string args = $"-i \"{inputFile}\" -c copy -f segment -segment_time 10 -reset_timestamps 0 -muxdelay 0 \"{outputMask}\""; // Works for MP4
            string args = $"-i \"{inputFile}\" -c copy -f segment -segment_time 10 -reset_timestamps 0 \"{outputMask}\""; // works for WEBM
            await RunFFMpeg(args);

            string[] segments = Directory.GetFiles($"{folder}");
            segments = segments.Where(i => i != inputFile).ToArray();
            return segments;
        }

        private async Task RunFFMpeg(string args)
        {
            Logger.LogInformation($"Running ffmpeg with args: {args}");
            StringBuilder output = new StringBuilder();
            Process process = StartFFMpeg(
                args,
                false,
                (msg) =>
                {
                    output.Append("D: ");
                    output.AppendLine(msg);
                },
                (err) =>
                {
                    output.Append("E: ");
                    output.AppendLine(err);
                });

            await process.WaitForExitAsync();
            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                string msg = $"{nameof(ProcessVideoFile)} - ffmpeg failed with exit code {exitCode}. Output: \r\n{output}";
                Logger.LogCritical(msg);
                throw new Exception(msg);
            }
        }

        /// <inheritdoc />
        public async Task<TimeSpan> GetVideoDuration(Stream videoStream)
        {
            Logger.LogInformation("Getting in-memory video duration");
            StringBuilder output = new();
            TimeSpan result = new();
            Process process = StartFFProcess(
                new FileInfo(FFProbePath).FullName,
                $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 -i pipe:0",
                true,
                (msg) =>
                {
                    // msg comes back saying "pipe:.mp4: Invalid argument"
                    output.Append("D: ");
                    output.AppendLine(msg);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        if (float.TryParse(msg, out float durationValue))
                        {
                            long ticks = Convert.ToInt64(durationValue * 10000000);
                            result = new TimeSpan(ticks);
                        }
                    }
                },
                (err) =>
                {
                    output.Append("E: ");
                    output.AppendLine(err);
                });

            // Write the stream to the ffprobe process
            videoStream.SeekAndReturn(0, () =>
            {
                //WARN: exception is thrown if chunk size exceeds 4096
                videoStream.CopyToInChunks(process.StandardInput.BaseStream, 4096);
            });

            await process.WaitForExitAsync();

            int exitCode = process.ExitCode;
            if (exitCode != 0)
            {
                string msg = $"{nameof(GetVideoDuration)} - ffprobe failed with exit code {exitCode}. Output: \r\n{output}";
                Logger.LogCritical(msg);
                throw new Exception(msg);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<string> ProcessVideoFile(string inputFile, string outputFile, IMediaEffect[] effects)
        {
            string args = BuildArgsFromEffects(inputFile, ref outputFile, effects);
            await RunFFMpeg(args);
            return outputFile;
        }

        public int MaxBitRate(MediaDetails[] mediaDetails)
        {
            return (mediaDetails == null || mediaDetails.Length == 0)
                ? 0
                : mediaDetails.Max(i => i.BitRate);
        }

        public async Task<string> ReencodeVideo(string videoPath)
        {
            string tempFileName = Path.GetTempFileName().Replace(".tmp", ".mp4");
            string args = $"-i \"{videoPath}\" -c:v libvpx-vp9 -c:a libopus \"{tempFileName}\"";
            await RunFFMpeg(args);
            return tempFileName;
        }

        /// <inheritdoc />
        public async Task<string> EnsureDurationHeader(string videoPath)
        {
            Logger.LogInformation($"Ensuring duration header for {videoPath}");
            StringBuilder output = new StringBuilder();
            // We need to reencode because most videos arrive with broked codecx and theser make it uniform
            videoPath = await ReencodeVideo(videoPath);
            string tempFileName = Path.GetTempFileName().Replace(".tmp", ".mp4");
            string args = $"-i \"{videoPath}\" -vcodec copy -acodec copy \"{tempFileName}\"";
            await RunFFMpeg(args);
            return tempFileName;
        }

        /// <inheritdoc />
        public async Task<string> EnsureDurationHeader(Stream videoStream)
        {
            Logger.LogInformation("Ensuring duration header for in-memory video");
            StringBuilder output = new StringBuilder();
            string tempFileName = Path.GetTempFileName().Replace(".tmp", ".mp4");
            Process process = StartFFMpeg(
                $"-i pipe:.webm -vcodec copy -acodec copy \"{tempFileName}\"",
                true,
                (msg) =>
                {
                    output.Append("D: ");
                    output.AppendLine(msg);
                },
                (err) =>
                {
                    output.Append("E: ");
                    output.AppendLine(err);
                });

            // Write the stream to the ffmpeg process
            videoStream.SeekAndReturn(0, () =>
            {
                //WARN: exception is thrown if chunk size exceeds 4096
                videoStream.CopyToInChunks(process.StandardInput.BaseStream, 4096);
            });

            await process.WaitForExitAsync();
            int exitCode = process.ExitCode;
            return tempFileName;
        }

        /// <inheritdoc />        
        public async Task ExtractWav(string inputPath, FileType inputFileType, string wavPath)
        {
            Logger.LogInformation($"Extracting WAV from {inputPath} to {wavPath}");
            if (!wavPath.EndsWith(".wav", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception("Parameter wavPath must end with \".wav\" to notify ffmpeg to output WAV audio.");
            }

            string args = $"-y -i \"{inputPath}\" \"{wavPath}\"";
            await RunFFMpeg(args);
        }

        /// <inheritdoc />        
        public async Task ExtractAudioFromVideoFile(string inputPath, string outputPath)
        {
            Logger.LogInformation($"Extracting audio from {inputPath} to {outputPath}");
            //Ensure the output folder exists
            string folder = Path.GetDirectoryName(outputPath);
            try
            {
                if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"ExtractAudioFromVideoFile failed to create folder named '{folder}' to contain audio file.");
                throw new Exception($"SplitVideo failed to create folder named '{folder}' to contain video segements.", ex);
            }

            string args = $"-i \"{inputPath}\" -vn \"{outputPath}\"";
            await RunFFMpeg(args);
        }

        #endregion

        #region Methods - Utility

        private string BuildArgsFromEffects(string inputFile, ref string outputFile, IMediaEffect[] mediaEffects, bool overwriteExistingFile = true)
        {
            StringBuilder output = new StringBuilder();

            // Determine whether to apply flag that overwrites an existing output file
            if (overwriteExistingFile)
            {
                output.Append("-y ");
            }

            // Determine whether to trim the start
            TrimStart trimStart = mediaEffects.OfType<TrimStart>().FirstOrDefault();
            if (trimStart != null)
            {
                output.Append($"-ss {trimStart.Duration.TotalSeconds} -accurate_seek ");
            }

            // Add input file
            output.Append($"-i \"{inputFile}\" ");

            // Setup Encoding
            FfmpegEncode encode = mediaEffects.OfType<FfmpegEncode>().FirstOrDefault()
                ?? new FfmpegEncode("copy", "copy", Path.GetExtension(inputFile));

            // Ensure the output file has the appropriate extension
            outputFile = outputFile.ReplaceExtension(encode.FileExtension);

            if (!string.IsNullOrEmpty(encode.VideoCodec))
            {
                output.Append($"-c:v {encode.VideoCodec} ");
            }
            if (!string.IsNullOrEmpty(encode.AudioCodec))
            {
                output.Append($"-c:a {encode.AudioCodec} ");
            }

            // Build out Fade Effects
            BuildFades(output, mediaEffects, true); // Video Fades
            BuildFades(output, mediaEffects, false); // Audio Fades

            // Determine whether to trim the start
            TrimEnd trimEnd = mediaEffects.OfType<TrimEnd>().FirstOrDefault();
            if (trimEnd != null)
            {
                output.Append($"-to {trimEnd.Location.TotalSeconds} ");
            }

            // Specify the output file
            output.Append($"\"{outputFile}\"");

            return output.ToString().Trim();
        }

        private void BuildFades(StringBuilder output, IMediaEffect[] effects, bool isVideo)
        {
            Func<Fade, bool> filter = isVideo
                ? (f) => f.FadeTarget != FadeTargetType.Audio
                : (f) => f.FadeTarget != FadeTargetType.Video;

            Fade[] fades = effects.OfType<Fade>().Where(filter).ToArray();
            if (fades.Any())
            {
                output.Append(isVideo ? "-vf" : "-af");
                output.Append(" \"");
                for (int i = 0; i < fades.Length; i++)
                {
                    BuildFade(output, fades[i], i == 0, isVideo);
                }
                output.Append("\" ");
            }
        }

        private void BuildFade(StringBuilder output, Fade fade, bool isFirst, bool isVideo)
        {
            if (!isFirst)
            {
                output.Append(',');
            }

            output.Append(isVideo ? "fade" : "afade");
            output.Append("=t=");
            output.Append(fade.FadeDir == Media.FadeDirType.FadeIn ? "in" : "out");
            output.Append(":st=");
            output.Append(fade.Location.TotalSeconds);
            output.Append(":d=");
            output.Append(fade.Duration.TotalSeconds);
        }

        private Process StartFFMpeg(string args, bool redirectStandardIn, Action<string?> onStdOutRecieved, Action<string> onStdErrorRecieved)
        {
            return StartFFProcess(new FileInfo(FFMpegPath).FullName, args, redirectStandardIn, onStdOutRecieved, onStdErrorRecieved);
        }

        private Process StartFFProbe(string args, bool redirectStandardIn, Action<string?> onStdOutRecieved, Action<string> onStdErrorRecieved)
        {
            return StartFFProcess(new FileInfo(FFProbePath).FullName, args, redirectStandardIn, onStdOutRecieved, onStdErrorRecieved);
        }

        private Process StartFFProcess(string exePath, string args, bool redirectStandardIn, Action<string?> onStdOutRecieved, Action<string> onStdErrorRecieved)
        {
            Logger.LogInformation($"Starting process {exePath} with args {args}");
            ValidateSettings();

            // Setup the process
            Process process = new Process();
            process.StartInfo.FileName = exePath;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardInput = redirectStandardIn;
            process.StartInfo.CreateNoWindow = true;
            process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                onStdOutRecieved?.Invoke(e.Data);
            });
            process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
            {
                onStdOutRecieved?.Invoke(e.Data);
            });

            //start process
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        /// <summary>
        /// Validates that the configuration and required files for video processing are present.
        /// </summary>
        /// <exception cref="Exception">Thrown if settings are invalid.</exception>
        public static void ValidateSettings()
        {
            if (!Validated)
            {
                if (string.IsNullOrEmpty(FFMpegBinDir))
                {
                    throw new Exception("Cannot process video because the FFMpeg executable path is not set.");
                }

                if (!Directory.Exists(FFMpegBinDir))
                {
                    throw new Exception($"FFMpeg bin directory path is set but the directory does not exist (Path: {new DirectoryInfo(FFMpegBinDir).FullName}|Current Dir: {Environment.CurrentDirectory}).");
                }

                if (!File.Exists(FFMpegPath))
                {
                    throw new Exception($"FFMpeg executable was not found in the FFMpeg bin directory (Path: {new DirectoryInfo(FFMpegBinDir).FullName}).");
                }

                if (!File.Exists(FFProbePath))
                {
                    throw new Exception($"FFProbe executable was not found in the FFMpeg bin directory (Path: {new DirectoryInfo(FFMpegBinDir).FullName}).");
                }

                Validated = true;
            }
        }

        #endregion
    }
}