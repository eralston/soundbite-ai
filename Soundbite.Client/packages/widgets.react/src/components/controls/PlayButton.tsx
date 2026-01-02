/** @jsx jsx */
import { jsx } from "@emotion/react";
import { observer } from "mobx-react-lite";
import { IMediaPlayerContext } from "../video/context/IMediaPlayerContext";
import { SbProgressButton } from "./SbProgressButton";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  onToggle?: (isPlaying: boolean) => void;
  disabled?: boolean;
  context?: IMediaPlayerContext;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/

export const PlayButton: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  /**
   * Handles the click event of the play button.
   */
  function onClick() {
    const isPlaying = props?.context?.isPlaying === true;
    if (isPlaying) {
      props.context?.pause();
    } else {
      props.context?.play();
    }
    if (props.onToggle) {
      props.onToggle(isPlaying);
    }
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  const title = props.context?.isPlaying === true ? "Pause" : "Play";

  return (
    <div className="sb-play-button">
      <SbProgressButton
        onClick={onClick}
        isRunning={props.context?.isPlaying === true}
        allowClickOnLoading={true}
        disabled={props.disabled}
        title={title}
      />
    </div>
  );
});
