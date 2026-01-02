import { Logger } from "@soundbite/api";
import { runInAction, makeObservable, observable, computed } from "mobx";
import { SessionValue } from "../../modules/StorageValue";
import { RecorderState } from "./RecorderState";
import { VideoClip } from "./VideoClip";

/**
 * VideoContext is intended to help manage the display, playback, and editing of a video.
 */
export class VideoEditorContext {
  //////////[ Static ]/////////////////////////////////////////////////////////////////////////
  public static devices: MediaDeviceInfo[] = [];
  public static isDeviceListLoaded: boolean = false;

  /** Return true if we have multiple devices */
  public static get isMultiDevice(): boolean {
    return this.devices.length > 1;
  }

  /**
   * Initiates the process of updating the list of media devices. This method automatically updates
   * the observable mediaDevices property on this class, so changes can be processed by an observer
   * or the promise can be awaited.
   * @returns a promise with an updated list of media devices
   */
  public static loadDevices(): Promise<MediaDeviceInfo[]> {
    const promise = new Promise<MediaDeviceInfo[]>((resolve, reject) => {
      if (this.isDeviceListLoaded) {
        resolve(VideoEditorContext.devices);
        return;
      }

      navigator.mediaDevices
        .enumerateDevices()
        .then((mediaDevices: MediaDeviceInfo[]) => {
          const videoDevices = mediaDevices.filter(
            (i) => i.kind == "videoinput"
          );
          VideoEditorContext.devices = videoDevices;
          this.isDeviceListLoaded = true;
          resolve(videoDevices);
        })
        .catch((ex: any) => {
          Logger.LogError("Failed to enumerate media devices.", ex);
          reject(ex);
        });
    });
    return promise;
  }

  //////////[ Properaties ]/////////////////////////////////////////////////////////////////////////

  /**
   * Reference to the Video element in the DOM where video is rendered when played.
   */
  public VideoElement: HTMLVideoElement | null = null;
  public onClipAdded?: (clip: VideoClip) => void;

  public stream?: MediaStream;
  private recorder?: MediaRecorder;
  private chunks?: any[];
  private originalVolume: number = 0;
  private lastDeviceId: SessionValue<string> = new SessionValue<string>(
    "VideoEditorContext",
    "lastDeviceId"
  );

  private recordingStarted?: Date; // Stores the last date/time the recorder was started
  private recordingDurationInMs: number = 0; // Store the number of miliseconds of the last recording (acculative with pausing)

  //////////[ Observables ]/////////////////////////////////////////////////////////////////////////

  public recorderState: RecorderState = RecorderState.NotRecording;
  public isWebCamOn: boolean = false;
  public isEditing: boolean = false;
  public currentDevice?: MediaDeviceInfo | null = null;

  public videoClips: VideoClip[] = [];
  public hasUnsavedRecording: boolean = false;

