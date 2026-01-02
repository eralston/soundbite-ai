using Masticore.Media;

namespace Soundbite
{
    public class EncodeVariantStreamResult
    {
        public EncodeVariantStreamRequest Request { get; set; }
        public MediaDetails[] SegmentDetails { get; set; }
    }
}
