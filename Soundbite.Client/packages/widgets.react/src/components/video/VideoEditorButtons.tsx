import { observer } from "mobx-react-lite";
import React, { useEffect } from "react";
import { faRedo } from "@fortawesome/free-solid-svg-icons";

import { RecorderState } from "./RecorderState";
import { VideoEditorService } from "./VideoEditorService";
import { VideoEditorContext } from "./VideoEditorContext";
import { SbButton, SbButtonSize, SbButtonType } from "../SbButton";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  Context?: VideoEditorContext;
  onReset?: () => void;
}

/**
 * VideoEditor is the top-level editor component for users to create and manipulate videos.
 * @param props
 */
export const VideoEditorButtons: React.FC<IProps> = observer(
  (props: IProps) => {
    //////////[ State ]///////////////////////////////////////////////////////////////////////////////

    const noContext = props.Context ? true : false;
    const context: VideoEditorContext = props.Context
      ? props.Context
      : new VideoEditorContext();

    //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

    useEffect(() => {}, [props.Context]);

    //////////[ Utility Methods ]/////////////////////////////////////////////////////////////////////

    //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

    /**
     * Deletes the current video and allows the user to re-record a video.
     */
    function onResetVideo(): void {
      context.removeClipAt(0);
      context.showRecorder();
      if (props.onReset) {
        props.onReset();
      }
    }

    async function onTrimStart(): Promise<void> {
      const clip = props.Context?.videoClips[0];
      if (context.VideoElement && clip && context.VideoElement) {
        if (!context.VideoElement.paused) {
          context.VideoElement.pause();
        }
        await VideoEditorService.trimStart(
          clip,
          context.VideoElement.currentTime
        );
        context.VideoElement.src = clip.url ?? "";
        context.VideoElement.currentTime = 0;
        context.VideoElement.load();
      }
    }

    function onTrimEnd(): void {}

    //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

    /**
     * Renders the video editor.
     */
    function Render() {
      return (
        <React.Fragment>
          {/* 
          <SbButton
            type={SbButtonType.PrimaryOutline}
            size={SbButtonSize.Small}
            icon={faChevronLeft}
            title={"Trim Start"}
            onClick={() => onTrimStart()}
            disabled={false}
          />
          */}
          <SbButton
            type={SbButtonType.PrimaryOutline}
            size={SbButtonSize.Large}
            icon={faRedo}
            title={"Reset Recording"}
            onClick={() => onResetVideo()}
            disabled={false}
          />
          {/*
          <SbButton
            type={SbButtonType.PrimaryOutline}
            size={SbButtonSize.Small}
            icon={faChevronRight}
            title={"Trim End"}
            onClick={() => onTrimEnd()}
            disabled={false}
          />
          */}
        </React.Fragment>
      );
    }

    return Render();
  }
);
