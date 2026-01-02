/** The origin of the clip */
export enum ClipSource {
  /** Recorded live audio on the device */
  RecordAudio,

  /** Recorded live video on device */
  RecordVideo,

  /** Uploaded audio - potentially a fully produced recording */
  Upload,

  /** Uploaded audio - potentially a fully produced recording */
  UploadVideo,
}

export enum DialogState {
  Empty,
  Waiting,
  Ready,
}

export enum AuthState {
  // Application is awaiting initialization payload
  Initializing = 0,

  // [Failed] - Initialization call failed so the application cannot initialize.
  InitializationFailed = 1,

  // User is not logged in and needs to do so
  AuthRequired = 2,

  // Process for acquiring a third-paty auth token is taking place
  ThirdPartyAuthorizing = 3,

  // Third party token has been received and application is waiting for organization info for user.
  ThirdPartyAuthorized = 4,

  // [Failed] - Third-party authentication token retrieval failed
  ThirdPartyAuthFailed = 5,

  // Displayed when user is in multiple organizations and needs to choose one to login to.
  OrganizationSelect = 6,

  // Process for acquiring a Soundbite API token is taking place
  Authorizing = 7,

  // [Failed] - Soundbite API token retrieval failed
  AuthFailed = 8,

  // Soundbite API token has been received and is available for use
  Authorized = 9,

  // Occurs when the user is switching to another organization to which they have access but no token
  OrganizationSwitch = 10,
}
