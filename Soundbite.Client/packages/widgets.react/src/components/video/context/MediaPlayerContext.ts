import { ClipWithContributor, PublicError, Utils } from "@soundbite/api";
import { makeObservable, observable, computed, runInAction } from "mobx";
import moment from "moment";
import canAutoPlay from "can-autoplay";

import { ErrorDlg } from "../../ErrorDlg";
import { IMediaPlayerContext } from "./IMediaPlayerContext";

/**
 * MediaPlayerContext represents media playback operations and player state information.
 */
export class MediaPlayerContext implements IMediaPlayerContext {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  /**
   * Stores a reference to the media element responsible for "playing" the media.  In an audio
   * context the media element is automatically created because it does not require a visible
   * location in the DOM. In a video context, the element must be made known to the context because
   * the context cannot automatically place the element in the DOM.
   */
  private _mediaElement?: HTMLMediaElement;

  /**
   * Stores a reference to the playback timer that automatically increments the playback time.
   */
  private _timer?: NodeJS.Timer;

  /**
   * Flag indicating whether media should reset to start when next played.  Normally set when media
   * plays to end so next play resets to beginning of video.
   */
  private resetOnPlay: boolean = false;

  /**
   * Flag indicating that when media starts playing it should jump to the context time.
   */
  private jumpToContextTime: boolean = false;

  //////////[ State ]///////////////////////////////////////////////////////////////////////////////

  public duration: number = 0;
  public isPublic: boolean = false;
  public mediaUrl: string = "";
  public isPlaying: boolean = false;
  public isMediaLoaded: boolean = false;
  public hasError: boolean = false;
  public supportsScrubbing: boolean = true;
  public error: Error = { name: "", message: "" };
  public time: number = 0;
  public mode: "audio" | "video" | "azurestreaming" | "hlsstreaming";
  public isAutoPlayAllowed?: boolean = undefined;

  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  /**
   * Determines whether the media element has been set.  Accessing the mediaElement property throws
   * an exception if the media element has not been set.  This property allows for the determination
   * of whether the media element is present without throwing an exception.
   */
  public get hasMediaElement(): boolean {
    return !!this._mediaElement;
  }

  /**
   * Gets/sets a reference to the HTML media element used for media playback operations.
   */
  public get mediaElement(): HTMLMediaElement {
    if (this._mediaElement) {
      return this._mediaElement;
    }
    throw new Error("Media element was not set.");
  }

  /**
   * Gets/sets a reference to the HTML media element used for media playback operations.
   */
  public set mediaElement(value: HTMLMediaElement) {
    this._mediaElement = value;
  }

  /**
   * Called if an error occurs with audio player.
   */
  public onMediaError?: (error: Error) => void;

  /**
   * Called whenever media begins playing.
   */
  public onPlay?: () => void;

  /**
   * Called when the media finishes playing.
   */
  public onPlaybackComplete?: () => void;

  /**
   * Gets the time summary which consists of the current time and the total duration.
   */
  public get timeSummary(): string {
    const time = moment.duration(this.time * 1000);
    const duration = moment.duration(
      (this.duration === Infinity ? 0 : this.duration) * 1000
    );
    let timeDisplay = this.displayDuration(time, duration);
    let durationDisplay = this.displayDuration(duration, duration);
    return `${timeDisplay}/${durationDisplay}`;
  }

  public get isVideo(): boolean {
    return (
      this.mode === "video" ||
      this.mode === "azurestreaming" ||
      this.mode === "hlsstreaming"
    );
  }

