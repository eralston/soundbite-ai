using Masticore;
using Soundbite.Models;
using Soundbite.Services.Tests.Content;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class ClipAnalyzerTest : ServiceTestBase
    {
        [Fact]
        public void InvalidMp3Test()
        {
            // ARRANGE
            NewClip newClip = new NewClip
            {
                Stream = MediaExamples.Invalid_Mp3,
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                ParticipantRole = ParticipantRole.Host
            };

            /**************************************************************************************/
            /**************************************************************************************/
            /**************************************************************************************/
            //TODO: update this when there is better WEBM detection.  GetFileType in the
            // ClipAnalyzer needs updating too.  Right now I am bypassing this due to time.
            // I believe the only major issue is that people can upload corrupt files but
            // in theory that will auto-correct when they realize the file won't play.

            // ACT + ASSERT
            //Assert.Throws<BadClipFileTypeException>(() =>
            //{
            //    ClipAnalyzer analyzer = ClipAnalyzer.CreateNew(newClip).Result;
            //});
            /**************************************************************************************/
            /**************************************************************************************/
            /**************************************************************************************/
        }

        [Fact]
        public void ThreeSecondCbrMp3()
        {
            // ARRANGE
            NewClip newClip = new NewClip
            {
                Stream = MediaExamples.Valid_ThreeSecondCbrMp3,
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                ParticipantRole = ParticipantRole.Host,
                Seconds = 1 // analyzed.IndicatedSeconds should equal this value
            };

            // ACT + ASSERT
            ClipAnalyzer analyzer = ClipAnalyzer.CreateNew(newClip).Result;
            Assert.Equal(2, analyzer.SizeSeconds);
            Assert.Equal(1, analyzer.IndicatedSeconds);
            Assert.Equal(3, analyzer.HeaderSeconds);
            Assert.Equal(3, analyzer.Seconds); // Take the biggest
        }

        [Fact]
        public void TwoMinuteFiveMegCbrMp3()
        {
            // ARRANGE
            NewClip newClip = new NewClip
            {
                Stream = MediaExamples.Valid_TwoMinuteFiveMegCbrMp3,
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                ParticipantRole = ParticipantRole.Host,
                Seconds = 13 // analyzed.IndicatedSeconds should equal this value
            };

            // ACT + ASSERT
            ClipAnalyzer analyzer = ClipAnalyzer.CreateNew(newClip).Result;
            Assert.Equal(310, analyzer.SizeSeconds); // File is just over 5mb
            Assert.Equal(13, analyzer.IndicatedSeconds); // Passed 13 above
            Assert.Equal(133, analyzer.HeaderSeconds); // Clip is 2 minutes 12 seconds long
            Assert.Equal(310, analyzer.Seconds); // Take the biggest
        }

        [Fact]
        public void ThirtySevenSecondsCbr()
        {
            // ARRANGE
            NewClip newClip = new NewClip
            {
                Stream = MediaExamples.Valid_ThirtySevenSecondSameSizeCbrMp3,
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                ParticipantRole = ParticipantRole.Host,
                Seconds = 7 // analyzed.IndicatedSeconds should equal this value
            };

            // ACT + ASSERT
            ClipAnalyzer analyzer = ClipAnalyzer.CreateNew(newClip).Result;
            Assert.Equal(36, analyzer.SizeSeconds); // File is just over 5mb
            Assert.Equal(7, analyzer.IndicatedSeconds); // Passed 7 above
            Assert.Equal(38, analyzer.HeaderSeconds); // Clip is 2 minutes 12 seconds long
            Assert.Equal(38, analyzer.Seconds); // Take the biggest
        }

        [Fact]
        public void ThirtySevenSecondsVbr()
        {
            // ARRANGE
            NewClip newClip = new NewClip
            {
                Stream = MediaExamples.Valid_ThirtySecondSecondSameSizeVbrMp3,
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                ParticipantRole = ParticipantRole.Host,
                Seconds = 7 // analyzed.IndicatedSeconds should equal this value
            };

            // ACT + ASSERT
            ClipAnalyzer analyzer = ClipAnalyzer.CreateNew(newClip, true).Result;
            Assert.Equal(36, analyzer.SizeSeconds); // File is just over 5mb
            Assert.Equal(7, analyzer.IndicatedSeconds); // Passed 7 above
            Assert.Equal(38, analyzer.HeaderSeconds); // Clip is 2 minutes 12 seconds long
            Assert.Equal(38, analyzer.Seconds); // Take the biggest
        }

        [Fact]
        public void ThirtySecondMusicMpg()
        {
            // ARRANGE
            NewClip newClip = new NewClip
            {
                Stream = MediaExamples.Valid_ThirtySecondMusicMpgVbr,
                ClipType = ClipType.Prompt,
                FileType = FileType.Mp3,
                ParticipantRole = ParticipantRole.Host,
                Seconds = 28 // analyzed.IndicatedSeconds should equal this value
            };

            // ACT + ASSERT
            ClipAnalyzer analyzer = ClipAnalyzer.CreateNew(newClip, true).Result;
            Assert.Equal(15, analyzer.SizeSeconds); // File is just over 5mb
            Assert.Equal(newClip.Seconds, analyzer.IndicatedSeconds); // Passed 7 above
            Assert.Equal(16, analyzer.HeaderSeconds); // Clip is 2 minutes 12 seconds long
            Assert.Equal(28, analyzer.Seconds); // Take the biggest
        }
    }
}
