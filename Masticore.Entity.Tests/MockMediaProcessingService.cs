using Masticore.Media;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Entity.Tests
{
    public class MockMediaProcessingService : IMediaProcessingService
    {
        public Task<string> EnsureDurationHeader(string videoPath)
        {
            return Task.FromResult(videoPath);
        }

        public Task<string> EnsureDurationHeader(Stream videoStream)
        {
            return Task.FromResult(string.Empty);
        }

        public Task ExtractWav(string inputPath, FileType inputFileType, string wavPath)
        {
            return Task.CompletedTask;
        }

        public Task<TimeSpan> GetVideoDuration(string videoPath)
        {
            return Task.FromResult(new TimeSpan());
        }

        public Task<TimeSpan> GetVideoDuration(Stream videoStream)
        {
            return Task.FromResult(new TimeSpan());
        }

        public Task<string> ProcessVideoFile(string inputFile, string outputFile, IMediaEffect[] effects)
        {
            return Task.FromResult(outputFile);
        }

        public Task<string[]> SplitVideo(string inputFile, string outputMask, int rotation)
        {
            return Task.FromResult(new string[0]);
        }

        public Task<MediaDetails> GetMediaDetails(string videoPath)
        {
            return Task.FromResult<MediaDetails>(null);
        }

        public Task EncodeSegment(string segmentPath, string outputFolder, int width, int height, int rotation)
        {
            return Task.CompletedTask;
        }

        public Task ExtractAudioFromVideoFile(string inputPath, string outputPath)
        {
            return Task.CompletedTask;
        }

    }
}
