using Masticore.Media;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Services.Tests
{
    public class MediaTests
    {

        #region Constants

        public const bool RunVideoTests = true;
        public static TimeSpan DemoWebFileDuration = new TimeSpan(0, 0, 2, 45, 485);
        public const string DemoWebmFile = "..\\..\\..\\Files\\Example.webm";  // Duration = 00:02:45.485
        public const string NoDurationWebmFile = "..\\..\\..\\Files\\NoDuration.webm";
        public const string OutputWebmFile = "..\\..\\..\\Files\\Output.webm";
        public const string OutputMp4File = "..\\..\\..\\Files\\Output.mp4";

        #endregion

        public IMediaProcessingService GetMediaProcessingService()
        {
            return new MediaProcessingService(new NullLogger<MediaProcessingService>());
        }

        [Fact]
        public async Task TrimStart()
        {
            // Arrange
            const int trimSeconds = 10; // Seconds to trim
            const int variance = 1; // Variance allowance because trim operation without transcoding is "sketchy"
            IMediaProcessingService service = GetMediaProcessingService();

            // Act
            if (RunVideoTests)
            {
                TimeSpan originalDuration = await service.GetVideoDuration(DemoWebmFile);

                await service.ProcessVideoFile(
                    DemoWebmFile,
                    OutputWebmFile,
                    new[] {
                        new TrimStart(){
                            Duration= new TimeSpan(0,0,trimSeconds)
                        }
                });

                // Assert
                Assert.True(File.Exists(OutputWebmFile), "Output file was not created");
                TimeSpan trimmedDuration = await service.GetVideoDuration(OutputWebmFile);
                Assert.True(originalDuration.TotalSeconds - trimmedDuration.TotalSeconds >= (trimSeconds - variance));
                File.Delete(OutputWebmFile);
            }
        }

        [Fact]
        public async Task TrimEnd()
        {
            // Arrange
            const int trimSeconds = 10; // Seconds to trim
            const int variance = 1; // Variance allowance because trim operation without transcoding is "sketchy"
            IMediaProcessingService service = GetMediaProcessingService();

            // Act
            if (RunVideoTests)
            {
                TimeSpan originalDuration = await service.GetVideoDuration(DemoWebmFile);

                await service.ProcessVideoFile(
                    DemoWebmFile,
                    OutputWebmFile,
                    new[] {
                        new TrimEnd(){
                             Location = DemoWebFileDuration.Subtract(new TimeSpan(0,0,trimSeconds))
                        }
                });

                // Assert
                Assert.True(File.Exists(OutputWebmFile), "Output file was not created");
                TimeSpan trimmedDuration = await service.GetVideoDuration(OutputWebmFile);
                Assert.True(originalDuration.TotalSeconds - trimmedDuration.TotalSeconds >= (trimSeconds - variance));
                File.Delete(OutputWebmFile);
            }
        }

        [Fact]
        public async Task ConvertToMp4()
        {
            // Arrange
            IMediaProcessingService service = GetMediaProcessingService();

            // Act
            if (RunVideoTests)
            {
                TimeSpan originalDuration = await service.GetVideoDuration(DemoWebmFile);

                await service.ProcessVideoFile(
                    DemoWebmFile,
                    OutputMp4File,
                    new IMediaEffect[] {
                        new FfmpegEncodeToMp4()
                    });

                // Assert
                Assert.True(File.Exists(OutputMp4File), "Output file was not created");
                TimeSpan trimmedDuration = await service.GetVideoDuration(OutputMp4File);
                Assert.True(originalDuration.TotalSeconds - trimmedDuration.TotalSeconds < 0.001f);
                File.Delete(OutputMp4File);
            }
        }

        [Fact]
        public async Task GetVideoFileDuration()
        {
            if (RunVideoTests)
            {
                IMediaProcessingService service = GetMediaProcessingService();
                TimeSpan duration = await service.GetVideoDuration(DemoWebmFile);
                Assert.Equal(2, duration.Minutes);
                Assert.Equal(45, duration.Seconds);
                Assert.Equal(485, duration.Milliseconds);
            }
        }

        [Fact]
        public async Task GetVideoStreamDuration()
        {
            if (!RunVideoTests)
            {
#pragma warning disable CS0162 // Unreachable code detected
                return;
#pragma warning restore CS0162 // Unreachable code detected
            }

            IMediaProcessingService service = GetMediaProcessingService();
            using FileStream fs = File.Open(DemoWebmFile, FileMode.Open);
            using MemoryStream ms = new MemoryStream();
            fs.CopyTo(ms);
            TimeSpan duration = await service.GetVideoDuration(ms);
            fs.Close();
            Assert.Equal(2, duration.Minutes);
            Assert.Equal(45, duration.Seconds);
            Assert.Equal(485, duration.Milliseconds);
        }

        [Trait("Heavy", "true")]
        [Fact]
        public async Task EnsureVideoFileDuration()
        {
            IMediaProcessingService service = GetMediaProcessingService();
            TimeSpan result = await service.GetVideoDuration(NoDurationWebmFile);

            // We should not have duration data from the no duration test file
            Assert.Equal(0, result.TotalSeconds);

            string tempFile = await service.EnsureDurationHeader(NoDurationWebmFile);
            result = await service.GetVideoDuration(tempFile);

            Assert.True(File.Exists(tempFile));
            Assert.True(result.TotalMilliseconds > 0);
            File.Delete(tempFile);
        }

        [Trait("Heavy", "true")]
        [Fact]
        public async Task EnsureVideoStreamDuration()
        {
            IMediaProcessingService service = GetMediaProcessingService();
            TimeSpan result = await service.GetVideoDuration(NoDurationWebmFile);

            // We should not have duration data from the no duration test file
            Assert.Equal(0, result.TotalSeconds);

            using (FileStream fs = new FileStream(NoDurationWebmFile, FileMode.Open))
            {
                string tempFile = await service.EnsureDurationHeader(NoDurationWebmFile);
                result = await service.GetVideoDuration(tempFile);
                fs.Close();

                Assert.True(File.Exists(tempFile));
                Assert.True(result.TotalMilliseconds > 0);
                File.Delete(tempFile);
            }
        }

        [Trait("Heavy", "true")]
        [Fact]
        public async Task ExtractWavFromVideo()
        {
            const string outputFile = "output.wav";

            IMediaProcessingService service = GetMediaProcessingService();
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            await service.ExtractWav(DemoWebmFile, FileType.Webm, outputFile);
            Assert.True(File.Exists(outputFile));
            File.Delete(outputFile);
        }
    }
}