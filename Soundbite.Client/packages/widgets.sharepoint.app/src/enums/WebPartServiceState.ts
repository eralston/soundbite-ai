/**
 * Enumeration of the various states of the WebPartService
 */
export enum WebPartServiceState {
  /** Denotes the service has not yet started to initialize. */
  none = 0,

  /** Denotes the service has started the initialization process. */
  init = 1,

  /** Denotes that initialization failed because configuration settings are not specified. */
  badConfig = 2,

  /** Denotes that initialization failed because the user token could not be retrieved. */
  tokenFailed = 3,

  /** Denotes that initialization failed calling the Soundbite API. */
  apiFailed = 4,

  /** Denotes that initialization was successful. */
  ok = 5,
}
