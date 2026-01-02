namespace Soundbite
{
    public class EncodeSegmentRequest
    {
        public EncodeSegmentRequest(EncodeVariantStreamRequest qualityLevelRequest, string segmentName)
        {
            QualityLevelRequest = qualityLevelRequest;
            SegmentName = segmentName;
        }

        public EncodeVariantStreamRequest QualityLevelRequest { get; set; }
        public string SegmentName { get; set; }
    }
}
