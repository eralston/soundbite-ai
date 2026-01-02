import { observer } from "mobx-react-lite";
import React, { useEffect } from "react";
import {
  faVideo,
  faCameraRotate,
  faStop,
} from "@fortawesome/free-solid-svg-icons";

import { RecorderState } from "./RecorderState";
import { VideoEditorContext } from "./VideoEditorContext";
import { SbButton, SbButtonSize, SbButtonType } from "../SbButton";
import { Logger } from "@soundbite/api";
import { ShowWhen } from "../controls";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  Context?: VideoEditorContext;
}

/**
 * VideoEditor is the top-level editor component for users to create and manipulate videos.
 * @param props
 */
export const VideoRecorderButtons: React.FC<IProps> = observer(
  (props: IProps) => {
    //////////[ State ]///////////////////////////////////////////////////////////////////////////////

    const context: VideoEditorContext = props.Context
      ? props.Context
      : new VideoEditorContext();

    //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

    useEffect(() => {}, [props.Context]);

    //////////[ Utility Methods ]/////////////////////////////////////////////////////////////////////

    //function isRecordDisabled(): boolean {
    //  return !noContext || !context.isWebCamOn;
    //}

    //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

    //function onToggleWebCam() {
    //  if (context.isWebCamOn) {
    //    context.stopWebCam();
    //  } else {
    //    context.startWebCam();
    //  }
    //}

    function onToggleRecording() {
      switch (context.recorderState) {
        case RecorderState.NotRecording:
          context.startRecording();
          break;
        case RecorderState.Recording:
          context.stopRecording();
          break;
        case RecorderState.Paused:
          context.resumeRecording();
          break;
      }
    }

    function onStopRecording() {
      if (context.isRecording || context.isPaused) {
        context.stopRecording();
      } else {
        Logger.LogWarning(
          "Cannot stop recording because component is not currently recording."
        );
      }
    }

    /**
     * Deletes the current video buffer and allows the user to re-record a video.
     */
    //function onResetVideo() {
    //  context.clearVideoBuffer();
    //}

    function onSwitchCameras() {
      context.switchWebCam();
    }

    //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

    /**
     * Renders the video editor.
     */
    function Render() {
      return (
        <React.Fragment>
          <div className="">
            {/*
          <SbButton
            type={SbButtonType.PrimaryOutline}
            size={SbButtonSize.Small}            
            icon={faVideo}
            title={context.isWebCamOn ? "Turn Camera Off" : "Turn Camera On"}
            onClick={() => onToggleWebCam()}
            disabled={!context}
          />
          // There is a bug where hitting reset while recording causes a crash on the element - or something
          // TODO: Fix it so this can be called any time
          <ShowWhen is={false} isDisplayBased={true}>
            <SbButton
              type={SbButtonType.PrimaryOutline}
              size={SbButtonSize.Small}
              icon={faRedo}
              title={"Reset Recording"}
              onClick={() => onResetVideo()}
            />
          </ShowWhen>
          */}
            <ShowWhen
              is={VideoEditorContext.isMultiDevice && !context.isRecording}
            >
              <SbButton
                type={SbButtonType.PrimaryOutline}
                size={SbButtonSize.Small}
                icon={faCameraRotate}
                title={"Switch Cameras"}
                onClick={() => onSwitchCameras()}
              />
            </ShowWhen>
            <SbButton
              type={SbButtonType.PrimaryOutline}
              size={SbButtonSize.Large}
              icon={context.isRecording ? faStop : faVideo}
              title={context.isRecording ? "Stop Recording" : "Record"}
              onClick={() => onToggleRecording()}
              disabled={!context}
            />

            {/*<ShowWhen is={context.hasUnsavedRecording && !context.isRecording}>*/}
            {/*  <SbButton*/}
            {/*    type={SbButtonType.PrimaryOutline}*/}
            {/*    size={SbButtonSize.Small}*/}
            {/*    icon={faPlay}*/}
            {/*    title={"Preview"}*/}
            {/*    onClick={() => onStopRecording()}*/}
            {/*  />*/}
            {/*</ShowWhen>*/}
          </div>
        </React.Fragment>
      );
    }

    return Render();
  }
);
