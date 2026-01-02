import MicRecorder from "mic-recorder-to-mp3";

export class Recorder {
  private recorder?: any;
  private file?: File;
  private isRunning: boolean = false;

  get isRecording() {
    return this.isRunning;
  }

  async getFile(fileName: string = "audio.mp3"): Promise<File | undefined> {
    if (this.file) return this.file;

    if (!this.recorder) return undefined;

    this.stop();

    const [buffer, blob] = await this.recorder.getMp3();

    this.file = new File(buffer, fileName, {
      type: blob.type,
      lastModified: Date.now(),
    });

    return this.file;
  }

  stop() {
    if (!this.isRunning) return;

    this.isRunning = false;

    if (this.recorder) this.recorder.stop();
  }

  async recordAsync(): Promise<void> {
    if (this.isRunning) return;

    if (!this.recorder)
      this.recorder = new MicRecorder({
        bitRate: 128,
      });

    this.isRunning = true;
    this.file = undefined;
    return this.recorder.start();
  }
}
