import * as Msal from "@azure/msal-browser";
import Axios from "axios";
import {
  AppPayload,
  Logger,
  ProviderType,
  PublicError,
  SoundbiteApiConfig,
  User,
  UserRole,
  UsersService,
  Utils,
} from "@soundbite/api";
import { AuthState, WidgetStore } from "@soundbite/widgets-react";

import SpaStore from "../../../store/Spa.Store";
import { IAuthProvider } from "./IAuthProvider";
import { IGraphMe } from "../../GraphSdk";
import { BrowserStorageService } from "../../BrowserStorageService";

export class SbAuthError extends Error {}

/**
 * IAuthProvider implementation for Office 365 / Microsoft Graph
 */
export class AadAuthProvider implements IAuthProvider {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  private msalConfig: Msal.Configuration;
  private msal: Msal.PublicClientApplication;
  private accountInfo?: Msal.AccountInfo;
  private scopesGraph: string[];
  private scopesSoundbiteApp: string[];
  private tokenSoundbiteApp?: string;
  private graphUserToken?: string;
  private azureAdAppToken?: string;

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  /**
   * Instantiates a new AADAuthProvider instance
   */
  constructor() {
    this.msalConfig = this.getMsalConfig();
    this.scopesGraph = SpaStore.config.adClientConfig.adScopes;
    this.scopesSoundbiteApp = SpaStore.config.adClientConfig.apiScopes;
    this.msal = new Msal.PublicClientApplication(this.msalConfig);
  }

  //////////[ IAuthProvider Implementation ]////////////////////////////////////////////////////////

  /**
   * Gets or sets the provider type
   */
  get providerType() {
    return ProviderType.AAD;
  }

  public get msalAccount(): Msal.AccountInfo {
    return this.accountInfo ?? this.msal.getAllAccounts()[0];
  }

  /**
   * Responsible for running any logic required to log the user in to the third-party provider.
   * @param forceReAuth - flag indicating that the user should be forced to login to the provider again.
   */
  async loginAsync(forceReAuth: boolean): Promise<void> {
    if (forceReAuth) {
      await this.redirectToLogin();
    } else {
      try {
        if (!this.tokenSoundbiteApp) {
          await this.getSoundbiteToken(true);
        }
      } catch (err: any) {
        await this.redirectToLogin();
      }
    }
  }

