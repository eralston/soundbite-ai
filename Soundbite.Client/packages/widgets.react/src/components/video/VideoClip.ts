/**
 * Video clips reprent a piece of video media.  Depending on how far we want to take the video
 * editor, the editor could be responsible for multiple clips to help arrange and piece together
 * various pieces of content.
 */
export class VideoClip {
  id: number;
  name: string;
  url?: string;
  file?: File;
  originalFile?: File;
  fileFormat?: string;
  isLocalRecording: boolean = false;
  durationInMs: number = 0;

  constructor(
    id: number,
    name: string,
    url: string,
    isLocalRecording: boolean,
    durationInMs: number
  ) {
    this.id = id;
    this.name = name;
    this.url = url;
    this.isLocalRecording = isLocalRecording;
    this.durationInMs = durationInMs;
  }
}
