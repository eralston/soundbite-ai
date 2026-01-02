using Masticore;
using Masticore.Queue;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// NOTE: I am using this soley to kick off the video encoding process. It uses durable functions stuff
    /// that I do not have time to shoehorn into this project or extract out into another project.  Basically,
    /// this is used to add a message to the queue and then the Azure function with a queue trigger for
    /// video encoding dequeues and processes the message. There is no implementation in this job.
    /// </summary>
    public class SbVideoEncodingJob : JobBase
    {
        public const string QueueName = "q-sbvideoencoding";

        #region Properties

        public EncodeClipRequest Request { get; set; }

        #endregion

        #region Constructor

        public SbVideoEncodingJob()
        {
        }

        public SbVideoEncodingJob(EncodeClipRequest request)
        {
            Request = request;
        }

        #endregion

        #region JobBase Implementation

        public override string Message => Request.ToLowerCamelJson();

        /// <inheritdoc/>
        public override Task Process()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}