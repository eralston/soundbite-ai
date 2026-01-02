import { action, makeObservable, observable } from "mobx";

import {
  AppPayload,
  GroupDetails,
  Logger,
  MemberRole,
  PersonRole,
  SeriesPreview,
  Session,
  SessionPreview,
  SoundbiteApiConfig,
  TokenInfo,
  UserRole,
  Utils,
} from "@soundbite/api";

import { DialogManager } from "../dialogs/DialogManager";
import { IMemberStore } from "../interfaces/IMemberStore";
import { IOrganizationStore } from "../interfaces/IOrganizationStore";
import { ISyncStore } from "../interfaces/ISyncStore";
import { IPersonStore } from "../interfaces/IPersonStore";
import { ISeriesStore } from "../interfaces/ISeriesStore";
import { ISessionStore } from "../interfaces/ISessionStore";
import { IGroupStore } from "../interfaces/IGroupStore";
import { IUserStore } from "../interfaces/IUserStore";
import { SessionStore } from "./SessionStore";
import { GroupStore } from "./GroupStore";
import { MemberStore } from "./MemberStore";
import { OrganizationStore } from "./OrganizationStore";
import { OrgSyncStore } from "./SyncStore";
import { PersonStore } from "./PersonStore";
import { SeriesStore } from "./SeriesStore";
import { UserStore } from "./UserStore";
import { AuthState } from "../enums";
import { GlobalTheme } from "../styles";

/**
 * Acts as the primary integration and extensibility point for the Widget framework.  Essentially
 * this exposes properties that reference our default MobX store implementations for various aspects
 * of the application.  Clients can opt to implement custom MobX store implementations and assign
 * them to the properties in this class to "override" our default functionality.
 *
 * This class also exposes a series of "Show" methods intended to allow for the intercept of showing
 * various screens in the application.  By assigning a new method implementation to these properties
 * clients can override default navigation and dialog display in the application.
 */
class WidgetStoreClass {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  // Properties
  isInitialized: boolean = false;
  TokenRefreshTimeout: NodeJS.Timeout | null = null;
  authState: AuthState = AuthState.Initializing; // Stores the application authentication state

  // Store references
  members: IMemberStore = MemberStore;
  organizations: IOrganizationStore = OrganizationStore;
  sync: ISyncStore = OrgSyncStore;
  people: IPersonStore = PersonStore;
  series: ISeriesStore = SeriesStore;
  sessions: ISessionStore = SessionStore;
  groups: IGroupStore = GroupStore;
  users: IUserStore = UserStore;

  showDeleteSession: (orgRoute: string, session: SessionPreview) => void =
    this.showDeleteSessionHandler;

  showDeleteSeries: (orgRoute: string, series: SeriesPreview) => void =
    this.showDeleteSeriesHandler;

  showNewSession: (
    orgRoute: string,
    groupRoute?: string,
    isSeries?: boolean,
    onClose?: () => void
  ) => void = this.showNewSessionHandler;

  showEditSession: (orgRoute: string, session: SessionPreview) => void =
    this.showEditSessionHandler;

  showEditSeries: (orgRoute: string, series: SeriesPreview) => void =
    this.showEditSeriesHandler;

  showPlayer: (orgRoute: string, sessionRoute: string) => void =
    this.showPlayerHandler;

  showRecorder: (orgRoute: string, sessionRoute: string) => void =
    this.showRecorderHandler;