  /**
   * Responsible for handling the response returned from an OAuth redirect.
   */
  async handleAuthRedirect(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.msal
        .handleRedirectPromise()
        .then(async (authResult: Msal.AuthenticationResult | null) => {
          // Determine whether an auth redirect is being processed
          if (authResult) {
            this.graphUserToken = authResult.accessToken;
            this.accountInfo = authResult.account;
          } else {
            Logger.LogWarning(
              "AadAuthProvider - Handle Redirect - auth result missing"
            );
          }

          await this.getSoundbiteToken(true);

          // Resolve the isInitializedPromise to indicate the provider has initialized
          resolve();
        })
        .catch((ex) => {
          // Reject the isInitializedPromise to indicate the provider failed to initialize
          Logger.LogError("AadAuthProvider - Auth Redirect Exception", ex);
          this.tokenSoundbiteApp = "";
          this.accountInfo = undefined;
          reject(ex);
        });
    });
  }

  /**
   * Responsible for logging the user out of the authentication provider.
   */
  async logoutAsync(): Promise<void> {
    SpaStore.setAuthProvider(ProviderType.Unknown);
    this.msal.logout();
  }

  /**
   *  Grants admin access for the current user & org
   */
  async grantAdminAsync(): Promise<void> {
    const appId = SpaStore.config.adPlatformConfig.appId;
    const tenantId = await getTenantId();
    const adminUrl = `https://login.microsoftonline.com/${tenantId}/adminconsent?client_id=${appId}`;
    window.location.href = adminUrl;
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  public getTenantId(): string | undefined {
    return this.accountInfo?.tenantId;
  }

  /**
   * Provides a way for AAD-based providers to access the graph token required for operations.
   */
  async wrapGraphCall<T>(
    action: (graphToken: string) => Promise<T>
  ): Promise<T> {
    if ((window as any).simulateFail === true) {
      throw new SbAuthError("Please login to AAD again");
    }

    try {
      await this.getGraphUserToken();
    } catch {
      throw new SbAuthError("Please login to AAD again");
    }

    if (this.graphUserToken) {
      const result = await action(this.graphUserToken);
      return result as T;
    } else {
      throw new SbAuthError(
        "Call to acuire graph token succeeded but graph token is not available."
      );
    }
  }

  /**
   * Responsible for building the MSAL configuration from the application settings.
   */
  private getMsalConfig(): Msal.Configuration {
    const isIE =
      window.navigator.userAgent.indexOf("MSIE") > -1 ||
      window.navigator.userAgent.indexOf("Trident/") > -1;

    const config = {
      auth: {
        clientId: SpaStore.config.adClientConfig.clientId,
        authority: "https://login.microsoftonline.com/common/",
        redirectUri: SpaStore.config.adClientConfig.redirectUri,
        postLogoutRedirectUri:
          SpaStore.config.adClientConfig.postLogoutRedirectUri,
        navigateToLoginRequestUrl: false,
      },
      cache: {
        cacheLocation: "localStorage",
        storeAuthStateInCookie: isIE,
      },
    };

    return config;
  }

  /**
   * Uses the graph provided access token to request a Soundbite token.
   * @param getUserMetaData - flag indicating whether to acquire user metadata from MS graph.
   */
  private async getSoundbiteToken(getUserMetaData: boolean): Promise<void> {
    try {
      let promises: Array<Promise<void | User>> = [];
      let userImage: any = null;
      let userInfo: User | null = null;
      let appPayload: AppPayload | null = null;

      // Acquire graph token (if needed)
      if (!this.graphUserToken) {
        await this.getGraphUserToken();
      }

      // Determine whether to use the graph token to acquire user metadata
      if (getUserMetaData) {
        promises.push(
          this.getGraphUserImage().then((result: any) => {
            userImage = result;
          })
        );
        promises.push(
          this.getGraphUserInfo().then((result?: User) => {
            userInfo = result || null;
          })
        );
      }

      // Acquire Azure AD App Token
      if (!this.azureAdAppToken) {
        promises.push(this.getGraphAppToken());
      }

      // Wait for all promises to resolve
      await Promise.all(promises);

      // Acquire application payload
      // TODO: technically this can be acquired right after getting the app token instead of after all promises finish
      appPayload = await this.getAppPayload(userInfo);

      if (appPayload.token) {
        WidgetStore.initialize(appPayload);

        // Update user image if available
        if (userImage) {
          UsersService.upsertMyImageAsync(userImage).then((data) => {
            WidgetStore.users.updateCurrentUserImageUrl((data as any).imageSrc);
          });
        }

        SpaStore.processPayload(appPayload, false);
      } else {
        // Getting to this point without a token indicates that authentication failed
        WidgetStore.authState = AuthState.AuthFailed;
      }
    } catch (ex) {
      Logger.LogError(
        "AadAuthProvider - failed to acquire Soundbite token.",
        ex
      );
      throw ex;
    }
  }

  /**
   * Retrieves an MS Graph user token that can be used to make graph calls on behalf of the user.
   */
  public async getGraphUserToken(): Promise<void> {
    this.accountInfo = this.accountInfo || this.msal.getAllAccounts()[0];
    if (this.accountInfo) {
      //TODO: figure out what to do when the response fails
      try {
        const request: Msal.SilentRequest = {
          account: this.accountInfo,
          scopes: this.scopesGraph,
        };
        const response: Msal.AuthenticationResult =
          await this.msal.acquireTokenSilent(request);
        this.graphUserToken = response.accessToken;
        this.accountInfo = response.account;
      } catch (ex) {
        Logger.LogError("AadAuthProvider - failed to acquire graph token.", ex);
        this.graphUserToken = undefined;
        throw ex;
      }
    } else {
      const ex = new Error(
        "AadAuthProvider - MSAL could not provide account information for user graph token attempt."
      );
      Logger.LogError(ex.message, ex);
      throw ex;
    }
  }

  /**
   * Retrieves an MS Graph App token for soundbite using the user graph token.
   */
  private async getGraphAppToken(): Promise<void> {
    //FARK
    this.accountInfo = this.accountInfo || this.msal.getAllAccounts()[0];
    if (this.accountInfo) {
      const request: Msal.SilentRequest = {
        account: this.accountInfo,
        scopes: this.scopesSoundbiteApp,
      };
      const response: Msal.AuthenticationResult =
        await this.msal.acquireTokenSilent(request);
      this.azureAdAppToken = response.accessToken;
    } else {
      const ex = new Error(
        "AadAuthProvider - MSAL could not provide account information for graph app token attempt."
      );
      Logger.LogError(ex.message, ex);
      throw ex;
    }
  }

  /**
   * Retrieves the user's image from MS graph.
   */
  private async getGraphUserImage(): Promise<any | null> {
    try {
      const response = await Axios.get(
        "https://graph.microsoft.com/v1.0/me/photo/$value",
        {
          responseType: "arraybuffer",
          headers: {
            "Content-Type": "application/json",
            Accept: "application/pdf",
            authorization: `Bearer ${this.graphUserToken}`,
          },
        }
      );
      return new Blob([response.data]);
    } catch (ex: any) {
      return null;
    }
  }

  /**
   * Retreives user information from MS Graph and sends it back to Soundbite.
   */
  private async getGraphUserInfo(): Promise<User | undefined> {
    try {
      // Call out to the graph API using the newly acquired token
      const me = (
        await Axios.get<IGraphMe>("https://graph.microsoft.com/v1.0/me/", {
          headers: {
            authorization: `Bearer ${this.graphUserToken}`,
          },
        })
      ).data;

      const phone: string = me.businessPhones[0];
      const userFields: User = {
        allowEmail: true,
        email: me.mail,
        givenName: me.givenName,
        familyName: me.surname,
        phone: phone,
        title: me.jobTitle,
        allowNews: true,
        allowMarketing: true,
        userRole: UserRole.Unknown,
        calendarSettings: "",
        providerType: 1,
        allowSms: true,
      } as User;

      return userFields;
    } catch {
      return undefined;
    }
  }

  /**
   * Retrieves the application payload using the MS Graph App token.
   * @param userInfoToSync - user information from MS Graph to sync with Soundbite.
   */
  private async getAppPayload(
    userInfoToSync: User | null
  ): Promise<AppPayload> {
    try {
      let url = process.env.REACT_APP_CONFIG_API_PREFIX + `/azureAuth/login`;
      const headers = {
        Authorization: `Bearer ${this.azureAdAppToken}`,
        "Content-Type": "application/json",
      };
      const orgRoute =
        Utils.getOrgRouteFromPath(BrowserStorageService.returnToPage) ??
        BrowserStorageService.lastSelectedOrg;
      if (orgRoute) {
        url += "?orgRoute=" + encodeURIComponent(orgRoute);
      }
      const appPayload = await SoundbiteApiConfig.httpAdapter.post<AppPayload>(
        url,
        userInfoToSync,
        { headers: headers }
      );
      return appPayload;
    } catch (ex) {
      Logger.LogError(
        "AadAuthProvider - failed to retrieve application payload.",
        ex
      );
      throw ex;
    }
  }

  /**
   * Redirects the user to the AAD login page so they can sign in. Ultimately, this exits the
   * application and the login process resumes when the user is redirected from the O365 login
   * process back to the Soundbite application with an authorization code that can be used to
   * acquire an MS graph or Soundbite authorization token.
   */
  private redirectToLogin(): Promise<void> {
    try {
      BrowserStorageService.clearNonAppLocalStorage(); // This seems to resolve certain auth looping issues (was calling localStorage.clear originally so if issues arise may need to rethink this)
      SpaStore.setAuthProvider(ProviderType.AAD); // Make sure to set this so the AAD auth provider loads after redirect
      return this.msal.loginRedirect({ scopes: this.scopesGraph });
    } catch (err: any) {
      const error = new PublicError(
        err,
        "Authentication Error",
        "Error trying to redirect to Active Directory login page"
      );
      Logger.LogError("AadAuthProvider - LoginRedirect failed.", error);
      throw error;
    }
  }
}

/**
 * Stores a reference to the AAD auth provider instance when it is instantiated.
 */
let instance: AadAuthProvider | null = null;

/** Get the tenant ID for the current logged in user; relies on the auth process already having been fired */
export async function getTenantId(): Promise<string | undefined> {
  const i = getAadAuthProvider() as AadAuthProvider;
  let acct = i.msalAccount;
  if (acct == null) {
    await i.handleAuthRedirect();
    acct = i.msalAccount;
  }
  if (acct == null) {
    await i.loginAsync(false);
  }
  return acct.tenantId;
}

/**
 * Retrieves the AAD auth provider.  This method is also responsible for creating an instance of
 * the provider if it does not already exist.
 */
export function getAadAuthProvider(): IAuthProvider {
  if (!instance) {
    instance = new AadAuthProvider();
  }
  return instance;
}
