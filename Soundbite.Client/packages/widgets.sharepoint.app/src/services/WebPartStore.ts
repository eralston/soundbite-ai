import { AadTokenProvider } from "@microsoft/sp-http";
import { WebPartContext } from "@microsoft/sp-webpart-base";
import { AppPayload, Logger, SoundbiteApiConfig } from "@soundbite/api";
import { setupAxiosAdapter } from "@soundbite/api-axios";
import { WidgetStore } from "@soundbite/widgets-react";
import { action, makeObservable, observable } from "mobx";
import { WebPartServiceState } from "../enums/WebPartServiceState";

//////////[ BEGIN - Image Locations ]///////////////////////////////////////////////////////////////
//NOTE: Do not modify anything in this section without accounting for the change in the web part
//      script.  Content in this section changes when CDN settings are updated.
SoundbiteApiConfig.imgAvatarUrl =
  "https://storage.usw.soundbite.cloud/cdn/spfx/autoVersion/v1.0.0.3/default-avatar.png";
SoundbiteApiConfig.imgEmptyCardArtUrl =
  "https://storage.usw.soundbite.cloud/cdn/spfx/autoVersion/v1.0.0.3/empty-card-art.png";
//////////[ END - Image Locations ]/////////////////////////////////////////////////////////////////

////////////////////////////////////////////////////////////////////////////////////////////////////
// DO NOT modify or remove the BEGIN/END comments as they mark a region of code that is updated
// by the WebPartScript.ps1 script when targeting different web part environments.  These values
// are automatically set by that script.  You can modify the values of the azureApiId and the apiUrl
// if needed, but do not change anything about comments surrounding them.
//////////[ BEGIN - API Settings ]//////////////////////////////////////////////////////////////////
const sbEnv = {
  azureApiId: "api://usw.soundbite.cloud/e852716e-f657-42f1-b81b-f3c06d2b37c9",
  apiUrl: "https://usw.soundbite.cloud/api/v1",
};
const sbwpverValue: string = "sv1.0.0.3"; //////////[ END - API Settings ]////////////////////////////////////////////////////////////////////

/**
 * Shared service class responsible for initial data calls to the Soundbite API.
 */
class WebPartStoreClass {
  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      state: observable,
      initialize: action,
    });

    // Make sure to setup the axios HTTP client for the Soundbite API
    setupAxiosAdapter();
  }

  //////////[ Variables ]///////////////////////////////////////////////////////////////////////////

  /** Stores the API url passed into the initialization function **/
  private apiUrl: string;
  private webPartContext: WebPartContext;

  //////////[ Observables ]/////////////////////////////////////////////////////////////////////////

  /** Stores the initialization state of the service */
  public state: WebPartServiceState = WebPartServiceState.none;

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Initializes the initial data payload that feeds web parts
   * @param context - reference to the web part context within the page
   */
  public async initialize(context: WebPartContext): Promise<void> {
    // Store off data for use later when re-acquiring tokens when they expire.
    this.apiUrl = sbEnv.apiUrl;
    this.webPartContext = context;

    // Make sure the apiUrl does NOT end with a slash
    if (this.apiUrl.charAt(this.apiUrl.length - 1) === "/") {
      this.apiUrl = this.apiUrl.substr(0, this.apiUrl.length - 1);
    }

    if (this.state !== WebPartServiceState.ok) {
      this.state = WebPartServiceState.init;
      if (this.validateSettings()) {
        // Attempt to acquire the token
        try {
          if (await this.getAccessToken()) {
            // Attempt to acquire the organization data
            try {
              // Make sure to include the sbwpver header which identifies the web part version the
              // client is using.  This helps us keep tabs on which versions are out in the wild
              const payload =
                await SoundbiteApiConfig.httpAdapter.post<AppPayload>(
                  `${this.apiUrl}/azureAuth/LoginSharePoint`,
                  null,
                  { headers: { sbwpver: sbwpverValue } }
                );
              if (payload) {
                WidgetStore.initialize(payload);
                this.state = WebPartServiceState.ok;
              } else {
                // API call failed because there was no data present
                Logger.LogError(
                  "API call to retrieve org data failed because response contained no data."
                );
                this.state = WebPartServiceState.apiFailed;
              }
            } catch (ex) {
              // API call failed due to an exception
              Logger.LogError("API call to retrieve org data failed.", ex);
              this.state = WebPartServiceState.apiFailed;
            }
          }
        } catch {
          // Failed to retrieve token
          this.state = WebPartServiceState.tokenFailed;
          Logger.LogError("Failed to retrieve token for user.");
        }
      } else {
        // Settings are invalid
        this.state = WebPartServiceState.badConfig;
        Logger.LogError("Configuration is invalid.");
      }
    }
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Responsible for acquiring an access token from Azure Active Directory (AAD). This method uses
   * the AadTokenProvider which can refresh the page in pursuit of the access token, and for some
   * reason this can keep happening over and over again.  So there is additional code in this method
   * to try to detect an infinite loop like this and exit the loop before the user gets too upset.
   */
  private async getAccessToken(): Promise<boolean> {
    const storageKey = "sbtknfo";
    let tokenInfo: string = sessionStorage.getItem(storageKey) ?? "0|0";
    let split = tokenInfo.split("|");
    let count = parseInt(split[0]);
    let lastDate = parseInt(split[1]);

    // Determine if the last date a token event occured is outside of 60 seconds
    if (new Date().getTime() - lastDate > 60000) {
      // Since it was outside of 60 seconds we reset the counter
      count = 0;
    }

    // Only allow token retrieval 3 times before giving up because we do not want to send
    // the user into an infinite token retrieval loop.
    if (count < 3) {
      // Increment the counter
      count += 1;

      // Store off the token counter information
      sessionStorage.setItem(storageKey, `${count}|${new Date().getTime()}`);

      // Attempt to acquire the token (this may cause a page refresh)
      const tokenProvider: AadTokenProvider =
        await this.webPartContext.aadTokenProviderFactory.getTokenProvider();
      const token = await tokenProvider.getToken(sbEnv.azureApiId);
      SoundbiteApiConfig.ApiPrefixUrl = this.apiUrl;
      SoundbiteApiConfig.getToken = () => Promise.resolve(token);
      return true;
    } else {
      Logger.LogError(
        "Failed to get access token from SharePoint after 3 attempts."
      );
      sessionStorage.removeItem(storageKey);
      this.state = WebPartServiceState.tokenFailed;
      return false;
    }
  }

  /**
   * Determines whether configuration settings are valid
   */
  private validateSettings(): boolean {
    return true;
  }
}

export const WebPartStore = new WebPartStoreClass();
