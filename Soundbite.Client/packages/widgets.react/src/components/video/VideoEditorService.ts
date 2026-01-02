import { createFFmpeg, fetchFile } from "@ffmpeg/ffmpeg";
import { VideoClip } from "./VideoClip";

class VideoEditorServiceClass {
  private ffmpeg = createFFmpeg({
    log: true,
    corePath: `${window.location.origin}/ffmpeg-core.js`,
  });
  private loadPromise: Promise<void> | undefined = undefined;

  /**
   * Retrieves a promise that indicates when ffmpeg is loaded.  When first called, the method calls
   * ffmpeg.load which downloads the ffmpeg WASM libraries.  When ffmpeg is loaded and ready to
   * run video editing commands, the promise resolves.
   */
  public ensureLoaded(): Promise<void> {
    if (!this.loadPromise) {
      this.loadPromise = new Promise<void>((resolve, reject) => {
        this.ffmpeg
          .load()
          .then(() => resolve())
          .catch((ex: any) => reject(ex));
      });
    }
    return this.loadPromise;
  }

  /**
   * Trims the start of the video clip by the specified duration.
   * @videoClip - the video clip to update
   * @duration - the amount of time to trim from the front of the video
   */
  public async trimStart(
    videoClip: VideoClip,
    duration: number
  ): Promise<void> {
    // TODO: Convert this to use mp4 instead of web
    if (videoClip?.file) {
      // Save off original data (if not already set)
      videoClip.originalFile = videoClip.originalFile ?? videoClip.file;

      await this.ensureLoaded();
      // TODO: Convert this to use mp4 instead of web
      this.ffmpeg.FS(
        "writeFile",
        "temp_in.webm",
        await fetchFile(videoClip.file)
      );
      await this.ffmpeg.run(
        "-ss",
        `${duration}`,
        "-i",
        "temp_in.webm",
        "-c:a",
        "copy",
        "-c:v",
        "copy",
        "temp_out.webm"
      );
      // TODO: Convert this to use mp4 instead of web
      const output = this.ffmpeg.FS("readFile", "temp_out.webm");
      const blob = new Blob([output.buffer], { type: "video/webm" });
      videoClip.file = new File([blob], "clip.webm");
      videoClip.url = URL.createObjectURL(videoClip.file);
    }
  }
}

export const VideoEditorService = new VideoEditorServiceClass();
