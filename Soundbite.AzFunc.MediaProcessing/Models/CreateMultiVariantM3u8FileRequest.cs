using System.Collections.Generic;

namespace Soundbite.AzFunc.MediaProcessing
{
    public class CreateMultiVariantM3u8FileRequest
    {
        public EncodeClipRequest EncodeClipRequest { get; set; }
        public IList<EncodeVariantStreamResult> QualityLevelResults { get; set; }
    }
}
