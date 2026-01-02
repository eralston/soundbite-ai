using Masticore;
using Masticore.Media;

namespace Soundbite.Models.JobRequests
{
    /// <summary>
    /// Represents a request for processing media for a clip.
    /// </summary>   
    public class AzureTransformRequest : MediaOperationRequest
    {
        public string InputAssetName { get; set; }
        public string OutputAssetName { get; set; }
        public string OutputAssetContainer { get; set; }
        public string FileNameNoExt { get; set; }
        public AzureTransform Transform { get; set; }

        public override void Validate()
        {
            base.Validate();
            Validator.NotNullOrEmpty(nameof(InputAssetName), InputAssetName, "Azure Transform request is invalid because InputAssetName was not specified.");
            Validator.NotNullOrEmpty(nameof(OutputAssetName), OutputAssetName, "Azure Transform  request is invalid because OutputAssetName was not specified.");
            Validator.NotNullOrEmpty(nameof(FileNameNoExt), FileNameNoExt, "Azure Transform  request is invalid because FileNameNoExt was not specified.");
            Validator.NotNull(nameof(Transform), Transform, "Azure Transform request is invalid because Transform was not specified.");
        }
    }
}