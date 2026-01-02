import React, { useEffect, useState } from "react";
import { VideoClip } from "./VideoClip";
import { VideoEditorContext } from "./VideoEditorContext";
import { VideoEditorButtons } from "./VideoEditorButtons";
import { VideoRecorderButtons } from "./VideoRecorderButtons";
import { observer } from "mobx-react-lite";
import { Loader } from "../controls";
import { GlobalTheme } from "../../styles/GlobalTheme";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  onRecorded?: (file: VideoClip) => void;
  onReset?: () => void;
  className?: string;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
/**
 * VideoEditor is the top-level editor component for users to create and manipulate videos.
 * @param props
 */
export const VideoEditor: React.FC<IProps> = observer((props: IProps) => {
  //////////[ State ]///////////////////////////////////////////////////////////////////////////////

  const [editorContext] = useState<VideoEditorContext>(
    new VideoEditorContext()
  );
  const videoRef = React.createRef<HTMLVideoElement>();
  const [isVideoLoading, setIsVideoLoading] = useState(true);

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  useEffect(() => {
    // It has not which implies this is a new context
    editorContext.VideoElement = videoRef.current;
    editorContext.onClipAdded = (clip: VideoClip) => {
      editorContext.showEditor();
      editorContext.playClip(clip);
      if (props.onRecorded) {
        props.onRecorded(clip);
      }
    };
    editorContext.showRecorder();
    editorContext.startWebCam();

    return function cleanup() {
      editorContext.stopWebCam();
    };
  }, []);

  //////////[ Utility Methods ]/////////////////////////////////////////////////////////////////////

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onLoaded() {
    setIsVideoLoading(false);
  }

  function onLoading() {
    setIsVideoLoading(true);
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  /**
   * Renders a message informing the user that video recording is not supported by the browser.
   */
  function VideoNotSupported() {
    return (
      <React.Fragment>
        This browser does not appear to support video recording. Please upload a
        video or use a different browser.
      </React.Fragment>
    );
  }

  /**
   * Renders the video editor.
   */
  function VideoSupported() {
    const isEditing = editorContext?.isEditing === true;
    const backColor = GlobalTheme.current.colors.neutrals.n900;

    return (
      <React.Fragment>
        <Loader
          isLoadedWhen={!isVideoLoading}
          isDisplayBased={true}
          style={{ height: "184px" }}
          className={props.className}
        >
          <video
            autoPlay={true}
            playsInline={true}
            className="rounded"
            controls={isEditing}
            ref={videoRef}
            style={{
              width: "100%",
              height: "100%",
              backgroundColor: backColor,
              transform: !editorContext.isEditing ? "scaleX(-1)" : "",
            }}
            onLoadStart={onLoading}
            onLoadedMetadata={onLoaded}
          ></video>
          <div className="d-flex align-items-center justify-content-center">
            {editorContext?.isEditing === true ? (
              <VideoEditorButtons
                Context={editorContext}
                onReset={props.onReset}
              />
            ) : (
              <VideoRecorderButtons Context={editorContext} />
            )}
          </div>
        </Loader>
      </React.Fragment>
    );
  }

  return VideoEditorContext.isVideoCaptureSupported()
    ? VideoSupported()
    : VideoNotSupported();
});
