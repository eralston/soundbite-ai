namespace Soundbite
{
    public class SetClipOpStatusRequest
    {
        #region Properties

        public string ClipOpRoute { get; set; }
        public ClipOperationStateType ClipOpState { get; set; }

        #endregion

        #region Constructor

        public SetClipOpStatusRequest(string clipOpRoute, ClipOperationStateType clipOpState)
        {
            ClipOpRoute = clipOpRoute;
            ClipOpState = clipOpState;
        }

        #endregion
    }
}