  public get isRecording(): boolean {
    return this.recorderState === RecorderState.Recording;
  }
  public get isPaused(): boolean {
    return this.recorderState === RecorderState.Paused;
  }

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      recorderState: observable,
      isEditing: observable,
      isWebCamOn: observable,
      currentDevice: observable,
      videoClips: observable,
      hasUnsavedRecording: observable,
      isRecording: computed,
      isPaused: computed,
    });
    VideoEditorContext.loadDevices().then((devices) => {
      runInAction(() => {
        if (devices.length === 0) {
          this.currentDevice = null;
          this.lastDeviceId.remove();
          return;
        }

        if (this.lastDeviceId.hasValue()) {
          const lastDevice = devices.find(
            (d) => d.deviceId === this.lastDeviceId.get()
          );
          if (lastDevice != null) {
            this.currentDevice = lastDevice;
            return;
          }
        }

        this.currentDevice = devices[0];
        this.lastDeviceId.set(this.currentDevice?.deviceId);
      });
    });
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Determines whether video capture is supported in the browser.
   */
  public static isVideoCaptureSupported(): boolean {
    return !!navigator?.mediaDevices?.getUserMedia;
  }

  public showEditor() {
    this.stopWebCam();
    runInAction(() => {
      this.isEditing = true;
    });
  }

  public showRecorder(startWebCam?: boolean) {
    if (startWebCam !== false) {
      this.startWebCam();
    }
    runInAction(() => {
      this.isEditing = false;
    });
  }

  /**
   * Starts streaming the web camera to the video element. Does NOT start recording automatically.
   */
  public startWebCam(): Promise<void> {
    const promise = new Promise<void>((resolve, reject) => {
      if (this.isWebCamOn) {
        resolve();
        return;
      }
      // Mute volume to avoid audio feedback
      this.muteVolume();

      if (VideoEditorContext.isVideoCaptureSupported()) {
        navigator.mediaDevices
          .getUserMedia(this.getConstraints())
          .then((stream: MediaStream) => {
            this.stream = stream;
            if (!!this.VideoElement) {
              this.VideoElement.srcObject = stream;
              runInAction(() => {
                this.isWebCamOn = true;
              });
              return resolve();
            } else {
              Logger.LogError(
                "Reference to the video DOM element is not present."
              );
              return reject();
            }
          })
          .catch((ex: any) => {
            Logger.LogError(
              "Unexpected error while opening user media for video capture.",
              ex
            );
          });
      } else {
        Logger.LogError("Video Editing is not supported in this browser.");
      }
    });
    return promise;
  }

  public switchWebCam(): void {
    // If we're not ready or don't have another choice, then we can't switch right now
    if (this.currentDevice == null) {
      return;
    }

    if (
      this.currentDevice == null ||
      !VideoEditorContext.isDeviceListLoaded ||
      VideoEditorContext.devices.length <= 1
    ) {
      return;
    }

    // Running means we stop
    const wasRunning = this.isWebCamOn;
    if (wasRunning) {
      this.stopWebCam();
    }

    // set current device to the next in the list
    let currentDeviceIndex = VideoEditorContext.devices.findIndex(
      (device) => this.currentDevice?.deviceId === device.deviceId
    );
    ++currentDeviceIndex;
    if (currentDeviceIndex >= VideoEditorContext.devices.length) {
      currentDeviceIndex = 0;
    }
    this.currentDevice = VideoEditorContext.devices[currentDeviceIndex];
    this.lastDeviceId.set(this.currentDevice.deviceId);

    // Resume if we were already running
    if (wasRunning) {
      this.startWebCam();
    }
  }

  /**
   * Stops streaming the web camera to the video element.  If a recording is in process, this
   * automatically pauses the recording process.
   */
  public stopWebCam(): void {
    // Automatically stop recording if the web camera is being turned off.
    if (
      this.recorderState == RecorderState.Recording ||
      this.recorderState == RecorderState.Paused
    ) {
      this.stopRecording();
    }

    const tracks = this.stream?.getTracks() ?? [];
    tracks.forEach((t) => t.stop());

    // Make sure we have a valid video element to stop
    if (!!this.VideoElement) {
      // Terminate the web camera feed
      this.VideoElement.srcObject = null;

      // Restore the volume to original state
      this.restoreOriginalVolume();

      // Update state
      runInAction(() => {
        this.isWebCamOn = false;
      });
    } else {
      Logger.LogError("Reference to the video DOM element is not present.");
    }
  }

  /**
   * Begins recording web camera data stream.
   */
  public startRecording(): void {
    if (this.recorderState === RecorderState.Recording) {
      return;
    }

    this.chunks = [];
    this.initRecorder();
    this.recorder?.start();
    this.muteVolume();

    this.recordingStarted = new Date();
    this.recordingDurationInMs = 0;

    // Update state
    runInAction(() => {
      this.recorderState = RecorderState.Recording;
      this.hasUnsavedRecording = true;
    });
  }

  /**
   * Pauses recording web camera data stream.
   */
  public pauseRecording(): void {
    if (this.recorderState == RecorderState.Recording) {
      this.recorder?.pause();
      this.storeRecordingDuration();

      runInAction(() => {
        this.recorderState = RecorderState.Paused;
      });
    }
  }

  /**
   * Resumes recording web camera data stream.
   */
  public resumeRecording(): void {
    if (this.recorderState == RecorderState.Paused) {
      this.recordingStarted = new Date();
      this.recorder?.resume();
      runInAction(() => {
        this.recorderState = RecorderState.Recording;
      });
    }
  }

  /**
   * Stops recording and saves the current video data.
   * @returns - URL that can be used to retrieve saved video stream.
   */
  public stopRecording(): void {
    if (this.recorderState === RecorderState.NotRecording) {
      return;
    }

    this.storeRecordingDuration();

    if (this.recorder) {
      const tracks = this.recorder.stream.getTracks();
      tracks.forEach((t) => t.stop());
      this.recorder.stop();
      runInAction(() => {
        // Wait for the recorder to finish processing the video before saving it
        // iOS Safari will fire dataavailable on the video element multiple times
        setTimeout(() => {
          this.recorderState = RecorderState.NotRecording;
          this.finalizeRecording();
        }, 1000);
      });
    } else {
      throw new Error(
        "Cannot stop recording because there is no recorder reference."
      );
    }

    this.restoreOriginalVolume();
  }

  public addClip(clip: VideoClip): void {
    runInAction(() => {
      this.videoClips = [...this.videoClips, clip];
    });
    if (this.onClipAdded) {
      this.onClipAdded(clip);
    }
  }

  public removeClipById(id: number): void {
    runInAction(() => {
      this.videoClips = this.videoClips.filter((i) => i.id !== id);
    });
  }

  public removeClipAt(index: number): void {
    runInAction(() => {
      this.videoClips = this.videoClips.splice(index, 1);
    });
  }

  public playClip(clip: VideoClip) {
    if (!this.VideoElement) {
      throw new Error("Cannot play clip because the video element is missing");
    }
    this.restoreOriginalVolume();
    this.VideoElement.srcObject = null;
    this.VideoElement.src = clip.url ?? "";
  }

  public finalizeRecording() {
    // Store off the video data to a blob accessible by URL
    const codec = this.getCodec(this.chunks);
    const ext = this.getExtensionFromCodec(codec);
    const blob = new Blob(this.chunks, { type: codec });
    const file = new File([blob], `clip.${ext}`);
    const clipId = this.getNextClipId();
    const clip = new VideoClip(
      clipId,
      `Clip ${clipId}`,
      URL.createObjectURL(blob),
      true,
      this.recordingDurationInMs
    );

    clip.file = file;
    clip.fileFormat = "mp4";
    this.addClip(clip);
    this.chunks = [];
    this.recordingDurationInMs = 0;

    runInAction(() => {
      this.hasUnsavedRecording = false;
    });
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  private onVideoDataCaptured(e: BlobEvent): void {
    // Whenever there is data push it to the chunked data array
    this.chunks?.push(e.data);
  }

  //////////[ Methods - Utility ]///////////////////////////////////////////////////////////////////

  /**
   * Attempts to retrieves the codec associated with the file.  In theory there can be multiple
   * chunks received but in testing there seems to only be 1 chunk and there is a codec associated
   * with that chunk via the 'type' property. This method iterates all of the chunks and grabs the
   * codec from that 'type' property giving priority to any 'type' value that has the video
   * mimetype signature
   */
  private getCodec(chunks: any[] | undefined): string {
    let codec = "";
    let hasVideoMimeSignature = false;
    if (chunks && chunks.length > 0) {
      for (let i = 0; i < chunks.length; i++) {
        if (!hasVideoMimeSignature) {
          if (!codec && chunks[i].type) {
            // Set the codec because we take what we can get
            codec = chunks[i].type;
            // Lock in the codec if it contains a video mimetype signature
            hasVideoMimeSignature = codec.indexOf("video/") >= 0;
          }
        }
      }
    }

    // Default to the iphone MP4 if we do not find anything
    if (!codec) {
      codec = 'video/mp4; codecs="avc1.42E01E, mp4a.40.2"';
    }

    return codec;
  }

  /**
   * Attempts to acquire the file extension for the clip based on the codec
   * @param codec - the codec from which to derive an extension
   * @returns the appropriate extension (without a leading .)
   */
  private getExtensionFromCodec(codec: string) {
    let codecLower = codec?.toLowerCase() ?? "";
    if (codecLower.indexOf("webm")) {
      return "webm";
    } else {
      return "mp4";
    }
  }

  /**
   * Restore the volume to the original volume when value was muted.
   */
  private restoreOriginalVolume() {
    if (this.VideoElement) {
      this.VideoElement.volume = this.originalVolume;
    }
  }

  /**
   * Mutes the audio in the video element to avoid creating audio feedback while the recorder
   * is running.  Retains original volume value so it can be reset after recording is over.
   */
  private muteVolume(): void {
    //NOTE: I believe when the video element is created the volume is automatically set to 100% so
    //  the original volume is always 100%. For now this works, but it technically does not try
    //  to account for changes in volume during recording because at the time of writing this there
    //  is now way to do that.

    if (this.VideoElement) {
      if (this.originalVolume == 0) {
        this.originalVolume = this.VideoElement.volume;
      }
      this.VideoElement.volume = 0;
    }
  }

  /**
   * Responsible for initializing the video recorder.
   */
  private initRecorder(): void {
    if (this.stream) {
      // Create a new recorder using the current media stream
      this.recorder = new MediaRecorder(this.stream, {
        mimeType: "video/webm; codecs=h264"
      });

      if (this.VideoElement != null) {
        this.VideoElement.srcObject = this.stream;
      } else {
        alert("Cannot set stream on video element");
        Logger.LogError(
          "Cannot assign stream to source object in initRecorder, video element will be blank"
        );
      }

      // Handle incomming video data
      this.recorder.addEventListener("dataavailable", (e) =>
        this.onVideoDataCaptured(e)
      );
    } else {
      throw new Error(
        "Cannot start recording because media stream is not initialized."
      );
    }
  }

  /**
   * Creates a unique ID for the video clip based on the current list of clips.
   */
  private getNextClipId(): number {
    let result: number = 0;

    if (this.videoClips.length === 0) {
      // There are no clips so start out with an ID of 1
      result = 1;
    } else {
      // There are clips so get the maximum value and add 1
      this.videoClips.forEach((i) => {
        if (i.id && result < i.id) {
          result = i.id;
        }
      });
      result++;
    }

    return result;
  }

  /**
   * Builds out the constraints used to request the video media stream.
   */
  private getConstraints(): MediaStreamConstraints {
    const constraints = {
      audio: true,
      video: {
        width: { ideal: 1280 },
        height: { ideal: 720 },
        deviceId: !!this.currentDevice?.deviceId
          ? this.currentDevice.deviceId
          : undefined,
      } as MediaTrackConstraints,
    } as MediaStreamConstraints;

    return constraints;
  }

  /**
   * Should be called when recording is paused or stops.  Responsible for using the record start
   * time to calculate the duration of the recording.
   */
  private storeRecordingDuration() {
    if (this.recordingStarted) {
      this.recordingDurationInMs =
        this.recordingDurationInMs +
        new Date().getTime() -
        this.recordingStarted.getTime();
      this.recordingStarted = undefined;
    } else {
      this.recordingDurationInMs = -999999999;
      throw new Error("Time tracking value not set");
    }
  }
}
