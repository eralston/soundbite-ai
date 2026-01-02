import {
  ClipWithContributor,
  OrganizationsService,
  SoundbiteApiConfig,
} from "@soundbite/api";
import { WidgetStore } from "../../../store";
import { MediaPlayerContext } from "./MediaPlayerContext";

/** NOTE: This class is currently not in use but kept here for future reference */

/**
 * Azure Media Player (AMP) IMediaPlayerContext implementation
 */
export class AzureMediaPlayerContext extends MediaPlayerContext {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  playerRef: any;

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  constructor() {
    super("azurestreaming");
  }

  //////////[ Overrides ]///////////////////////////////////////////////////////////////////////////

  public jumpTo(time: number): void {
    if (this.playerRef) {
      this.playerRef.currentTime(time);
    }
  }

  public async load(
    clip?: ClipWithContributor,
    mediaUrl?: string,
    autoPlay?: boolean,
    isPublic?: boolean
  ): Promise<void> {
    this.isPublic = isPublic ?? false;

    // Setup the Azure Media Player options
    const playerOptions = {
      controls: false,
      logo: { enabled: false },
      width: 448,
    };

    this.playerRef = (window as any).amp(
      this.mediaElement,
      playerOptions,
      async () => {
        const token = await SoundbiteApiConfig.getToken();
        let prefix =
          WidgetStore.organizations.currentOrg?.settings.azure
            .streamingEndpointUrlPrefix;

        if (!prefix && !WidgetStore.organizations.currentOrg) {
          prefix = await OrganizationsService.readAmsStreamingUrl(
            WidgetStore.organizations.currentOrgRoute as string
          );
        }

        // Update the duration if it is a number that makes sense
        this.playerRef.addEventListener("durationchange", () => {
          const newDuration = this.playerRef.duration();
          if (!Number.isNaN(newDuration) && Number.isFinite(newDuration)) {
            this.duration = newDuration;
          }
        });

        // Watch for completion of the media
        this.playerRef.addEventListener("ended", () => {
          // Ended can fire when the end of the media is encountered OR the data stream stops. When
          // encoutering the end of the video, the Azure Media Player seems to rewind the video to
          // the zero mark.  I have NOT encountered a data stream termianting the player, so my
          // ASSUMPTION is that the player does not reset when encountering a data issue, but I may
          // very well be wrong.
          if (this.playerRef.currentTime() === 0) {
            if (this.onPlaybackComplete) {
              this.onPlaybackComplete();
            }
          }
        });

        // Set is playing to false if the pause event fires.  Pause fires right before playback ends.
        this.playerRef.addEventListener("pause", () => {
          this.isPlaying = false;
        });

        // Set is playing to true if the play event fires and isPlaying is not already true. This
        // should not happen in theory but we have it here just in case.
        this.playerRef.addEventListener("play", () => {
          if (!this.isPlaying) {
            this.isPlaying = true;
          }
        });

        // Update the location of the scrubber as the player time progresses
        this.playerRef.addEventListener("timeupdate", () => {
          this.time = this.playerRef.currentTime();
        });

        // Load the streaming media source from Azure Media Services
        const sources: any = [
          {
            src: `${prefix}${clip?.hostingData}/clip.ism/manifest(format=m3u8-cmaf,encryption=cbc)`,
            //type: "application/vnd.apple.mpegurl"
            type: "application/vnd.ms-sstr+xml",
          },
        ];

        const origUrl = sources[0].src;
        const encodedOrigUrl = encodeURIComponent(origUrl.split("://")[1])
          .replace("(", "%28")
          .replace(")", "%29");
        const proxyUrl =
          SoundbiteApiConfig.ApiPrefixUrl +
          `/MediaProxy/Manifest/clip.ism/manifest(format=m3u8-cmaf,encryption=cbc)?playbackUrl=${encodedOrigUrl}&token=${token}`;
        sources[0].src = proxyUrl;

        // Include encryption / token settings for protected content
        if (!this.isPublic) {
          sources[0].protectionInfo = [
            {
              type: "AES",
              authenticationToken: `Bearer=${token}`,
            },
          ];
        }

        this.playerRef.src(sources);
      }
    );
  }

  public play(): void {
    if (this.playerRef) {
      this.playerRef.play();
      this.isPlaying = true;
      if (this.onPlay) {
        this.onPlay();
      }
    }
  }

  public pause(): void {
    if (this.playerRef) {
      this.playerRef.pause();
      this.isPlaying = false;
    }
  }

