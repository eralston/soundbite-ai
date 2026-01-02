import Axios, { AxiosRequestConfig } from "axios";
import { action, makeObservable, observable } from "mobx";
import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import {
  ProviderType,
  SpaConfig,
  AppPayload,
  AzureAuthService,
  User,
  Utils,
} from "@soundbite/api";
import { AuthState, WidgetStore } from "@soundbite/widgets-react";

import { ProviderFactory } from "../sdk/providers/ProviderFactory";
import { BrowserStorageService } from "../sdk/BrowserStorageService";
import { ClientOpService } from "../sdk/ClientOpService";
import NavService from "../sdk/NavService";
import { getAadAuthProvider } from "../sdk/providers/auth/AadAuthProvider";

/**
 * Responsible for managing application state related to Single Page App (SPA)
 */
class SpaStoreClass {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  private _currentOrg?: string; // Stores the org route of the current org
  protected static _appInsights?: ApplicationInsights; // Provides telemtry to the front-end

  token: string | null = null; // Stores the API token
  refreshToken: string | null = null; // Stores the refresh token
  authProviderType: ProviderType = ProviderType.Unknown; // Stores a reference to the auth provider used to login
  config: SpaConfig = {} as SpaConfig; // Stores the SPA configuration received from the API
  loginCallbacks: { (): void }[] = []; // Stores an array of callbacks to make after the soundbite API token is received

  //////////[ Observables ]/////////////////////////////////////////////////////////////////////////

