import * as Msal from "@azure/msal-browser";

import IAuthStrategy, { IAuthObserver } from "./IAuthStrategy";
import PublicError from "./PublicError";

export interface IAdClientConfig {
  clientId: string;
  redirectUri: string;
  postLogoutRedirectUri: string;
  adScopes: string[];
  apiScopes: string[];
}

// TODO: Expose this mapping concept via IAuthStrategy
export enum TokenType {
  Tenant,
  Host,
  Identity,
}

/**
 * Implements an IAuthStrategy using MSAL js
 * https://github.com/AzureAD/microsoft-authentication-library-for-js
 * https://dev.azure.com/soundbiteai/Soundbite/_wiki/wikis/Soundbite.wiki/11/Authentication
 * TODO: There is a forthcoming version w/ PKCE support
 * */
export default class AdStrategy implements IAuthStrategy {
  public static isIE =
    window.navigator.userAgent.indexOf("MSIE") > -1 ||
    window.navigator.userAgent.indexOf("Trident/") > -1;

  public static _instance?: AdStrategy;

  public static init(config: IAdClientConfig, observer?: IAuthObserver) {
    if (this._instance === undefined && config !== undefined) {
      this._instance = new AdStrategy(config);
      this._instance.observer = observer;
    }

    return this.instance;
  }

  public static get instance(): AdStrategy {
    if (!this._instance) {
      throw new Error("Call init on AdStrategy to initialize instance");
    }
    return this._instance;
  }

  /**
   * Template for the configuration of the MSAL app instance
   * TODO: Make ClientID, redirectUri, and postLogoutRedirectUri able to vary by environment
   * */
  private msalConfig: Msal.Configuration = {
    auth: {
      clientId: "", // Set by SpaConfig
      authority: "https://login.microsoftonline.com/common/",
      redirectUri: "", // Set by SpaConfig
      postLogoutRedirectUri: "", // Set by SpaConfig
      navigateToLoginRequestUrl: true,
    },
    cache: {
      cacheLocation: "localStorage",
      storeAuthStateInCookie: AdStrategy.isIE,
    },
  };

  // Scopes when requesting login tokens
  public adScopes: string[] = [
    // Set by SpaConfig
  ];

  // Scopes when request API tokens
  public tenantScopes: string[] = [
    // Set by Spaconfig
  ];

  public msal: Msal.PublicClientApplication;
  public accountInfo?: Msal.AccountInfo;

  observer?: IAuthObserver;

  protected constructor(config: IAdClientConfig) {
    this.Configure(config);

    this.msal = new Msal.PublicClientApplication(this.msalConfig);
    this.msal
      .handleRedirectPromise()
      .then((result: Msal.AuthenticationResult | null) => {
        if (this.accountInfo === undefined) this.accountInfo = result?.account;

        if (this.observer) this.observer.onAuth(this);
      });

    this.accountInfo = this.msal.getAllAccounts()[0];
  }

  private Configure(config: IAdClientConfig) {
    if (this.msalConfig.auth !== undefined) {
      this.msalConfig.auth.clientId = config.clientId;
      this.msalConfig.auth.redirectUri = config.redirectUri;
      this.msalConfig.auth.postLogoutRedirectUri = config.postLogoutRedirectUri;
    }

    this.adScopes = config.adScopes;
    this.tenantScopes = config.apiScopes;
  }

  public scopes(type: TokenType): string[] {
    switch (type) {
      case TokenType.Tenant:
        return this.tenantScopes;
      case TokenType.Host:
        return this.tenantScopes;
      case TokenType.Identity:
        return this.adScopes;
    }
  }

  public get adAuthRequest(): Msal.AuthorizationUrlRequest {
    return { scopes: this.scopes(TokenType.Identity) };
  }

  public redirectToLogin(): Promise<void> {
    try {
      localStorage.clear(); // This seems to resolve certain auth looping issues
      return this.msal.loginRedirect(this.adAuthRequest);
    } catch (err) {
      throw new PublicError(
        err,
        "Authentication Error",
        "Error trying to redirect to Active Directory login page"
      );
    }
  }

  public popupLoginAsync(): Promise<Msal.AuthenticationResult> {
    try {
      return this.msal.loginPopup(this.adAuthRequest);
    } catch (err) {
      throw new PublicError(
        err,
        "Authentication Error",
        "Error trying to present Active Directory login popup"
      );
    }
  }

  public redirectToAdminConsent() {
    try {
      const tenantId = "common";
      const clientId = this.msalConfig.auth?.clientId;
      const state = "12345"; // TODO: Anti-forgery token
      const redirectUri = this.msalConfig.auth?.redirectUri;
      const scopes = this.adScopes
        .map((s) => encodeURIComponent(s))
        .join("%20");

      const adminConsentUri =
        `https://login.microsoftonline.com/${tenantId}/v2.0/adminconsent` +
        `?client_id=${clientId}` +
        `&state=${state}` +
        `&redirect_uri=${redirectUri}` +
        `&scope=${scopes}`;

      window.location.replace(adminConsentUri);
    } catch (err) {
      throw new PublicError(
        err,
        "Authentication Error",
        "Error trying to redirect to Active Directory admin consent page"
      );
    }
  }

  /**
   * Classifies errors from msal into issues that need addressed via interactive user login
   * @param err
   */
  public isFallbackToLoginError(err: Error) {
    return true;
  }

  // IAuthStrategy

  public async tokenSilentAsync(
    type: TokenType = TokenType.Tenant
  ): Promise<string> {
    if (this.accountInfo === undefined)
      throw new Msal.InteractionRequiredAuthError(
        "No account information, please login"
      );

    const request: Msal.SilentRequest = {
      account: this.accountInfo,
      scopes: this.scopes(type),
    };
    const result: Msal.AuthenticationResult = await this.msal.acquireTokenSilent(
      request
    );
    if (this.observer) this.observer.onAuth(this);
    const token = result.accessToken;
    return token;
  }

  public async tokenAsync(type: TokenType = TokenType.Tenant): Promise<string> {
    try {
      // Attempt to acquire API token silently from AD about the API
      const token = await this.tokenSilentAsync(type);
      return token;
    } catch (err) {
      if (this.isFallbackToLoginError(err)) {
        localStorage.clear(); // TODO: This seems to resolve certain login loops, but I wonder if we can be more targeted with wiping our localStorage
        await this.redirectToLogin();
        return "";
      } else {
        throw new PublicError(
          err,
          "Authentication Error",
          "Error trying to make contact with Active Directory"
        );
      }
    }
  }

  public isLoggingIn(): boolean {
    return false;
  }

  public async isLoggedInAsync(observer?: IAuthObserver): Promise<boolean> {
    if (observer) this.observer = observer;
    return this.accountInfo !== undefined;
  }

  public async loginAsync(): Promise<void> {
    return this.redirectToLogin();
  }

  public async logoutAsync(): Promise<void> {
    this.msal.logout();
  }
}
