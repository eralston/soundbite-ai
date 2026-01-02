import React, { useState, useEffect, useRef } from "react";
import { observer } from "mobx-react-lite";

import { ClipWithContributor } from "@soundbite/api";

import { Loader, PlayButton, ShowWhen } from "./controls";
import { DownloadBtn } from "./controls/DownloadBtn";
import { IMediaPlayerContext } from "./video/context/IMediaPlayerContext";
import { MediaPlayerContextFactory } from "./video/context/MediaPlayerContextFactory";
import { Scrubber2 } from "./SoundScrubber2";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/

interface IProps {
  clip?: ClipWithContributor; // When specifies this takes precident over mediaUrl
  mediaUrl?: string;
  videoElement?: HTMLVideoElement | null;
  isPublic: boolean;

  /** Called when the player goes through its lifecycle and the duration of the clip changes from unknown to known */
  onDuration?: (duration?: number) => void;
  onFirstPlay?: () => void;
  onMediaError?: (ex: Error) => void;
  onContext?: (context: IMediaPlayerContext) => void;

  allowDownload?: boolean;
  autoPlay?: boolean;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/

/**
 * A simple player with a sound scrubber and play/pause button
 * @param props
 */
export const Player: React.FC<IProps> = observer((props: IProps) => {
  //////////[ State ]///////////////////////////////////////////////////////////////////////////////

  const mediaUrl = props.clip?.url ?? props.mediaUrl;

  const [isFirstPlay, setIsFirstPlay] = useState<boolean>(true);
  const [mediaType, setMediaType] = useState<string>(
    MediaPlayerContextFactory.getTypeName(props.clip, props.mediaUrl)
  );
  const context = useRef<IMediaPlayerContext | undefined>(
    MediaPlayerContextFactory.getContext(props.clip, props.mediaUrl)
  );

  const isShowDownload = mediaUrl != null && props.allowDownload === true;
  const scrubberWidth = isShowDownload ? "8" : "10";

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  // Initial loading
  useEffect(() => {
    commonContextSetup();

    // Make sure to stop playback if the player unloads
    return () => {
      context?.current?.pause();
    };
  }, []);

  // Reload when when clip or media URL changes
  useEffect(() => {
    // Determine whether the clip/media url has changed and setup context appropriately
    const newMediaType = MediaPlayerContextFactory.getTypeName(
      props.clip,
      props.mediaUrl
    );
    if (newMediaType != mediaType) {
      setMediaType(newMediaType);
      const newContext: IMediaPlayerContext | undefined =
        MediaPlayerContextFactory.getContext(props.clip, props.mediaUrl);
      context.current = newContext;
      commonContextSetup();
    }
  }, [props.clip, props.mediaUrl]);

  useEffect(() => {
    if (props.videoElement) {
      if (context.current) {
        context.current.mediaElement = props.videoElement;
        context.current.load(
          props.clip,
          props.mediaUrl,
          props.autoPlay,
          props.isPublic
        );
      }
    }
  }, [props.videoElement]);

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Responsible for setting common context configurations.  Context must be setup during initial
   * load and when mediaUrl or clip changes. Common logic should be here to avoid duplication and
   * to avoid accidentially implementing in one place but not the other.
   */
  function commonContextSetup(): void {
    if (context.current) {
      context.current.onMediaError = props.onMediaError;
      context.current.onPlay = () => {
        if (isFirstPlay) {
          setIsFirstPlay(false);
          if (props.onFirstPlay) {
            props.onFirstPlay();
          }
        }
      };

      if (mediaUrl) {
        if (context.current.hasMediaElement) {
          context.current.load(
            props.clip,
            props.mediaUrl,
            props.autoPlay ?? false
          );
        }
      }

      if (props.onContext) {
        props.onContext(context.current);
      }
    }
  }

  //////////[ Build UI ]////////////////////////////////////////////////////////////////////////////

  function GetScrubber() {
    if (context.current) {
      return <Scrubber2 context={context.current} />;
    } else {
      return <div>No Audio Context</div>;
    }
  }

  return (
    <Loader
      isLoadedWhen={props.mediaUrl != null || props.clip != null}
      isSmall={true}
    >
      <React.Fragment>
        <div className="mx-1 px-0 flex-grow-1 align-self-center">
          <GetScrubber />
        </div>
        <ShowWhen is={isShowDownload && !context.current?.hasError}>
          <div className="mx-1 px-0 align-self-center">
            <DownloadBtn mediaUrl={mediaUrl} outline={true} asBlob={true} />
          </div>
        </ShowWhen>
        <div className="ml-1 px-0 align-self-center">
          <PlayButton
            context={context.current}
            disabled={context.current?.hasError}
          />
        </div>
      </React.Fragment>
    </Loader>
  );
});
