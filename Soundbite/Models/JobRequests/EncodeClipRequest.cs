using Masticore.Media;
using Soundbite.Models.JobRequests;

namespace Soundbite
{
    public class EncodeClipRequest : MediaOperationRequest
    {
        #region Properties - Request

        public string OUID { get; set; }
        public string ClipOpRoute { get; set; }
        public string UniqueFolderId { get; set; }
        public string ClipFileName { get; set; }

        #endregion

        #region Properties - Operations

        //TODO: These are using during processing but do not need to be set for the request.
        //      at some point they could be refactored away from this method

        public string AzureContainer { get; set; }
        public string AzureBlobPath { get; set; }
        public string LocalClipFilePath { get; set; }

        /// <summary>
        /// Media details for the parent clip that this will be encoded from
        /// </summary>
        public MediaDetails SourceMediaDetails { get; set; }

        #endregion
    }
}