  invalidOrgRoute: boolean = false; // Stores a value indicating that the requested org route is invalid (not found or access denied)

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      // Observables
      invalidOrgRoute: observable,
      // Actions
      loginWithThirdParty: action,
      logout: action,
      loginToOrg: action,
      processPayload: action,
    });
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Responsible for initializing the application.  Initialization consists of requesting the
   * initial application payload which contains configuration settings for the application as well
   * as additional data if the user has a valid HTTP-only refresh token.  This token will be sent
   * automatically by the browser (if available).  This method must then determine whether
   */
  async initialize(): Promise<void> {
    // Acquire the application payload a second time in order to retrieve organizations for user
    let url = process.env.REACT_APP_CONFIG_API_PREFIX + `/spa/load`;
    let options = {
      headers: {
        "Content-Type": "application/json",
      },
    } as AxiosRequestConfig;
    if (this.token) {
      options.headers.Authorization = `Bearer ${this.token}`;
    }

    // Determine if the refresh token is present in local storage
    const localRefreshToken = BrowserStorageService.refreshToken;
    const initialOrgRoute = this.getInitialOrgRoute();

    if (localRefreshToken) {
      options.headers.rt = localRefreshToken;
      if (initialOrgRoute) {
        url += `?orgRoute=${initialOrgRoute}`;
      }
    }

    // Determine if last login provider is present in local storage
    const localLoginProvider = BrowserStorageService.authProviderType;
    if (localLoginProvider) {
      this.authProviderType = localLoginProvider;
    }

    // Acquire the initial application payload to get configuration
    var payload = (await Axios.get<AppPayload>(url, options)).data;
    this.processPayload(payload, true);
  }

  /**
   * Determines the initial organization route.  This method will return the org route in the URL if
   * present.  If not, the method uses the last org route in local storage. Method will return null
   * when no org route can be determined.
   */
  private getInitialOrgRoute(): string | null {
    const result =
      Utils.getOrgRouteFromPath(window.location.pathname) ??
      BrowserStorageService.lastSelectedOrg;
    return result;
  }

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Responsible for initiating third-party auth login using the specified provider
   * @param providerType - specifies which authentication provider to use
   */
  async loginWithThirdParty(providerType: ProviderType) {
    this.authProviderType = providerType;
    BrowserStorageService.authProviderType = providerType;
    const provider = ProviderFactory.getAuthProvider();
    if (provider !== null) {
      WidgetStore.authState = AuthState.ThirdPartyAuthorizing;
      await provider.loginAsync(false);
    }
  }

  /**
   * Starts the flow to grant admin access for the current provider
   * */
  async grantAdminAsync() {
    // Log the user out using the appropriate auth provider (if set)
    // TODO: Make this AAD non-specific in the future when we support multiple login types
    const provider = getAadAuthProvider();
    if (provider) {
      await provider.grantAdminAsync();
    }
  }

  /**
   * Responsible for initiating login using the specified provider
   * @param providerType - provider to use for logging the user into the application
   */
  async logout() {
    // If the provider type is not set attempt to acquire it from local storage
    if (!this.authProviderType) {
      var loginProviderTypeLs = BrowserStorageService.authProviderType;
      if (loginProviderTypeLs) {
        this.authProviderType = loginProviderTypeLs;
      }
    }

    // Log the user out using the appropriate auth provider (if set)
    const provider = ProviderFactory.getAuthProvider();
    if (provider) {
      await provider.logoutAsync();
    }

    // Clear values from memory / local storage
    this.token = null;
    this.refreshToken = null;
    BrowserStorageService.authProviderType = null;
    BrowserStorageService.refreshToken = null;
    WidgetStore.authState = AuthState.AuthRequired;
  }

  /**
   * Request a Soundbite API token for the specified organization.
   * @param orgRoute - route of the organization with which the auth token should be associated.
   * @returns - flag indicating whether login was successful.
   */
  async loginToOrg(orgRoute: string): Promise<boolean> {
    var payload: AppPayload = await AzureAuthService.loginToOrgAzureAD(
      orgRoute,
      null as any as User
    );
    this.processPayload(payload, false);
    await WidgetStore.organizations.setCurrentOrganization(orgRoute);
    BrowserStorageService.lastSelectedOrg = orgRoute;
    return this.token ? true : false;
  }

  /**
   * Responsible for handling all tasks that need to occur when first receiving a Soundbite API
   * token. This includes setting values in this class, saving tokens to local storage, parsing the
   * refresh token to determine the associated organization, and setting authentication state.
   * @param token - incoming token
   * @param refreshToken - incoming refresh token
   */
  private storeTokenInfo(token: string, refreshToken: string) {
    BrowserStorageService.refreshToken = refreshToken;
    this.token = token;
    this.refreshToken = refreshToken;
    WidgetStore.authState = AuthState.Authorized;

    // Process all of the login callbacks that have been registered and then clear them out
    this.loginCallbacks.forEach((callback) => callback());
    this.loginCallbacks = [];
  }

  /**
   * Responsible processing application payload data and setting state accordingly.
   * @param payload - application payload received from the server.
   * @param isInit - flag indicating that this is the initialization call for the SPA
   */
  processPayload(payload: AppPayload, isInit: boolean): void {
    // Make sure the payload was received
    if (payload) {
      // Determine whether the configuration was received and apply it if so
      if (payload.config) {
        this.loadConfig(payload.config);
      }

      // Determine whether a token was issued
      if (payload.token) {
        // Presence of a token implies user and organizations should be present
        WidgetStore.users.currentUser = payload.user;
        WidgetStore.organizations.myOrgs = payload.organizations;

        // Determine whether the "current" organization is set
        if (payload.organization) {
          this.storeTokenInfo(payload.token, payload.refreshToken);
          WidgetStore.organizations.currentOrg = payload.organization;
          WidgetStore.authState = AuthState.Authorized;

          // User is authorized so determine where they should "go"
          if (NavService.hasReturnPage) {
            NavService.gotoReturnPage();
          } else {
            // Determine whether the current path implies the user should be redirected to the org feed
            const pathsToRedirect = [
              "/",
              "/public/sso/logino365",
              "/public/sso/loginokta",
            ];
            const currentPath = window.location.pathname?.toLowerCase();
            if (pathsToRedirect.find((i) => i === currentPath)) {
              // Send the user to the current organization's feed
              NavService.goto(
                `/organizations/${payload.organization.details.route}`
              );
            }
          }
        } else {
          // When no organization is present the user must select an org
          this.storeTokenInfo(payload.token, payload.refreshToken); // This orgRoute value must match value in Masticore.Constants.UserOnlyOrgRoute
          WidgetStore.authState = AuthState.OrganizationSelect;
        }
      } else {
        // Determine if payload is from spa initialization
        if (isInit) {
          // SPA init without a token indicates authentication is required.
          WidgetStore.authState = AuthState.AuthRequired;
        } else {
          // Not from SPA init implies an authorization failure.
          WidgetStore.authState = AuthState.AuthFailed;
        }
      }
    }
  }

  loadConfig(config: SpaConfig) {
    this.config = config;
    try {
      if (!config.telemetryKey) return;

      // Load the app insights client from the config
      const appInsights = new ApplicationInsights({
        config: {
          connectionString: config.telemetryKey,
        },
      });
      appInsights.loadAppInsights();
      appInsights.trackPageView();

      SpaStoreClass._appInsights = appInsights;
      console.debug("App Insights Loaded with key", config.telemetryKey);
    } catch (err: any) {
      console.error("Error loading app insights:", err);
    }
  }

  /**
   * Gets the organization that is currently being displayed in the UI.
   */
  get currentOrg(): string | null {
    return this._currentOrg ?? null;
  }

  /**
   * Stores off the authentication service to local storage.  We do this so we know what service was
   * originally used to login so we can log the user out using the same auth provider.
   * @param authProvider - authentication provider used to originally authenticate the user.
   */
  setAuthProvider(authProvider: ProviderType): void {
    if (authProvider) {
      BrowserStorageService.authProviderType = authProvider;
    }
  }

  //TODO: we probably want to pop up a dialog to let the user know what is about to happen or
  //      give them the option to re-authenticate.

  /**
   * Determines whether any client operations failed due to authentication issues and if so
   * initiates the third-party login process.
   */
  async checkClientOpAuthFailures(): Promise<void> {
    if (ClientOpService.requiresAuthToProcessFailedOps) {
      const authProvider = ProviderFactory.getAuthProvider();
      if (authProvider) {
        authProvider.loginAsync(true);
      }
    }
  }
}

export default new SpaStoreClass();