  public get isAudio(): boolean {
    return this.mode === "audio";
  }

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  constructor(mode: "audio" | "video" | "azurestreaming" | "hlsstreaming") {
    this.mode = mode;

    // Setup obvservable state values
    makeObservable(this, {
      duration: observable,
      isPublic: observable,
      error: observable,
      hasError: observable,
      isMediaLoaded: observable,
      isPlaying: observable,
      mediaUrl: observable,
      mode: observable,
      supportsScrubbing: observable,
      time: observable,
      timeSummary: computed,
      isVideo: computed,
      isAudio: computed,
    });

    // Set audio element defaults
    if (mode === "audio") {
      this.mediaElement = new Audio();
      this.mediaElement.preload = "none";
      this.mediaElement.autoplay = false;
      this.mediaElement.playbackRate = 1.0;
    }
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  protected async autoPlayAllowed(): Promise<boolean> {
    if (this.isAutoPlayAllowed == null) {
      this.isAutoPlayAllowed = (await canAutoPlay.video()).result;
    }
    return this.isAutoPlayAllowed ?? false;
  }

  /**
   * Loads the specified media into the player.
   * @mediaUrl - URL of the media to load
   */
  public load(
    clip?: ClipWithContributor,
    mediaUrl?: string,
    autoPlay?: boolean,
    isPublic?: boolean
  ): Promise<void> {
    if (!clip && Utils.isNullOrEmpty(mediaUrl)) {
      throw new Error(
        "MediaPlayerContext cannot load media when both the clip and the media URL are missing."
      );
    }

    this.isPublic = isPublic ?? false;

    var localMediaUrl = clip ? clip.url : (mediaUrl as string);

    // Automatically stop the current media if it is playing
    if (!this.mediaElement.paused) {
      this.mediaElement.pause();
    }

    // Make sure to handle when the new media stops
    this.mediaElement.onended = this.onPlaybackCompleteInternal.bind(this);

    // Mark that media is not loaded (existing media automatically unloaded)
    runInAction(() => {
      this.isPlaying = false;
      this.isMediaLoaded = false;
    });

    this.mediaElement.src = localMediaUrl;
    this.mediaElement.load();

    return new Promise((resolve, reject) => {
      // Setup the audio error handler so it rejects the promise
      this.mediaElement.onerror = () => {
        if (this.mediaElement.error) {
          const err = new PublicError(
            new Error(this.mediaElement.error.message),
            "Unable to Load Audio",
            "Unable to load file; potentially due to incorrect file type. Please contact the host and ensure they are using the Soundbite audio recorder or uploading valid MP3 files."
          );

          // Display the error dialog to the user
          ErrorDlg.show(
            err,
            "Unable to Load Audio",
            "Unable to load file; potentially due to incorrect file type. Please contact the host and ensure they are using the Soundbite audio recorder or uploading valid MP3 files."
          );

          runInAction(() => {
            this.hasError = true;
            this.error = err;
          });

          if (this.onMediaError) {
            this.onMediaError(err);
          }
        }

        //NOTE: Resolving here instead of rejecting because error information is captured in the
        // context and can be referenced and responded to.  Rejecting causes a second dialog to
        // supercede the one above with an "unknown" error instead of useful information.
        resolve();
      };

      // Watch for the audio to signal that it has loaded far enough that
      // it believes it can play all the way through without stopping...
      this.mediaElement.oncanplaythrough = async () => {
        runInAction(() => {
          if (this.mediaElement.duration !== Infinity) {
            this.duration = this.mediaElement.duration;
          } else if (this.duration === 0) {
            this.duration = this.mediaElement.duration;
          }
          this.error = { name: "", message: "" };
          this.hasError = false;
          this.isMediaLoaded = true;
        });

        const autoPlayAllowed = await this.autoPlayAllowed();
        if (autoPlay && autoPlayAllowed) {
          this.play();
        }
        resolve();
      };
    });
  }

  /**
   * Plays the current media.
   */
  public play(): void {
    // Check if the media needs to be reset to beginning
    if (this.resetOnPlay === true) {
      this.jumpTo(0);
    } else if (!this.jumpToContextTime) {
      // When starting to play, set the context time to the media element time. This avoid an issue
      // where the delta between the two increases each time play/pause is clicked.
      runInAction(() => {
        this.time = this.mediaElement.currentTime;
      });
    }

    // Jump to the context time if the flag is set
    if (this.jumpToContextTime) {
      this.mediaElement.currentTime = this.time;
      this.jumpToContextTime = false;
    }

    // Setup timer to increment media play time each second
    this._timer = setInterval(() => {
      runInAction(() => {
        //NOTE: the time value in the context is independent of the actual media time. In theory,
        //  the two could get out of sync and require correction.  However, in testing a 15 minute
        //  run the initial delta between the two was 80ms off and all subsequent deltas were less
        //  than 10ms off in either direction (+/-). May need to test on slower machines
        this.time += 1;
      });
    }, 1000);

    // Update state
    runInAction(() => {
      this.isPlaying = true;
    });

    // Play the media
    this.mediaElement.play();

    // Fire the onPlay handler if necessary
    if (this.onPlay) {
      this.onPlay();
    }
  }

  /**
   * Pauses the current media.
   */
  public pause(): void {
    if (this.hasMediaElement) {
      this.mediaElement.pause();
    }
    runInAction(() => {
      this.isPlaying = false;
    });
    if (this._timer) {
      clearInterval(this._timer);
      this._timer = undefined;
    }
  }

  /**
   * Navigates to the beginning of the media.
   */
  public jumpToStart(): void {
    this.jumpTo(0);
  }

  /**
   * Navigates to the specified playback position in the media.
   * @param time - the time to jump to
   */
  public jumpTo(time: number): void {
    this.resetOnPlay = false;
    runInAction(() => {
      this.time = time;
      this.mediaElement.currentTime = time;
      if (this.mediaElement.currentTime != time) {
        this.jumpToContextTime = true;
      }
    });
  }

  //////////[ Methods - Utility ]///////////////////////////////////////////////////////////////////

  /**
   * Converts a moment duration value into a formatted string.
   * @param display - duration to format
   * @param duration - used to determine format for @display parameter
   */
  private displayDuration(
    display: moment.Duration,
    totalDuration: moment.Duration
  ): string {
    if (totalDuration.hours() > 0) {
      return `${Utils.pad(display.hours(), 2)}:${Utils.pad(
        display.minutes(),
        2
      )}:${Utils.pad(display.seconds(), 2)}`;
    } else {
      const minPad = totalDuration.minutes() > 9 ? 2 : 1;
      return `${Utils.pad(display.minutes(), minPad)}:${Utils.pad(
        display.seconds(),
        2
      )}`;
    }
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  private onPlaybackCompleteInternal(): void {
    this.pause();
    this.time = this.duration;
    this.resetOnPlay = true;
    if (this.onPlaybackComplete) {
      this.onPlaybackComplete();
    }
  }
}
