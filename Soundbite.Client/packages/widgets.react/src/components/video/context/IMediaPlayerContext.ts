import { ClipWithContributor } from "@soundbite/api";

/**
 * MediaPlayerContext represents media playback operations and player state information.
 */
export interface IMediaPlayerContext {
  isPlaying: boolean;
  pause(): void;
  play(): void;
  jumpTo(time: number): void;

  onPlaybackComplete?: () => void;
  load(
    clip?: ClipWithContributor,
    mediaUrl?: string,
    autoPlay?: boolean,
    isPublic?: boolean
  ): Promise<void>;
  duration: number;
  isPublic: boolean;

  readonly timeSummary: string;

  hasError: boolean;
  time: number;

  readonly isVideo: boolean;

  /**
   * Determines whether the media element has been set.  Accessing the mediaElement property throws
   * an exception if the media element has not been set.  This property allows for the determination
   * of whether the media element is present without throwing an exception.
   */
  readonly hasMediaElement: boolean;

  /**
   * Gets/sets a reference to the HTML media element used for media playback operations.
   */
  mediaElement: HTMLMediaElement;

  /**
   * Called if an error occurs with audio player.
   */
  onMediaError?: (error: Error) => void;

  /**
   * Called whenever media begins playing.
   */
  onPlay?: () => void;
}
