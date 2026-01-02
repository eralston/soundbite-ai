import {
  AppPayload,
  OrganizationWithPermissions,
  SoundbiteApiConfig,
  Logger,
} from "@soundbite/api";
import { AuthState, WidgetStore } from "@soundbite/widgets-react";
import { setupAxiosAdapter } from "@soundbite/api-axios";
import { action, makeObservable, observable, runInAction } from "mobx";
import * as microsoftTeams from "@microsoft/teams-js";

interface envSettings {
  apiUrl: string;
  teamsWebUrl: string;
}

/**
 * Environment specific settings are stored in javascript files in the /public/static/js folder.
 * The build process is responsible for copying environment specific values into the
 * /public/static/js/settings.js file which is referenced in the index.html page.  This loads
 * environment settings into window.SoundbiteTeamsAppSettings which is referenced below. This allows
 * a single build to be used across environments.
 **/
function GetSettings(): envSettings {
  return (window as any).SoundbiteTeamsAppSettings as envSettings;
}

/**
 * App Store for the Microsoft Teams App
 */
class TeamsAppStoreClass {
  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      // Observables
      msTeamsToken: observable,
      orgData: observable,
      // Actions
      initialize: action,
    });

    // Make sure to setup the axios HTTP client for the Soundbite API
    setupAxiosAdapter();
    SoundbiteApiConfig.ApiPrefixUrl = this.settings.apiUrl;
  }

  //////////[ Variables ]///////////////////////////////////////////////////////////////////////////

  // Helper to ensure initialize is not called multiple times simultaneously
  private allowInit: boolean = true;
  public settings: envSettings = GetSettings();

  //////////[ Observables ]/////////////////////////////////////////////////////////////////////////

  /** Stores the initialization state of the service */
  public initComplete: boolean = false;
  public orgData: OrganizationWithPermissions | null = null;
  public msTeamsToken: string | null = null;

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  public async initialize(): Promise<void> {
    Logger.LogInfo("TeamsAppStore initializing");

    if (this.allowInit) {
      this.allowInit = false;
      try {
        microsoftTeams.initialize(() => {
          Logger.LogInfo("Microsoft Teams initialized");
          runInAction(() => {
            WidgetStore.authState = AuthState.ThirdPartyAuthorizing;
          });
          microsoftTeams.authentication.getAuthToken({
            successCallback: (token: string) =>
              this.onTeamsAuthTokenSuccess(token),
            failureCallback: (error: any) => this.onTeamsAuthTokenFailed(error),
          });
        });
      } catch {
        // Notify teams that initialization failed
        microsoftTeams.appInitialization.notifyFailure({
          reason: microsoftTeams.appInitialization.FailedReason.Other,
          message: "An unexpected error occured initializing the Teams App.",
        });
      }
    }
  }

  //////////[ Event Handlers ]////////////////////////////////////////////////////////////////////

  private async onTeamsAuthTokenSuccess(token: string): Promise<void> {
    Logger.LogInfo("Microsoft Teams auth token retrieved");
    runInAction(() => (this.msTeamsToken = token));
    SoundbiteApiConfig.getToken = () => Promise.resolve(token);
    await this.getAppPayload();
  }

  private async getAppPayload(): Promise<void> {
    let appPayloadFailed = false;
    Logger.LogInfo("Getting AppPayload");
    runInAction(() => (WidgetStore.authState = AuthState.Authorizing));
    try {
      const payload = await SoundbiteApiConfig.httpAdapter.post<AppPayload>(
        `${this.settings.apiUrl}azureAuth/LoginTeams`,
        null
      );

      if (payload) {
        runInAction(() => {
          WidgetStore.initialize(payload);
          this.initComplete = true;
          microsoftTeams.appInitialization.notifySuccess();
        });
      } else {
        appPayloadFailed = true;
      }
    } catch (error: any) {
      appPayloadFailed = true;
      if (error?.response?.status === 404) {
        runInAction(() => (WidgetStore.authState = AuthState.AuthFailed));
      } else {
        runInAction(
          () => (WidgetStore.authState = AuthState.InitializationFailed)
        );
      }
      Logger.LogError("Failed to retrieve AppPayload", error);
    }

    if (appPayloadFailed) {
      microsoftTeams.appInitialization.notifyFailure({
        reason: microsoftTeams.appInitialization.FailedReason.Other,
        message: "Failed to retrieve organization data from Soundbite API.",
      });
    }
  }

  private onTeamsAuthTokenFailed(error: any): void {
    // Notify teams that initialization failed
    WidgetStore.authState = AuthState.ThirdPartyAuthFailed;
    microsoftTeams.appInitialization.notifyFailure({
      reason: microsoftTeams.appInitialization.FailedReason.AuthFailed,
      message: "Failed to retrieve authentication token from Microsoft Teams.",
    });
  }
}

export const TeamsAppStore = new TeamsAppStoreClass();