  showReports: (orgRoute: string, session: SessionPreview) => void =
    this.showReportsHandler;

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      authState: observable,
      isInitialized: observable,
      initialize: action,
    });
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Gets true if the current user is the given user role or higher; otherwise, false
   * @param userRole
   */
  isUserRole(userRole: UserRole) {
    const currentRole = this.users.currentUser?.userRole ?? UserRole.Unknown;

    if (currentRole === UserRole.Unknown) {
      return false;
    }

    const isRoleOrBetter = currentRole >= userRole;
    return isRoleOrBetter;
  }

  /**
   * Get true if the given user is the given person role or higher for the current org; otherwise, false
   * @param personRole
   */
  isPersonRole(personRole: PersonRole) {
    const currentRole =
      this.organizations.currentOrg?.details.me?.personRole ??
      PersonRole.Unknown;

    if (currentRole === PersonRole.Unknown) {
      return false;
    }

    const isRoleOrBetter =
      this.isUserRole(UserRole.God) || currentRole >= personRole;
    return isRoleOrBetter;
  }

  /**
   * Get true if the given user is the given member role or higher for the current group; otherwise, false
   * @param memberRole
   */
  isMemberRole(memberRole: MemberRole, groupDetails?: GroupDetails) {
    // Not having the group yet implies we can't make the comparison whatsoever, so we must err on the side of saying they are NOT
    if (groupDetails == null) {
      return false;
    }

    const isGodOrAdmin =
      this.isUserRole(UserRole.God) || this.isPersonRole(PersonRole.Admin);
    const isRoleOrBetter =
      isGodOrAdmin || groupDetails.memberRole >= memberRole;
    return isRoleOrBetter;
  }

  /**
   * Responsible for populating the widget store with any available info from the app payload.
   * Method will setup token to be passed to all API calls if included in the app payload.
   * @param appPayload
   * @param refreshTimeout - number of minutes to wait before refreshing the API token
   */
  initialize(appPayload: AppPayload | null, refreshTimeout: number = 50): void {
    if (appPayload == null) {
      return;
    }
    if (appPayload.organization) {
      this.organizations.currentOrg = appPayload.organization;
    }

    if (appPayload.organizations) {
      this.organizations.myOrgs = appPayload.organizations;
    }

    if (appPayload.user) {
      this.users.currentUser = appPayload.user;
    }

    const orgRoute = appPayload?.organization.details.route;
    const orgTheme = appPayload.organization?.settings?.theme;
    if (orgTheme != null) {
      GlobalTheme.set(orgTheme, orgRoute, true);
    } else {
      GlobalTheme.clear(orgRoute);
    }

    if (appPayload.token) {
      const token = appPayload.token;
      SoundbiteApiConfig.getToken = () => Promise.resolve(token);

      if (this.TokenRefreshTimeout) {
        clearTimeout(this.TokenRefreshTimeout);
      }

      this.authState = !!this.organizations.currentOrg
        ? AuthState.Authorized
        : AuthState.OrganizationSelect;

      // Setup a timeout that refreshes the token periodically
      this.TokenRefreshTimeout = setTimeout(() => {
        if (this.organizations.currentOrg) {
          const orgRoute = this.organizations.currentOrg.details.route;
          const url =
            SoundbiteApiConfig.ApiPrefixUrl +
            `/getToken/${encodeURIComponent(orgRoute)}`;
          SoundbiteApiConfig.httpAdapter
            .get<TokenInfo>(url)
            .then((data: TokenInfo) => {
              var token = data.token;
              SoundbiteApiConfig.getToken = () => Promise.resolve(token);
              Logger.LogInfo("Retrieved new token.");
            })
            .catch((err) => {
              Logger.LogError(
                "Failed to retrieve new token.  Current token may expire.",
                err
              );
            });
        }
      }, refreshTimeout * 60 * 1000);
    } else {
      // Token is not available which indicates that authentication is required
      this.authState = AuthState.AuthRequired;
    }

    this.isInitialized = true;
    Logger.LogInfo("WidgetStore initialized");
  }

  reset(): void {
    // Reset all stores that keep local state
    // This should be a very similar list as refresh()
    this.organizations.reset();
    this.people.reset();
    this.sessions.reset();
    this.groups.reset();

    this.sync.reset();
  }

  async refresh(): Promise<void> {
    // TODO: Consider making something to sync the orgRoute across these stores
    // They could theoretically fall out of sync

    // Refresh all stores that keep local state
    // This should be the same list as Reset()
    this.organizations.refresh();
    this.people.refresh();
    this.sessions.refresh();
    this.groups.refresh();
  }

  //////////[ Dialog Handlers ]/////////////////////////////////////////////////////////////////////

  private showPlayerHandler(orgRoute: string, sessionRoute: string): void {
    DialogManager.ShowPlayerDialog(orgRoute, sessionRoute);
  }

  private showRecorderHandler(orgRoute: string, sessionRoute: string): void {
    DialogManager.ShowRecordDialog(orgRoute, sessionRoute);
  }

  private showReportsHandler(orgRoute: string, session: SessionPreview): void {
    DialogManager.ShowReportsDialog(orgRoute, session);
  }

  private showDeleteSeriesHandler(
    orgRoute: string,
    series: SeriesPreview
  ): void {
    DialogManager.ShowSeriesDeleteDialog(orgRoute, series);
  }

  private showDeleteSessionHandler(orgRoute: string, session: Session): void {
    DialogManager.ShowSessionDeleteDialog(orgRoute, session);
  }

  private showEditSessionHandler(orgRoute: string, session: Session): void {
    alert("showing new session, woohoo!");
  }

  private showEditSeriesHandler(orgRoute: string, series: SeriesPreview): void {
    DialogManager.ShowSeriesEditDialog(orgRoute, series);
  }

  private showNewSessionHandler(
    orgRoute: string,
    groupRoute?: string,
    isSeries: boolean = false,
    onClose?: () => void
  ): void {
    DialogManager.ShowSessionNewDialog(orgRoute, groupRoute, isSeries, onClose);
  }
}

export const WidgetStore = new WidgetStoreClass();
