import canAutoPlay from "can-autoplay";

import {
  ClipWithContributor,
  Logger,
  OrganizationsService,
  SoundbiteApiConfig,
  Utils,
} from "@soundbite/api";

import { WidgetStore } from "../../../store";
import { MediaPlayerContext } from "./MediaPlayerContext";

// References the Hls library
let Hls: any = (window as any).Hls;

/**
 * Azure Media Player (AMP) IMediaPlayerContext implementation
 */
export class HlsMediaPlayerContext extends MediaPlayerContext {
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
      mediaUrl = Utils.isNullOrEmpty(mediaUrl)
        ? await this.buildMediaUrl(clip)
        : (mediaUrl as string);

      // Determine whether the browser has the native ability to play an apple formatted M3U8 file
      if (this.mediaElement.canPlayType("application/vnd.apple.mpegurl")) {
        this.isNative = true;
        this.initializeVideoElement(autoPlay ?? false);
        this.mediaElement.src = mediaUrl;
      } else if (Hls.isSupported()) {
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

  /**
   * Builds out the URL to the M38u proxy API call
   */
  private async buildMediaUrl(clip: ClipWithContributor): Promise<string> {
    // Acquire the Azure Media Service url prefix
    let prefix =
      WidgetStore.organizations.currentOrg?.settings.azure
        .streamingEndpointUrlPrefix;

    // If we do not have a prefix it may simply mean that we are accessing the public player
    // anonymously or via a route that does not load organization settings.  When this occurs,
    // attempt to load the AMS media URL using the current organization route.
    if (
      Utils.isNullOrEmpty(prefix) &&
      WidgetStore.organizations.currentOrgRoute != null
    ) {
      prefix = await OrganizationsService.readAmsStreamingUrl(
        WidgetStore.organizations.currentOrgRoute
      );

      if (Utils.isNullOrEmpty(prefix)) {
        const msg =
          "Organization does not have an AMS Streaming URL prefix configured or the setting could not be obtained.";
        Logger.LogError(msg);
        throw new Error(msg);
      }
    }

    if (
      Utils.isNullOrEmpty(prefix) &&
      Utils.isNullOrEmpty(WidgetStore.organizations.currentOrgRoute)
    ) {
      const msg =
        "Canont build media URL because the current organization route is unknown.";
      Logger.LogError(msg);
      throw new Error(msg);
    }

    // Make sure we have a valid prefix
    if (Utils.isNullOrEmpty(prefix)) {
      const msg =
        "Cannot build media URL because organization does not have an Azure Media URL prefix defined.";
      Logger.LogError(msg);
      throw new Error(msg);
    }

    // Build the URL - we are currently always asking for an apple formatted M3U8 file
    const azureMediaUrl = `${prefix}${clip?.hostingData}/clip.ism/manifest(format=m3u8-aapl)`;

    // Determine if this is a public soundbite (i.e. not encrypted)
    if (this.isPublic) {
      // Send the direct azure media service URL
      return azureMediaUrl;
    } else {
      // Send the proxy URL
      return await this.buildMediaProxyUrl(
        clip,
        prefix as string,
        azureMediaUrl
      );
    }
  }

  private async buildMediaProxyUrl(
    clip: ClipWithContributor,
    prefix: string,
    azureMediaUrl: string
  ): Promise<string> {
    const token = (await SoundbiteApiConfig.getToken()) as string;

    // Ensure the token is present
    if (Utils.isNullOrEmpty(token)) {
      const msg = "Cannot build media URL without Soundbite token.";
      Logger.LogError(msg);
      throw new Error(msg);
    }

    // Ensure the org route is loaded
    if (!prefix && !WidgetStore.organizations.currentOrg) {
      prefix = await OrganizationsService.readAmsStreamingUrl(
        WidgetStore.organizations.currentOrgRoute as string
      );
    }

    const azureMediaUrlEnc = encodeURI(azureMediaUrl)
      .replace("(", "%28")
      .replace(")", "%29");
    const mediaUrl = `${
      SoundbiteApiConfig.ApiPrefixUrl
    }/mediaproxy/manifest?sourceUrl=${azureMediaUrlEnc}&token=${encodeURIComponent(
      token
    )}`;
    return mediaUrl;
  }

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
