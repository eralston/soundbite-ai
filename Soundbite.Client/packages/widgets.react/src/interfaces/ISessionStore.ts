import {
  ClipDetails,
  IMediaEffect,
  NewClip,
  NewSession,
  ParticipantStateUpdate,
  SeriesPreview,
  SessionDetails,
  SessionPreview,
  SessionSecurityType,
} from "@soundbite/api";

/** Used to return the context of an upload action */
export interface UploadResult {
  error?: Error;
  isUploaded: boolean;
  isReady: boolean;
  clipDraft?: ClipDetails;
}

/** Captures the potentially partial success of creating a session, optionally returning the various elements of the process*/
export interface CreateSessionResult {
  // The result is only a true success if sessionDetails != null && upload.clipDetails != null && wasSuccess === true

  /** Optional error that is set when an exception happened during the create process */
  error?: Error;

  /** Only set when the API created at least a draft session */
  sessionDraft?: SessionDetails;

  /** Set when the final session w/ full clips is available; if we have this, then the creation was successful */
  session?: SessionDetails;

  /** Only set when the upload was attempted, be sure to check wasSuccess to ensure this was completed */
  upload?: UploadResult;
}

/**
 * Interface defininig the contract for a session store implementation.
 */
export interface ISessionStore {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  /** Gets or sets the list of sessions across all groups that are available to the user. */
  orgFeed?: SessionPreview[];

  /** Gets or sets the list of sessions for the most recently loaded group */
  groupFeed?: SessionPreview[];

  /** Gets or sets the list of series across all groups that are available to the user. */
  orgSeries?: SeriesPreview[];

  /** Gets or sets the list of series for the most recently loaded group. */
  groupSeries?: SeriesPreview[];

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Acknowledges the session as having been listent to by the current user.
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to acknowledge
   * @param session - (optional) reference to the session to update with updated participant state
   */
  acknowledgeSessionAsync(
    orgRoute: string,
    sessionRoute: string,
    session?: SessionDetails,
    removeFromFeeds?: boolean
  ): Promise<void>;

  /**
   * Responsible for clearing ALL cached data in the store.
   */
  reset(): void;

  /**
   * Resets then refreshes all observables on this store
   */
  refresh(): Promise<void>;

  /**
   * Async read the sessions for the given org
   * @param orgRoute
   * @param allowCache Default to TRUE
   */
  readOrgFeedAsync(
    orgRoute: string,
    sessionSecurity: SessionSecurityType,
    allowCache?: boolean
  ): Promise<SessionPreview[]>;

  /**
   * Async read the sessions for the given group in the given org
   * @param orgRoute
   * @param groupRoute
   * @param allowCache Default to TRUE
   */
  readGroupFeedAsync(
    orgRoute: string,
    groupRoute: string,
    sessionSecurity: SessionSecurityType,
    allowCache?: boolean
  ): Promise<SessionPreview[]>;

  /**
   * Async read the series for the given org
   * @param orgRoute
   * @param allowCache Default to TRUE
   */
  readOrgSeriesAsync(
    orgRoute: string,
    allowCache?: boolean
  ): Promise<SeriesPreview[]>;

  /**
   * Async read the series for the given group in the given org
   * @param orgRoute
   * @param groupRoute
   * @param allowCache
   */
  readGroupSeriesAsync(
    orgRoute: string,
    groupRoute: string,
    allowCache?: boolean
  ): Promise<SeriesPreview[]>;

  /**
   * Creates a new session.
   * @param orgRoute - route of the organization with which the session is associated.
   * @param newSession - session creation information.
   * @param newClip - initial recording to associate with the session.
   */
  createSessionAsync(
    orgRoute: string,
    newSession: NewSession,
    clip: NewClip,
    mediaEffects?: IMediaEffect[],
    onProgress?: (percent: number) => void,
    retry?: CreateSessionResult
  ): Promise<CreateSessionResult>;

  /**
   * Deletes the specified session
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to delete
   */
  deleteSessionAsync(orgRoute: string, sessionRoute: string): Promise<void>;

  /**
   * Retrieves the specified session
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to retrieve
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  readSessionDetailsAsync(
    orgRoute: string,
    sessionRoute: string,
    allowCache?: boolean
  ): Promise<SessionDetails>;

  /**
   * Updates the participant state
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to acknowledge
   * @param state - participant state update information
   * @param session - (optional) reference to the session to update with updated participant state
   */
  updateParticipantStateAsync(
    orgRoute: string,
    sessionRoute: string,
    state: ParticipantStateUpdate,
    session?: SessionDetails
  ): Promise<void>;

  /**
   * Stores updates to the specified session
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to update
   * @param session - updated session to persist
   */
  updateSessionAsync(
    orgRoute: string,
    sessionRoute: string,
    session: NewSession
  ): Promise<SessionDetails>;

  /**
   * Uploads a new clip and associates it with the specified session / prompt
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session with which the new clip is associated
   * @param promptRoute - route of the prompt with which the new clip is associated
   * @param clip - new clip to upload
   * @param mediaEffects - effects to apply to the media server-sides
   */
  uploadClipAsync(
    orgRoute: string,
    sessionRoute: string,
    promptRoute: string,
    clip: NewClip,
    mediaEffects?: IMediaEffect[],
    onProgress?: (percent: number) => void,
    retry?: UploadResult
  ): Promise<UploadResult>;
}
