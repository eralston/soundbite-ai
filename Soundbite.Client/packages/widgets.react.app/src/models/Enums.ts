/**
 * Enumeration of the various asyncronous load states
 */
export enum LoadState {
  /**
   * Load has not yet been requested.
   */
  NotLoaded = 0,

  /**
   * Load has been initiated but is not yet complete.
   */
  Loading = 1,

  /**
   *  Loading has been requested but the operation failed.
   */
  Failed = 2,

  /**
   * Load has been requested and succeeded.
   */
  Loaded = 3,
}
