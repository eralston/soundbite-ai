import axios from "axios";
import { action, observable, makeObservable, runInAction } from "mobx";

import {
  NewClip,
  NewSession,
  ParticipantRole,
  ParticipantState,
  ParticipantStateUpdate,
  SeriesPreview,
  SessionDetails,
  SessionsService,
  SessionPreview,
  ClipService,
  ClipState,
  SeriesService,
  SessionSecurityType,
  IMediaEffect,
} from "@soundbite/api";

import {
  CreateSessionResult,
  ISessionStore,
  UploadResult,
} from "../interfaces/ISessionStore";
import { ConcurrentCache } from "../modules/ConcurrentCache";

/**
 * MobX store class containing states and actions for sessions.
 */
class SessionStoreClass implements ISessionStore {
  /** The max size where the session store will directly upload files to the API */
  public static readonly maxDirectUploadSize = 25000000;

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      // Observables
      orgFeed: observable,
      orgSeries: observable,
      groupFeed: observable,
      groupSeries: observable,

      // Actions
      acknowledgeSessionAsync: action,
      createSessionAsync: action,
      deleteSessionAsync: action,
      readOrgFeedAsync: action,
      readOrgSeriesAsync: action,
      readGroupFeedAsync: action,
      readGroupSeriesAsync: action,
      updateParticipantStateAsync: action,
      updateSessionAsync: action,
    });
  }

  //////////[ Private Methods ]//////////////////////////////////////////////////////////

  /**
   * If the clip is not yet ready, then it checks if the API says it's ready now
   * It will not only return the current state, but also accumulate it into the given UploadResult to enable seamless retry
   * WARNING: Calling this may have a side-effect of just-in-time processing the workflow for a clip, but that behavior is still async and "eventually consistent", this it is not 1:1 the same as forcing a processing step on the clip/prompt/session
   * @param result
   */
  public async clipState(result: UploadResult): Promise<ClipState> {
    // This may have already been determined as ready by successful form upload or a retry
    if (result.isReady || result.clipDraft == null) {
      return ClipState.Unknown;
    }

    // Check the file arrived
    const state = await ClipService.stateAsync(
      result.clipDraft.orgRoute,
      result.clipDraft.sessionRoute,
      result.clipDraft.promptRoute,
      result.clipDraft.clipRoute
    );

    result.clipDraft.state = state;
    result.isReady = state === ClipState.Ready;
    return state;
  }

  /**
   * Unless you're implementing a custom upload flow, you can use this.uploadClipAsync to accomplish a default clip create and upload lifecycle
   * Uploads the clip to storage based on current progress and stream size; supports retry via checking the given UploadResult
   * result.clipDraft.uploadUrl must contain a value, including an appended SAS token with upload permissions
   * newClip.Stream must contain a value and its size must be larger than SessionStoreClass.maxDirectUploadSize
   * If the clip is less than SessionStoreClass.maxDirectUploadSize, you should upload it directly via ClipService.createAsync
   * @param result
   * @param newClip
   * @param onProgress
   */
  public async uploadToStorageIfNeeded(
    result: UploadResult,
    newClip: NewClip,
    onProgress?: (percent: number) => void
  ) {
    const isStorageUpload =
      !result.isUploaded &&
      newClip.stream != null &&
      newClip.stream.size > SessionStoreClass.maxDirectUploadSize;

    // This may already be upload or simply doesn't need it
    if (result.clipDraft?.uploadUrl == null || !isStorageUpload) {
      return;
    }

    // We only indirect upload if we have a stream and a place to put it
    // TODO: Consider if it would be better to use the Azure SDK
    await axios.put(result.clipDraft.uploadUrl, newClip.stream, {
      headers: {
        "x-ms-blob-type": "BlockBlob",
      },
      onUploadProgress: (progressEvent: any) => {
        if (onProgress != null) {
          const percentCompleted = Math.round(
            (progressEvent.loaded * 100) / progressEvent.total
          );
          let newPercent = percentCompleted > 95 ? 95 : percentCompleted;
          newPercent = newPercent < 10 ? 10 : newPercent;
          // We have one more state check step, so we max at 99 for the upload
          onProgress(newPercent);
        }
      },
    });

    result.isReady = false;
    result.isUploaded = true;
  }

  /**
   * Unless you're implementing a custom upload flow, you can use this.uploadClipAsync to accomplish a default clip create and upload lifecycle
   * Creates a new clip record via API if one does not already exist in the given UploadResult
   * If newClip.stream != null exists and newClip.stream.size <= SessionStoreClass.maxDirectUploadSize, this direct uploads the clip data
   * Otherwise, you should augment this call with uploadToStorageIfNeeded to perform upload direct to storage using this.uploadToStorageIfNeeded
   * @param result
   * @param newClip
   * @param onProgress
   * @param orgRoute
   * @param sessionRoute
   * @param promptRoute
   */
  public async createClipIfNeeded(
    result: UploadResult,
    newClip: NewClip,
    orgRoute: string,
    sessionRoute: string,
    promptRoute: string,
    mediaEffects?: IMediaEffect[],
    onProgress?: (percent: number) => void
  ) {
    if (result.clipDraft != null) {
      return;
    }

    var { formData, isFormUpload } = this.formDataForClip(
      newClip,
      mediaEffects
    );

    if (isFormUpload && onProgress != null) {
      onProgress(10);
    }

    // TODO: Impement progress callback when isApiUpload
    result.clipDraft = await ClipService.createAsync(
      orgRoute,
      sessionRoute,
      promptRoute,
      formData,
      {
        onProgress: (percent: number) => {
          if (!isFormUpload || onProgress == null) {
            return;
          }
          const newPercent = percent < 90 ? percent : 90;
          onProgress(newPercent);
        },
      }
    );

    result.isUploaded = isFormUpload;
    result.isReady = result.clipDraft.state === ClipState.Ready;
  }

  /**
   * Analyzes the NewClip object, returning the related form data and a flag indicating if it already contains the clip
   * @param newClip
   * @param onProgress
   */
  private formDataForClip(newClip: NewClip, mediaEffects?: IMediaEffect[]) {
    let formData = new FormData();
    formData.append("fileType", newClip.fileType.toString());
    formData.append("clipType", newClip.clipType.toString());
    formData.append("participantRole", newClip.participantRole.toString());
    formData.append("seconds", newClip.seconds?.toString() ?? "0");
    formData.append("mediaEffects", JSON.stringify(mediaEffects));

    const isFormUpload =
      newClip.stream != null &&
      newClip.stream.size <= SessionStoreClass.maxDirectUploadSize;

    // API upload is the most reliable for us
    if (newClip.stream != null && isFormUpload) {
      // We only upload directly if we have a stream and it is small
      formData.append("clip", newClip.stream, "clip");
    }

    return { formData, isFormUpload };
  }

  //////////[ Observable Data Properties ]//////////////////////////////////////////////////////////

  private lastOrgRoute?: string;
  private lastOrgSecurityType?: SessionSecurityType;
  private lastGroupRoute?: string;
  private lastGroupSecurityType?: SessionSecurityType;

  orgFeed?: SessionPreview[] = undefined;
  groupFeed?: SessionPreview[] = undefined;

  orgSeries?: SeriesPreview[] = undefined;
  groupSeries?: SeriesPreview[] = undefined;

  //////////[ Cache Members ]///////////////////////////////////////////////////////////////////////

  sessionDetailsCache: ConcurrentCache<SessionDetails> =
    new ConcurrentCache<SessionDetails>();

  /**
   * Responsible for clearing ALL cached data in the store.
   */
  resetCache(): void {
    this.sessionDetailsCache.clear();
  }

  /** Resets the store such that it's ready to target a new organization */
  reset(): void {
    this.resetCache();

    this.lastOrgRoute = undefined;
    this.lastGroupRoute = undefined;

    // Feed
    this.orgFeed = undefined;
    this.groupFeed = undefined;

    // Series
    this.orgSeries = undefined;
    this.groupSeries = undefined;
  }

  /** Based on current data, refreshes the value of each property in this object based on its last state  */
  async refresh(): Promise<void> {
    this.resetCache();

    const promises: Promise<any>[] = [];
    const orgRoute = this.lastOrgRoute;
    const orgSecurityType =
      this.lastOrgSecurityType || SessionSecurityType.Protected;
    const groupRoute = this.lastGroupRoute;
    const groupSecurityType =
      this.lastGroupSecurityType || SessionSecurityType.Protected;

    // Feed
    if (orgRoute != null) {
      promises.push(this.readOrgFeedAsync(orgRoute, orgSecurityType, false));

      if (groupRoute != null) {
        promises.push(
          this.readGroupFeedAsync(
            orgRoute,
            groupRoute,
            groupSecurityType,
            false
          )
        );
      }
    }

    // Series
    if (orgRoute != null) {
      promises.push(this.readOrgSeriesAsync(orgRoute, false));

      if (groupRoute != null) {
        promises.push(this.readGroupSeriesAsync(orgRoute, groupRoute, false));
      }
    }

    await Promise.all(promises);
  }

  /**
   * Creates a key used for caching operations
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to delete
   */
  private getCacheKey(orgRoute: string, sessionRoute: string): string {
    return `${orgRoute}-${sessionRoute}`;
  }

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Async read the session feed for the given org relevant to the current user
   * @param orgRoute
   * @param allowCache Default to TRUE
   */
  async readOrgFeedAsync(
    orgRoute: string,
    sessionSecurity: SessionSecurityType,
    allowCache: boolean = true
  ): Promise<SessionPreview[]> {
    if (
      this.lastOrgRoute === orgRoute &&
      this.lastOrgSecurityType == sessionSecurity &&
      this.orgFeed != null &&
      allowCache
    ) {
      return this.orgFeed;
    }

    // Reset
    this.lastOrgRoute = undefined;
    this.lastOrgSecurityType = undefined;
    this.orgFeed = undefined;

    // Load
    const ret =
      sessionSecurity === SessionSecurityType.Public
        ? await SessionsService.readPublicFeedAsync(orgRoute)
        : await SessionsService.readFeedAsync(orgRoute);
    this.lastOrgRoute = orgRoute;
    this.lastOrgSecurityType = sessionSecurity;
    this.orgFeed = ret;
    return ret;
  }

  /**
   * Async read the feed for the given group in the given org
   * @param orgRoute
   * @param groupRoute
   * @param allowCache
   */
  async readGroupFeedAsync(
    orgRoute: string,
    groupRoute: string,
    sessionSecurity: SessionSecurityType,
    allowCache: boolean = true
  ): Promise<SessionPreview[]> {
    if (
      this.lastOrgRoute === orgRoute &&
      this.lastGroupRoute === groupRoute &&
      this.lastGroupSecurityType === sessionSecurity &&
      this.groupFeed != null &&
      allowCache
    ) {
      return this.groupFeed;
    }

    // Reset
    this.lastGroupRoute = undefined;
    this.lastOrgRoute = undefined;
    this.lastGroupSecurityType = undefined;
    this.groupFeed = undefined;

    // Load
    const ret =
      sessionSecurity === SessionSecurityType.Public
        ? await SessionsService.readGroupPublicFeedAsync(orgRoute, groupRoute)
        : await SessionsService.readGroupFeedAsync(orgRoute, groupRoute);

    this.lastOrgRoute = orgRoute;
    this.lastGroupRoute = groupRoute;
    this.lastGroupSecurityType = sessionSecurity;
    this.groupFeed = ret;

    return ret;
  }

  /**
   * Async read the series for the given org relevant to the current user
   * @param orgRoute
   * @param allowCache Default to TRUE
   */
  async readOrgSeriesAsync(
    orgRoute: string,
    allowCache: boolean = true
  ): Promise<SeriesPreview[]> {
    if (
      this.lastOrgRoute === orgRoute &&
      this.orgSeries != null &&
      allowCache
    ) {
      return this.orgSeries;
    }

    // reset
    this.lastOrgRoute = undefined;
    this.orgSeries = undefined;

    // load
    const ret = await SeriesService.readAllAsync(orgRoute);
    this.lastOrgRoute = orgRoute;
    this.orgSeries = ret;
    return ret;
  }

  /**
   * Async read the series for the given group in the given org
   * @param orgRoute
   * @param groupRoute
   * @param allowCache
   */
  async readGroupSeriesAsync(
    orgRoute: string,
    groupRoute: string,
    allowCache: boolean = true
  ): Promise<SeriesPreview[]> {
    if (
      this.lastOrgRoute === orgRoute &&
      this.lastGroupRoute === groupRoute &&
      this.groupSeries != null &&
      allowCache
    ) {
      return this.groupSeries;
    }

    // Reset
    this.lastOrgRoute = undefined;
    this.lastGroupRoute = undefined;
    this.groupSeries = undefined;

    // Load
    const ret = await SeriesService.readAllGroupAsync(orgRoute, groupRoute);
    this.lastOrgRoute = orgRoute;
    this.lastGroupRoute = groupRoute;
    this.groupSeries = ret;

    return ret;
  }

  /**
   * Acknowledges the session as having been listent to by the current user.
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to acknowledge
   * @param session - (optional) reference to the session to update with updated participant state
   */
  async acknowledgeSessionAsync(
    orgRoute: string,
    sessionRoute: string,
    session?: SessionDetails,
    removeFromFeeds?: boolean
  ): Promise<void> {
    const state = {
      participantRole: ParticipantRole.Unknown,
      participantState: ParticipantState.Consumed,
    } as ParticipantStateUpdate;
    await this.updateParticipantStateAsync(
      orgRoute,
      sessionRoute,
      state,
      session
    );

    // Remove the acknowledged session from the myFeed list
    if (removeFromFeeds) {
      runInAction(() => {
        this.orgFeed = this.orgFeed?.filter((i) => i.route !== sessionRoute);
        this.groupFeed = this.groupFeed?.filter(
          (i) => i.route !== sessionRoute
        );
      });
    }
  }

  /**
   * Creates a new session.
   * @param orgRoute - route of the organization with which the session is associated.
   * @param newSession - session creation information.
   * @param newClip - initial recording to associate with the session.
   */
  async createSessionAsync(
    orgRoute: string,
    newSession: NewSession,
    clip: NewClip,
    mediaEffects?: IMediaEffect[],
    onProgress?: (percent: number) => void,
    retry?: CreateSessionResult
  ): Promise<CreateSessionResult> {
    const result: CreateSessionResult = retry ?? {};
    try {
      // Calling this when we already have a full session should do nothing
      if (result.session != null) {
        return result;
      }

      // Create draft if we don't already have one
      if (result.sessionDraft == null) {
        result.sessionDraft = await SessionsService.createAsync(
          orgRoute,
          newSession
        );
      }

      if (result.sessionDraft == null) {
        console.warn("Create session draft failed");
        return result;
      }

      // Attempt the upload and abort if it's not a complete success, capturing all draft objects)
      result.upload = await this.uploadClipAsync(
        orgRoute,
        result.sessionDraft.route,
        result.sessionDraft.prompts[0].route,
        clip,
        mediaEffects,
        onProgress,
        result?.upload
      );

      if (
        result.upload == null ||
        result.upload.clipDraft == null ||
        !result.upload?.isReady
      ) {
        console.warn("Clip upload incomplete");
        return result;
      }

      // We pull the session again so this time we pick up all the prompts and links
      result.session = await this.readSessionDetailsAsync(
        orgRoute,
        result.sessionDraft.route,
        false
      );

      if (result.session == null) {
        console.warn("Reading final session returned empty");
        return result;
      }

      this.sessionDetailsCache.set(
        this.getCacheKey(orgRoute, result.session.route),
        result.session
      );

      this.refresh();
    } catch (err: any) {
      result.error = err;
    }

    return result;
  }

  /**
   * Deletes the specified session
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to delete
   */
  async deleteSessionAsync(
    orgRoute: string,
    sessionRoute: string
  ): Promise<void> {
    await SessionsService.deleteAsync(orgRoute, sessionRoute);

    // remove from local
    this.orgFeed = this.orgFeed?.filter((i) => i.route !== sessionRoute);
    this.groupFeed = this.groupFeed?.filter((i) => i.route !== sessionRoute);
    this.sessionDetailsCache.delete(this.getCacheKey(orgRoute, sessionRoute));
  }

  /**
   * Updates the participant state
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to acknowledge
   * @param state - participant state update information
   * @param session - (optional) reference to the session to update with updated participant state
   */
  async updateParticipantStateAsync(
    orgRoute: string,
    sessionRoute: string,
    state: ParticipantStateUpdate,
    session?: SessionDetails
  ): Promise<void> {
    await SessionsService.updateStateAsync(orgRoute, sessionRoute, state);

    // Update the session on the client - this logic needs to sync with Server-Side ISessionService.UpdateStateAsync
    if (session) {
      const participants =
        state.participantRole > ParticipantState.Unknown
          ? session.participants
          : session.participants.filter(
              (p) => p.participantRole == state.participantRole
            );
      participants.forEach(
        (p) => (p.participantState = state.participantState)
      );
    }
  }

  /**
   * Stores updates to the specified session
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to update
   * @param session - updated session to persist
   */
  async updateSessionAsync(
    orgRoute: string,
    sessionRoute: string,
    session: NewSession
  ): Promise<SessionDetails> {
    const result = await SessionsService.updateAsync(
      orgRoute,
      sessionRoute,
      session
    );
    this.sessionDetailsCache.set(
      this.getCacheKey(orgRoute, result.route),
      result
    );
    return result;
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Retrieves the specified session
   * @param orgRoute - route of the organization with which the session is associated
   * @param sessionRoute - route of the session to retrieve
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  async readSessionDetailsAsync(
    orgRoute: string,
    sessionRoute: string,
    allowCache: boolean = true
  ): Promise<SessionDetails> {
    const key = this.getCacheKey(orgRoute, sessionRoute);
    const ret = this.sessionDetailsCache.getAsync(
      key,
      () => SessionsService.readAsync(orgRoute, sessionRoute),
      allowCache
    );
    return ret;
  }

  /**
   * Uploads a new clip and associates it with the specified session / prompt
   * @param orgRoute Route of the organization with which the session is associated
   * @param sessionRoute Route of the session with which the new clip is associated
   * @param promptRoute Route of the prompt with which the new clip is associated
   * @param newClip New clip to upload
   * @param mediaEffects - effects to apply to the media server-sides
   * @param onProgress Optional callback that will receive at least one 100% progress call
   */
  async uploadClipAsync(
    orgRoute: string,
    sessionRoute: string,
    promptRoute: string,
    newClip: NewClip,
    mediaEffects?: IMediaEffect[],
    onProgress?: (percent: number) => void,
    retry?: UploadResult
  ): Promise<UploadResult> {
    const result: UploadResult = retry ?? {
      isReady: false,
      isUploaded: false,
    };

    // If we're retrying a success, then skip
    if (result.clipDraft != null && result.isReady) {
      if (onProgress != null) {
        onProgress(100);
      }
      return result;
    }

    try {
      if (onProgress != null) {
        onProgress(1);
      }

      // If !isApiUpload and !isStorageUpload then this would be a draft
      // TODO: Implement draft

      // If we didn't successfully make a draft clip last time, then make it now
      // This is to make a new record to start the process
      await this.createClipIfNeeded(
        result,
        newClip,
        orgRoute,
        sessionRoute,
        promptRoute,
        mediaEffects,
        onProgress
      );

      // Direct to storage is for large clips and only if we have all the info we need
      await this.uploadToStorageIfNeeded(result, newClip, onProgress);

      // If we're down to just verifying the clip, then we're almost completely done
      if (onProgress != null) {
        onProgress(95);
      }

      // We may be retrying simply because it wasn't done processing
      await this.clipState(result);

      // We got to the end, so we've done everything we can
      if (onProgress != null) {
        onProgress(100);
      }
    } catch (err: any) {
      console.error("Error uploading clip");
      result.error = err;
    }

    return result;
  }
}

// Export a singleton instance of the store
export const SessionStore = new SessionStoreClass();
