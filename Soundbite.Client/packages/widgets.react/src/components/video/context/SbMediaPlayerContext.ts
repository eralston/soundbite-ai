import canAutoPlay from "can-autoplay";

// HLS Video Player
// GitHub Home - https://github.com/video-dev/hls.js
// Design Info (Events) - https://github.com/video-dev/hls.js/blob/master/docs/design.md
// Demo - https://hlsjs.video-dev.org/demo/

import {
  ClipWithContributor,
  Logger,
  SoundbiteApiConfig
} from "@soundbite/api";

import { WidgetStore } from "../../../store";
import { MediaPlayerContext } from "./MediaPlayerContext";

// References the Hls library
let Hls: any = (window as any).Hls;

/**
 * Soundbite Media Player implementation (for use with azure storage hosted HTfiles)
 */
export class SbMediaPlayerContext extends MediaPlayerContext {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  hlsPlayer: any;
  isNative: boolean = false;

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  constructor() {
    super("hlsstreaming");
  }

  //////////[ Overrides ]///////////////////////////////////////////////////////////////////////////

  public jumpTo(time: number): void {
    if (this.mediaElement) {
      this.mediaElement.currentTime = time;
    }
  }

  public async load(
    clip?: ClipWithContributor,
    mediaUrl?: string,
    autoPlay?: boolean,
    isPublic?: boolean
  ): Promise<void> {
    if (clip) {
      this.isPublic = isPublic ?? false;

      // Validate that we have an Hls reference
      Hls = Hls ?? (window as any).Hls;
      if (!Hls) {
        const msg = "Hls library is not loaded or is otherwise inexcessible.";
        Logger.LogError(msg);
        throw new Error(msg);
      }

      // Acquire the URL
      if (mediaUrl == undefined || mediaUrl == "" || mediaUrl == null) {
        mediaUrl = SoundbiteApiConfig.ApiPrefixUrl + `/media/clip/${encodeURIComponent(clip.route)}/playlist.m3u8`;
      }

      //////////////////////////////////////////////////////////////////////////////////////////////
      // The possibility exists to play files natively, but we are currently relying on the HLS
      // event model to inject our tokens into the playlists.  So native playback will not work.
      // If we switched to injecting tokens on the server side native playback may work, but if
      // all targeted devices can play the HLS player then we can get reliable playback data from
      // the HLS player for metrics.  Leaving the code below for now in case it becomes useful in
      // the future or if we determine the HLS player doesn't work on some devices and are forced
      // to use the native player.
      //////////////////////////////////////////////////////////////////////////////////////////////
      // // Determine whether the browser has the native ability to play an apple formatted M3U8 file
      // if (this.mediaElement.canPlayType("application/vnd.apple.mpegurl")) {
      //   this.isNative = true;
      //   this.initializeVideoElement(autoPlay ?? false);
      //   this.mediaElement.src = mediaUrl;
      // }
      //////////////////////////////////////////////////////////////////////////////////////////////

      const token = await SoundbiteApiConfig.getToken()
      Hls.DefaultConfig.xhrSetup = function (xhr: any, url: any) {
        const isApiCall = url.includes(SoundbiteApiConfig.ApiPrefixUrl);
        if (isApiCall) {
          xhr.setRequestHeader("Authorization", "Bearer " + token);
        }
      }

      if (Hls.isSupported()) {
        this.isNative = false;
        this.initializeHlsPlayer(autoPlay ?? false);
        this.hlsPlayer = new Hls();

        this.hlsPlayer.loadSource(mediaUrl);
        this.hlsPlayer.attachMedia(this.mediaElement);
      } else {
        // No way to play video so log/throw an exception
        const msg = "Hls and Native Video is not supported. Cannot play video.";
        Logger.LogError(msg);
        throw new Error(msg);
      }
    }
  }

  public play(): void {
    if (this.mediaElement) {
      this.mediaElement.play();
      this.isPlaying = true;
      if (this.onPlay) {
        this.onPlay();
      }
    }
  }

  public pause(): void {
    if (this.mediaElement) {
      this.mediaElement.pause();
      this.isPlaying = false;
    }
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  private initializeHlsPlayer(autoPlay: boolean): void {
    this.initializeVideoElement(autoPlay);
  }

  private initializeVideoElement(autoPlay: boolean): void {
    if (this.mediaElement) {
      // Setup auto play (if requested)
      this.mediaElement.addEventListener("canplay", async () => {
        const autoPlayAllowed = await this.autoPlayAllowed();
        if (autoPlay && autoPlayAllowed) {
          this.play();
        }
      });

      // Handle durationchange event
      this.mediaElement.addEventListener("durationchange", () => {
        this.handleDurationChange();
      });

      // Handle timeupdate event
      this.mediaElement.addEventListener("timeupdate", () => {
        this.handleTimeUpdate();
      });

      // Handle play event
      this.mediaElement.addEventListener("play", () => {
        this.handleOnPlay();
      });

      // Handle pause event
      this.mediaElement.addEventListener("pause", () => {
        this.handleOnPause();
      });

      // Handle ended event
      this.mediaElement.addEventListener("ended", () => {
        this.handleEnded();
      });
    } else {
      const msg =
        "Cannot initialize video element because the Video element is not referenced.";
      Logger.LogError(msg);
      throw new Error(msg);
    }
  }

  private handleTimeUpdate(): void {
    this.time = this.mediaElement.currentTime;
  }

  private handleDurationChange(): void {
    const newDuration = this.mediaElement.duration;
    if (!Number.isNaN(newDuration) && Number.isFinite(newDuration)) {
      this.duration = newDuration;
    }
  }

  private handleOnPlay(): void {
    if (!this.isPlaying) {
      this.isPlaying = true;
    }
  }

  private handleOnPause(): void {
    if (this.isPlaying) {
      this.isPlaying = false;
    }
  }

  private handleEnded(): void {
    // Ended can fire when the end of the media is encountered OR the data stream stops. When
    // encoutering the end of the video, the Azure Media Player seems to rewind the video to
    // the zero mark.  I have NOT encountered a data stream termianting the player, so my
    // ASSUMPTION is that the player does not reset when encountering a data issue, but I may
    // very well be wrong.
    if (this.mediaElement.currentTime === 0) {
      if (this.onPlaybackComplete) {
        this.onPlaybackComplete();
      }
    }
  }
}
