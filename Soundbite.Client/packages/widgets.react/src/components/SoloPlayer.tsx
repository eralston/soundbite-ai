import React, { useState } from "react";
import {
  ClipType,
  ClipWithContributor,
  SessionDetails,
  SessionSecurityType,
  SoundbiteApiConfig,
  Utils,
} from "@soundbite/api";

import { Player } from "./Player";
import { Loader, ShowWhen } from "./controls";
import { IMediaPlayerContext } from "./video/context/IMediaPlayerContext";

interface IProps {
  session?: SessionDetails;
  allowDownload?: boolean;
  onFirstPlay?: () => void;
  onMediaError?: (err: Error) => void;
  autoPlay?: boolean;
  onContext?: (context: IMediaPlayerContext) => void;
}

export const SoloPlayer: React.FC<IProps> = (props: IProps) => {
  // Not all clips are valid for display
  const allValidClips: ClipWithContributor[] =
    props.session?.prompts[0].clips?.filter(
      (i) =>
        i.clipType != ClipType.RawClip && i.clipType != ClipType.RawTranscript
    ) ?? [];

  const clip: ClipWithContributor = allValidClips[0];
  const displayName = Utils.userDisplay(clip.contributor);
  const imageUrl = clip.contributor.imageSrc ?? SoundbiteApiConfig.imgAvatarUrl;
  const videoRef = React.useRef(null);

  const [isVideo, setIsVideo] = useState<boolean>(false);
  const [isVideoLoading, setIsVideoLoading] = useState(true);

  /**
   * Handles event that fires when player establishes a media context. This helps determine whether
   * the video components should be displayed to users.
   */
  function onContext(context: IMediaPlayerContext): void {
    // Set media duration
    context.duration = clip.seconds;

    // Setup Video (if applicable)
    const isVideoValue = context.isVideo;
    if (isVideo != isVideoValue) {
      setIsVideo(isVideoValue);
    }

    if (props.onContext) {
      props.onContext(context);
    }
  }

  function onLoaded() {
    setIsVideoLoading(false);
  }

  function onLoading() {
    setIsVideoLoading(true);
  }

  return (
    <Loader isLoadedWhen={props.session != null}>
      <ShowWhen is={isVideo} isDisplayBased={true}>
        <Loader isDisplayBased={true} isLoadedWhen={!isVideoLoading || true}>
          <video
            className="sb-video mb-2"
            ref={videoRef}
            webkit-playsinline={true.toString()}
            playsInline={true}
            onLoadStart={onLoading}
            onLoadedMetadata={onLoaded}
          />
        </Loader>
      </ShowWhen>
      <div className="sb-soloplayer sb-cover-modal-body d-flex flex-row">
        <ShowWhen is={clip != null}>
          <div className="d-none d-sm-block align-self-center mr-1 px-0">
            <span className="avatar rounded-circle">
              <img alt="Avatar" src={imageUrl} />
            </span>
          </div>
          <div className="align-self-center mx-1 px-0">
            <h4 className="mb-0">{displayName}</h4>
          </div>
          <Player
            clip={clip}
            allowDownload={props.allowDownload}
            videoElement={videoRef?.current}
            onContext={onContext}
            onFirstPlay={props.onFirstPlay}
            onMediaError={props.onMediaError}
            isPublic={
              props.session?.sessionSecurity == SessionSecurityType.Public
            }
            autoPlay={props.autoPlay}
          />
        </ShowWhen>
        <ShowWhen is={clip === undefined}>
          <div>
            <h1>Unable to Load Clip for Session {props.session?.name}</h1>
          </div>
        </ShowWhen>
      </div>
    </Loader>
  );
};
