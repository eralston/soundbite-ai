using Masticore.Storage;
using System.IO;
using System.Reflection;

namespace Soundbite.Services.Tests.Content
{
    /// <summary>
    /// A compliation of example data for running tests
    /// </summary>
    public class MediaExamples
    {
        private static readonly Assembly _Assembly = Assembly.GetExecutingAssembly();

        public static Stream Valid_TransciptRecording
        {
            get
            {
                string resourceName = "Soundbite.Services.Tests.Content.Transcript.mp3";
                return _Assembly.GetManifestResourceStream(resourceName);
            }
        }

        public static Stream Valid_ThreeSecondCbrMp3
        {
            get
            {
                string resourceName = "Soundbite.Services.Tests.Content.three-seconds-cbr.mp3";
                return _Assembly.GetManifestResourceStream(resourceName);
            }
        }

        public static Stream Valid_TwoMinuteFiveMegCbrMp3
        {
            get
            {
                string resourceName = "Soundbite.Services.Tests.Content.two-min-five-mb-cbr.mp3";
                return _Assembly.GetManifestResourceStream(resourceName);
            }
        }

        public static Stream Valid_ThirtySevenSecondSameSizeCbrMp3
        {
            get
            {
                string resourceName = "Soundbite.Services.Tests.Content.37-sec-cbr.mp3";
                return _Assembly.GetManifestResourceStream(resourceName);
            }
        }

        public static Stream Valid_ThirtySecondSecondSameSizeVbrMp3
        {
            get
            {
                string resourceName = "Soundbite.Services.Tests.Content.37-sec-vbr.mp3";
                return _Assembly.GetManifestResourceStream(resourceName);
            }
        }

        public static Stream Valid_ThirtySecondMusicMpgVbr
        {
            get
            {
                string resourceName = "Soundbite.Services.Tests.Content.30-sec-music-mpg-vbr.mpg";
                return _Assembly.GetManifestResourceStream(resourceName);
            }
        }

        /// <summary>
        /// A valid Stream object, but it's holding a simple string value not a real MP3 file
        /// </summary>
        public static Stream Invalid_Mp3 => "This is Not an MP3; It's just a string".ToStream();
    }
}
