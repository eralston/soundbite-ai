import { Clip, ClipHostingType, Utils } from "@soundbite/api";
import { AzureMediaPlayerContext } from "./AzureMediaPlayerContext";
import { HlsMediaPlayerContext } from "./HlsMediaPlayerContext";
import { IMediaPlayerContext } from "./IMediaPlayerContext";
import { MediaPlayerContext } from "./MediaPlayerContext";
import { SbMediaPlayerContext } from "./SbMediaPlayerContext"

/**
 * MediaPlayerContext represents media playback operations and player state information.
 */
class MediaPlayerContextFactoryClass {
  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  getContext(clip?: Clip, mediaUrl?: string): IMediaPlayerContext {
    const typeName = this.getTypeName(clip, mediaUrl);
    switch (typeName) {
      case "audio":
        return new MediaPlayerContext("audio");
      case "video":
        return new MediaPlayerContext("video");
      case "azurestreaming":
        return new AzureMediaPlayerContext();
      case "hlsstreaming":
        return new HlsMediaPlayerContext();
      case "sbstreaming":
        return new SbMediaPlayerContext();
      default:
        throw new Error(
          `Failed to create a MediaPlayerContext instance for the specifid clip/url. (typename=${typeName})`
        );
    }
  }

  /**
   * Determines the appropriate media context for the clip
   * @clip - reference to the clip for which the context is being constructed.
   * @mediaUrl - reference to the media URL for which the context is being constructed. Note that
   *   the mediaUrl is primarily used for displaying local file blobs recorded from the browser.
   */
  getTypeName(clip?: Clip, mediaUrl?: string): string {
    let result: string;

    // Make sure we have a clip with which to work
    if (clip) {
      result = this.getTypeNameByClip(clip);
    } else if (mediaUrl) {
      result = this.getTypeNameByMediaUrl(mediaUrl);
    } else {
      throw new Error(
        "Cannot get MediaPlayerContext type name without a clip or media URL."
      );
    }

    return result;
  }

  private getTypeNameByClip(clip: Clip): string {
    let result: string = "UNKNOWN";

    if (Utils.IsVideo(clip.fileType)) {
      switch (clip.hostingType) {
        case ClipHostingType.AzureStreaming:
          result = "hlsstreaming";
          break;
        case ClipHostingType.AzureStorage:
          result = "video";
          break;
        case ClipHostingType.SbStreaming:
          result = "sbstreaming";
          break;
        default:
          throw new Error(
            `Cannot determine MediaPlayerContext type name because the hosting type is not implemented (type=${clip.hostingType}).`
          );
      }
    } else {
      result = "audio";
    }
    return result;
  }

  private getTypeNameByMediaUrl(mediaUrl: string): string {
    if (!Utils.isNullOrEmpty(mediaUrl)) {
      const parts: string[] = mediaUrl.split(".");
      const ext: string = parts[parts.length - 1].toLowerCase();
      switch (ext) {
        case "mp3":
        case "mpg":
          return "audio";
        case "webm":
        case "mp4":
          return "video";
        default:
          throw new Error(
            "Cannot determine MediaPlayerContext type name because the media URL extension is unrecognized."
          );
      }
    } else {
      throw new Error(
        "Cannot determine MediaPlayerContext type name because the media URL is missing."
      );
    }
  }
}
export const MediaPlayerContextFactory = new MediaPlayerContextFactoryClass();
