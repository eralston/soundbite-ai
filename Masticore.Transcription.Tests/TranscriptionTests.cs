using Masticore.Media;
using Masticore.Tests;
using Masticore.Transcription.Azure;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Transcription.Tests
{
    public class TranscriptionTests : TestBase
    {
        public static string WavFilePath = Environment.CurrentDirectory + "\\..\\..\\..\\files\\NoBasisForGovt.wav";
        public static string OtherWavFilePath = Environment.CurrentDirectory + "\\..\\..\\..\\files\\Recording.wav";
        // public const string Mp3FilePath = "C:\\Code\\Soundbite\\Masticore.Transcription.Tests\\files\\NoBasisForGovt.mp3";
        public static string Mp3FilePath = Environment.CurrentDirectory + "\\..\\..\\..\\files\\TranscriptionTests.mp3";
        public static string WebmFilePath = Environment.CurrentDirectory + "\\..\\..\\..\\files\\VideoTransTest.webm";
        public const string AzureRegion = "westus";
        public const string AzureKey = "[AZURE TRANSCRIPTION KEY]";

        [Fact]
        public async Task TranscribeWavFile()
        {
            // Arrange
            IMediaProcessingService mediaProcessingService = new MediaProcessingService(new NullLogger<MediaProcessingService>());
            AzureTranscriptionService service = new AzureTranscriptionService(new NullLogger<AzureTranscriptionService>(), mediaProcessingService, AzureRegion, AzureKey);

            // Act
            TranscriptionResult result = await service.TranscribeFile(
               OtherWavFilePath, FileType.Wav);

            // Assert
            Assert.NotEqual("Error", result.State);
            Assert.Equal("This is a test to see if it's going to transcribe.", result.Sections[0].Text);
        }

        [Trait("Heavy", "true")]
        [Fact]
        public async Task TranscribeMp3File()
        {
            // Arrange
            IMediaProcessingService mediaProcessingService = new MediaProcessingService(new NullLogger<MediaProcessingService>());
            AzureTranscriptionService service = new AzureTranscriptionService(new NullLogger<AzureTranscriptionService>(), mediaProcessingService, AzureRegion, AzureKey);

            // Act
            TranscriptionResult result = await service.TranscribeFile(Mp3FilePath, FileType.Mp3);

            // Assert
            Assert.NotNull(result);
            // NOTE: Number of sections and their duration can change from time-to-time
            // This uses a real Azure service in its run, so it may change from time to time
            Assert.Equal(2, result.Sections.Count);

            // Azure Speech to Text does not produce the same results each time.  The "mutterings"
            // word sometimes comes back as "moorings" so I decided to not do a direct check against
            // the text and instead opted for the word counts.

            //NOTE: Something weird has happened here twice now.  When these tests were originally
            //  setup the result.Sections was a 0 based index.  At some point it became a 1 based
            //  index and the tests were updated.  Now it is back to a 0 based index.  This does
            //  not seem to have messed up any run-time functionality in the system but it is weird.

            // NOTE: length can change depending on how they interpret some words (and it changes from time to time) so there is a range check
            int length = (result.Sections[0] as AzureTranscriptionSection).Data.NBest[0].Words.Length;
            Assert.True(length >= 47);
            Assert.True(length <= 48);

            // NOTE: length can change depending on how they interpret some words (and it changes from time to time) so there is a range check
            length = (result.Sections[1] as AzureTranscriptionSection).Data.NBest[0].Words.Length;
            Assert.True(length >= 43);
            Assert.True(length <= 44);

            Assert.True(result.Duration.TotalSeconds > 28);
        }

        [Fact]
        public async Task TranscribeWebmFile()
        {
            // Arrange
            IMediaProcessingService mediaProcessingService = new MediaProcessingService(new NullLogger<MediaProcessingService>());
            AzureTranscriptionService service = new AzureTranscriptionService(new NullLogger<AzureTranscriptionService>(), mediaProcessingService, AzureRegion, AzureKey);

            // Act
            TranscriptionResult result = await service.TranscribeFile(WebmFilePath, FileType.Webm);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.State == "Success");
            Assert.Equal(1, result.Sections.Count);

            // Azure Speech to Text does not produce the same results each time.  The "mutterings"
            // word sometimes comes back as "moorings" so I decided to not do a direct check against
            // the text and instead opted for the word counts.

            //NOTE: Soundbite may be getting confused between 1 or 2 words so we consider this a
            //  success if it comes back with 18 or 19 words.
            int length = (result.Sections[0] as AzureTranscriptionSection).Data.NBest[0].Words.Length;
            Assert.True(length == 18 || length == 19);
        }
    }
}
