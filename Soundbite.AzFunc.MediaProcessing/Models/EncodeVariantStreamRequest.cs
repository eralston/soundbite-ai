namespace Soundbite
{
    public class EncodeVariantStreamRequest
    {
        #region Properties

        public EncodeClipRequest ClipRequest { get; set; }

        public string QualityLevelName { get; set; }

        public string[] Segments { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Rotation { get; set; }

        #endregion

        #region Constructor

        public EncodeVariantStreamRequest(EncodeClipRequest clipRequest, string qualityLevelName, string[] segments, int width, int height, int rotation)
        {
            ClipRequest = clipRequest;
            QualityLevelName = qualityLevelName;
            Segments = segments;
            Height = height;
            Width = width;
            Rotation = rotation;
        }

        #endregion
    }
}