  //////////[ Overrides ]///////////////////////////////////////////////////////////////////////////

  private logAllEvents(): void {
    this.playerRef.addEventListener("canplaythrough", () => {
      console.log("[Event]-canplaythrough");
    });
    this.playerRef.addEventListener("tech-click", () => {
      console.log("[Event]-tech-click");
    });
    this.playerRef.addEventListener("complete", () => {
      console.log("[Event]-complete");
    });
    this.playerRef.addEventListener("decryptorInitialized", () => {
      console.log("[Event]-decryptorInitialized");
    });
    this.playerRef.addEventListener("disposing", () => {
      console.log("[Event]-disposing");
    });
    this.playerRef.addEventListener("downloadbitratechanged", () => {
      console.log("[Event]-downloadbitratechanged");
    });
    this.playerRef.addEventListener("durationchange", () => {
      console.log("[Event]-durationchange");
    });
    this.playerRef.addEventListener("embeddedcaptionsfound", () => {
      console.log("[Event]-embeddedcaptionsfound");
    });
    this.playerRef.addEventListener("emsgAvailable", () => {
      console.log("[Event]-emsgAvailable");
    });
    this.playerRef.addEventListener("ended", () => {
      console.log("[Event]-ended");
    });
    this.playerRef.addEventListener("error", () => {
      console.log("[Event]-error");
    });
    this.playerRef.addEventListener("errorInPlayingAd", () => {
      console.log("[Event]-errorInPlayingAd");
    });
    this.playerRef.addEventListener("exitfullscreen", () => {
      console.log("[Event]-exitfullscreen");
    });
    this.playerRef.addEventListener("firstquartile", () => {
      console.log("[Event]-firstquartile");
    });
    this.playerRef.addEventListener("fullscreen", () => {
      console.log("[Event]-fullscreen");
    });
    this.playerRef.addEventListener("fullscreenchange", () => {
      console.log("[Event]-fullscreenchange");
    });
    this.playerRef.addEventListener("livestartupretry", () => {
      console.log("[Event]-livestartupretry");
    });
    this.playerRef.addEventListener("loadeddata", () => {
      console.log("[Event]-loadeddata");
    });
    this.playerRef.addEventListener("loadedmetadata", () => {
      console.log("[Event]-loadedmetadata");
    });
    this.playerRef.addEventListener("loadstart", () => {
      console.log("[Event]-loadstart");
    });
    this.playerRef.addEventListener("midpoint", () => {
      console.log("[Event]-midpoint");
    });
    this.playerRef.addEventListener("mute", () => {
      console.log("[Event]-mute");
    });
    this.playerRef.addEventListener("pause", () => {
      console.log("[Event]-pause");
    });
    this.playerRef.addEventListener("play", () => {
      console.log("[Event]-play");
    });
    this.playerRef.addEventListener("playbackbitratechanged", () => {
      console.log("[Event]-playbackbitratechanged");
    });
    this.playerRef.addEventListener("playing", () => {
      console.log("[Event]-playing");
    });
    this.playerRef.addEventListener("ratechange", () => {
      console.log("[Event]-ratechange");
    });
    this.playerRef.addEventListener("resume", () => {
      console.log("[Event]-resume");
    });
    this.playerRef.addEventListener("rewind", () => {
      console.log("[Event]-rewind");
    });
    this.playerRef.addEventListener("seeked", () => {
      console.log("[Event]-seeked");
    });
    this.playerRef.addEventListener("seeking", () => {
      console.log("[Event]-seeking");
    });
    this.playerRef.addEventListener("skip", () => {
      console.log("[Event]-skip");
    });
    this.playerRef.addEventListener("sourceset", () => {
      console.log("[Event]-sourceset");
    });
    this.playerRef.addEventListener("splicewaiting", () => {
      console.log("[Event]-splicewaiting");
    });
    this.playerRef.addEventListener("start", () => {
      console.log("[Event]-start");
    });
    this.playerRef.addEventListener("thirdquartile", () => {
      console.log("[Event]-thirdquartile");
    });
    this.playerRef.addEventListener("timeupdate", () => {
      console.log("[Event]-timeupdate");
    });
    this.playerRef.addEventListener("unmute", () => {
      console.log("[Event]-unmute");
    });
    this.playerRef.addEventListener("volumechange", () => {
      console.log("[Event]-volumechange");
    });
    this.playerRef.addEventListener("waiting", () => {
      console.log("[Event]-waiting");
    });
  }
}
