import {
  AppPayload,
  Logger,
  OrgOktaSettings,
  ProviderType,
  SoundbiteApiConfig,
  Utils,
} from "@soundbite/api";
import { AuthState, WidgetStore } from "@soundbite/widgets-react";

import SpaStore from "../../../store/Spa.Store";
import { IAuthProvider } from "./IAuthProvider";
import { BrowserStorageService } from "../../BrowserStorageService";
import { OktaAuth, OktaAuthOptions } from "@okta/okta-auth-js";

export class SbAuthError extends Error {}

/**
 * IAuthProvider implementation for Office 365 / Microsoft Graph
 */
class OktaProviderClass implements IAuthProvider {
  //////////[ Fields ]/////////////////////////////////////////////////////////////////

  private _oktaAuth: OktaAuth | null = null;
  private _accessToken: string | null = null;
  private _idToken: string | null = null;

  //////////[ Properties ]/////////////////////////////////////////////////////////////

  // Gets a reference to the OktaAuth instance used for OKTA operations.  This value must be
  // initialized via the applySettings method before it can be used.
  private get oktaAuth(): OktaAuth {
    if (!this._oktaAuth) {
      throw new Error(
        "OKTA settings must be specified by calling the applySettings method."
      );
    }
    return this._oktaAuth;
  }

  //////////[ IAuthProvider Implementation ]////////////////////////////////////////////////////////

  /**
   * Gets or sets the provider type
   */
  get providerType() {
    return ProviderType.OKTA;
  }

  /**
   * Responsible for running any logic required to log the user in to the third-party provider.
   * @param forceReAuth - flag indicating that the user should be forced to login to the provider again.
   */
  async loginAsync(forceReAuth: boolean): Promise<void> {
    await this.oktaAuth.signInWithRedirect();
  }

  /**
   * Responsible for handling the response returned from an OAuth redirect.
   */
  async handleAuthRedirect(): Promise<void> {
    let hasError = false;
    try {
      let tokenResponse = await this.oktaAuth.token.parseFromUrl();
      this._accessToken = tokenResponse.tokens.accessToken?.accessToken ?? null;
      this._idToken = tokenResponse.tokens.idToken?.idToken ?? null;
      const appPayload = await this.getAppPayload();
      if (appPayload.token) {
        //////////////////////////////////////////////////////////////////////////////////
        //TODO: there is some weird interactions that go on between these two things...
        // If you reverse the calls to the next two lines there is a flicker that occurs
        // kinda implying it is overdoing some thinking that probably does not need to
        // be there.  For now it is fine, but it should probably be investigated...
        SpaStore.processPayload(appPayload, false);
        WidgetStore.initialize(appPayload);
        //////////////////////////////////////////////////////////////////////////////////
      } else {
        // Getting to this point without a token indicates that authentication failed
        WidgetStore.authState = AuthState.AuthFailed;
      }
    } catch (ex) {
      hasError = true;
    }

    if (
      hasError ||
      Utils.isNullOrEmpty(this._accessToken) ||
      Utils.isNullOrEmpty(this._idToken)
    ) {
      throw new Error(
        "OKTA authentication failed to acquire Access and/or ID tokens during the login process."
      );
    }
  }

  /**
   * Responsible for logging the user out of the authentication provider.
   */
  async logoutAsync(): Promise<void> {}

  /**
   *  Grants admin access for the current user & org
   */
  async grantAdminAsync(): Promise<void> {}

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Allows consumers of the provider to pass in OKTA settings to configure the provider.
   */
  applySettings(settings: OrgOktaSettings): void {
    const oktaConfig: OktaAuthOptions = {
      issuer: settings.issuerUrl,
      clientId: settings.clientId,
      redirectUri: window.location.origin + "/public/sso/loginOkta",
      scopes: ["openid", "email", "profile", "phone"],
      pkce: true,
    };
    this._oktaAuth = new OktaAuth(oktaConfig);
  }

  /**
   * Retrieves the application payload using the MS Graph App token.
   * @param userInfoToSync - user information from MS Graph to sync with Soundbite.
   */
  private async getAppPayload(): Promise<AppPayload> {
    try {
      const orgRoute = BrowserStorageService.lastOktaSettings?.orgRoute;
      const url =
        process.env.REACT_APP_CONFIG_API_PREFIX +
        `/oktaAuth/login?orgRoute=${orgRoute}`;
      const headers = {
        includeToken: false,
        Authorization: `Bearer ${this._accessToken}`,
        "Content-Type": "application/json",
      };

      const appPayload = await SoundbiteApiConfig.httpAdapter.post<AppPayload>(
        url,
        null,
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
}

export const OktaProvider = new OktaProviderClass();
