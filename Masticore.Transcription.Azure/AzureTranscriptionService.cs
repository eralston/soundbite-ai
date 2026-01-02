using Masticore.Media;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Soundbite;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Masticore.Transcription.Azure
{
    /// <summary>
    /// Contains transcription result information with details
    /// </summary>
    public class AzureTranscriptionService : ITranscriptionService
    {
        #region Properties

        public string SpeechServiceKey { get; set; }
        public string AzureRegion { get; set; }
        private ILogger Logger { get; set; }
        private IMediaProcessingService MediaProcessingService { get; set; }

        #endregion

        #region Constructor

        public AzureTranscriptionService(ILogger<AzureTranscriptionService> logger, IMediaProcessingService mediaProcessingService, string azureRegion, string speechServiceKey)
        {
            Logger = logger;
            MediaProcessingService = mediaProcessingService;
            AzureRegion = azureRegion;
            SpeechServiceKey = speechServiceKey;
        }

        #endregion

        #region ITranscriptionService Implementation

        /// <summary>
        /// Configures the transcription service based on the specified organization settings.
        /// </summary>
        /// <param name="orgSettings">Organization settings used to configure the transcription service.</param>
        /// <returns>a task indicating success or failure of the operation.</returns>
        public Task Initialize(IOrgSettings orgSettings)
        {
            OrgSettings? sbOrgSettings = orgSettings as OrgSettings;
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<TranscriptionResult> TranscribeFile(string filePath, FileType format)
        {
            switch (format)
            {
                case FileType.Wav:
                    return await TranscribeWaveFile(filePath);
                case FileType.Mp3:
                case FileType.MpgAudio:
                case FileType.Mpg:
                    return await TranscribeMp3File(filePath);
                default:
                    if (format.IsVideo())
                    {
                        return await TranscribeVideoFile(filePath, format);
                    }
                    throw new NotImplementedException("Transcription does not support the specified file format.");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Transcribes an WAV file.
        /// </summary>
        /// <param name="filePath">Path to the WAV file to transcribe.</param>
        /// <returns>a <see cref="TranscriptionResult"/> with transcription data.</returns>
        private async Task<TranscriptionResult> TranscribeWaveStream(Stream stream)
        {
            //NOTE: Code does not work as-is. I'm leaving this in here for now. Stream is reading binary data
            // but nothing is getting picked up by the audio processor.  Some reading seems to indicate that
            // for a stream to work the audio config needs to be set with bit rate / channel / sample settings
            // that we don't have right off.  File based is working fine. 

            // LINK: https://github.com/MicrosoftDocs/azure-docs/blob/main/articles/cognitive-services/Speech-Service/how-to-use-audio-input-streams.md

            TranscriptionResult result = new TranscriptionResult();
            TaskCompletionSource<int> taskCompleteionSource = new TaskCompletionSource<int>();
            SpeechConfig config = SpeechConfig.FromSubscription(SpeechServiceKey, AzureRegion);
            StringBuilder transcriptionStringBuilder = new StringBuilder();

            using (PushAudioInputStream audioInputStream = AudioInputStream.CreatePushStream())
            using (AudioConfig audioInput = AudioConfig.FromStreamInput(audioInputStream))
            using (SpeechRecognizer recognizer = new SpeechRecognizer(config, audioInput))
            {
                // Subscribes to events.  
                recognizer.Recognizing += (sender, eventargs) =>
                {
                    //TODO: Handle recognized intermediate result  
                };

                recognizer.Recognized += (sender, eventargs) =>
                {
                    if (eventargs.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        transcriptionStringBuilder.Append(eventargs.Result.Text);
                    }
                    else if (eventargs.Result.Reason == ResultReason.NoMatch)
                    {
                        //TODO: Handle not recognized value  
                    }
                };

                recognizer.Canceled += (sender, eventargs) =>
                {
                    if (eventargs.Reason == CancellationReason.Error)
                    {
                        //TODO: Handle error  
                    }

                    if (eventargs.Reason == CancellationReason.EndOfStream)
                    {
                        //Console.WriteLine(transcriptionStringBuilder.ToString());
                    }

                    taskCompleteionSource.TrySetResult(0);
                };

                recognizer.SessionStarted += (sender, eventargs) =>
                {
                    //Started recognition session                      
                };

                recognizer.SessionStopped += (sender, eventargs) =>
                {
                    //Ended recognition session                      
                    taskCompleteionSource.TrySetResult(0);
                };

                // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.  
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                byte[] data;
                BinaryReader reader = new BinaryReader(stream);
                do
                {
                    data = reader.ReadBytes(1024);
                    audioInputStream.Write(data, data.Length);
                } while (data.Length > 0);

                // Close the stream to terminate the transcription recognizer
                audioInputStream.Close();

                // Waits for completion.  
                // Use Task.WaitAny to keep the task rooted.  
                Task.WaitAny(new[] { taskCompleteionSource.Task });

                // Stops recognition.  
                await recognizer.StopContinuousRecognitionAsync();
            }


            return result;
        }

        /// <summary>
        /// Transcribes an WAV file.
        /// </summary>
        /// <param name="filePath">Path to the WAV file to transcribe.</param>
        /// <returns>a <see cref="TranscriptionResult"/> with transcription data.</returns>
        private async Task<TranscriptionResult> TranscribeWaveFile(string filePath)
        {
            TranscriptionResult result = new TranscriptionResult()
            {
                Date = DateTime.UtcNow,
                Format = "ASTT" /*Azure Speech To Text*/,
                Version = new Version(1, 0, 0)
            };
            TaskCompletionSource<int> taskCompleteionSource = new TaskCompletionSource<int>();
            SpeechConfig config = SpeechConfig.FromSubscription(SpeechServiceKey, AzureRegion);
            config.RequestWordLevelTimestamps();
            config.OutputFormat = OutputFormat.Detailed;
            //config.SetProfanity(ProfanityOption.Masked)

            using (AudioConfig audioInput = AudioConfig.FromWavFileInput(filePath))
            {
                using SpeechRecognizer recognizer = new SpeechRecognizer(config, audioInput);
                recognizer.Recognized += (sender, eventArgs) =>
                {

                    if (eventArgs.Result.Reason == ResultReason.RecognizedSpeech)
                    {
                        string json = eventArgs.Result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
                        result.Sections.Add(new AzureTranscriptionSection()
                        {
                            OffsetTicks = eventArgs.Result.OffsetInTicks,
                            DurationTicks = eventArgs.Result.Duration.Ticks,
                            Text = eventArgs.Result.Text,
                            Data = JsonUtils.FromJson<AzureTranscriptionData>(json)
                        });
                    }
                    else if (eventArgs.Result.Reason == ResultReason.NoMatch)
                    {
                        //TODO: Handle not recognized value  
                    }
                };

                recognizer.Canceled += (sender, eventArgs) =>
                {
                    switch (eventArgs.Reason)
                    {
                        case CancellationReason.Error:
                            Logger.LogError($"Transcription cancelled due to error: {eventArgs.ErrorDetails}");
                            result.State = "Error";
                            break;
                        case CancellationReason.CancelledByUser:
                            Logger.LogError($"Transcription cancelled by user -- this should not happen.");
                            result.State = "CancelledByUser";
                            break;
                        case CancellationReason.EndOfStream:
                            result.State = "Success";
                            break;
                    }

                    // Regardless of reason, mark the task complete
                    taskCompleteionSource.TrySetResult(0);
                };

                recognizer.SessionStopped += (sender, eventargs) =>
                {
                    //This may not seem to do anything but is here in case there is no recognized speech.
                    taskCompleteionSource.TrySetResult(0);
                };


                // Starts continuous recognition. Uses StopContinuousRecognitionAsync() to stop recognition.  
                await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                // Waits for completion.  
                // Use Task.WaitAny to keep the task rooted.  
                Task.WaitAny(new[] { taskCompleteionSource.Task });

                // Stops recognition.  
                await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            }

            result.ProcessingDuration = new TimeSpan(DateTime.UtcNow.Ticks - result.Date.Ticks);
            if (result.Sections.Any())
            {
                TranscriptionSection last = result.Sections.Last();
                result.Duration = new TimeSpan(last.OffsetTicks + last.DurationTicks);
            }
            return result;
        }

        /// <summary>
        /// Transcribes an MP3 file.
        /// </summary>
        /// <param name="filePath">Path to the MP3 file to transcribe.</param>
        /// <returns>a <see cref="TranscriptionResult"/> with transcription data.</returns>
        private async Task<TranscriptionResult> TranscribeMp3File(string filePath)
        {
            string convertedFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            ConvertMp3ToWav(filePath, convertedFile);
            TranscriptionResult result = await TranscribeWaveFile(convertedFile);
            try
            {
                File.Delete(convertedFile);
            }
            catch (Exception ex) // Pokemon!
            {
                Logger.LogError(ex, "Failed to delete temporary WAV file during transcription.");
            }
            return result;
        }

        /// <summary>
        /// Transcribes an MP3 file.
        /// </summary>
        /// <param name="filePath">Path to the video file to transcribe.</param>
        /// <param name="fileFormat">Format of the video file to transcribe.</param>
        /// <returns>a <see cref="TranscriptionResult"/> with transcription data.</returns>
        private async Task<TranscriptionResult> TranscribeVideoFile(string filePath, FileType fileFormat)
        {
            string convertedFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            convertedFile = $"{convertedFile.Substring(0, convertedFile.Length - 4)}.wav";
            await MediaProcessingService.ExtractWav(filePath, fileFormat, convertedFile);
            TranscriptionResult result = await TranscribeWaveFile(convertedFile);

            try
            {
                File.Delete(convertedFile);
            }
            catch (Exception ex) // Pokemon!
            {
                Logger.LogError(ex, "Failed to delete temporary WAV file during transcription.");
            }
            return result;
        }

        /// <summary>
        /// Converts an MP3 file to a WAV file.
        /// </summary>
        /// <param name="mp3FilePath">Path to the MP3 file to convert.</param>
        /// <param name="waveFilePath">Path to the WAV file to create.</param>
        private static void ConvertMp3ToWav(string mp3FilePath, string waveFilePath)
        {
            using Mp3FileReader mp3 = new Mp3FileReader(mp3FilePath);
            using WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3);
            WaveFileWriter.CreateWaveFile(waveFilePath, pcm);
        }

        #endregion
    }
}