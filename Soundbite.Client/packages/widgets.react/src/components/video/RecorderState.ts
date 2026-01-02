/**
 * Enumeration of the various video editor recorder states.
 */
export enum RecorderState {
  /**
   * Denotes that the recorder is not currently recording and is not paused.
   */
  NotRecording = 0,

  /**
   * Denotes that the recorder is actively recording.
   */
  Recording = 1,

  /**
   * Denotes that the recorder is paused and can resume recording.
   */
  Paused = 2,

  /**
   * Denotes that the recorder is an error state.
   */
  Error = 99,
}
