import { ClientOp, ClientOpRunInfo, IHttpAdapter } from "..";

/**
 * Exposes configuration settings for the Soundbite API
 */
class SoundbiteApiConfigClass {
  /////[ Private Fields ]/////////////////////////////////////////////////////////////////////////

  private _httpAdapter: IHttpAdapter | null = null;
  private _apiPrefixUrl = "https://usw.soundbite.cloud/api/v1";

  /////[ Public Properties ]//////////////////////////////////////////////////////////////////////

  // NOTE: The imgAvatarUrl and the imgEmptyCardArtUrl were created primarily to provide support
  //       for including images in the client-side assets within the SharePoint solution.

  /**
   * Gets or sets the URL of the default avatar image.
   */
  public imgAvatarUrl: string = "/img/default-avatar.png";

  /**
   * Gets or sets the URL of the empty card art image.
   */
  public imgEmptyCardArtUrl: string = "/img/empty-card-art.png";

  /***
   * Gets or sets a function to acquire an authentication token for calls to the Soundbite platform.
   */
  public getToken: () => Promise<string | null> = () => {
    return Promise.resolve(null);
  };

  /**
   * Gets the URL prefix for Soundbite API calls.  Value defaults to the production URL
   * for the Soundbite Platform but can be modified to point to test/debug environments as needed.
   */
  public get ApiPrefixUrl() {
    return this._apiPrefixUrl;
  }

  /**
   * Sets the URL prefix for Soundbite API calls.
   */
  public set ApiPrefixUrl(value: string) {
    // Ensure there is no trailing slash because our API calls assume it is not there.
    if (value && value[value.length - 1] === "/") {
      value = value.substring(0, value.length - 1);
    }
    this._apiPrefixUrl = value;
  }

  /**
   * Gets or sets the client operation handler which is responsible for completing requests on the
   * client that could not be completed on the server.  Most client operations are requested because
   * the server has no way to authenticate the user into third-party applications.  The handler
   * assigned to this property by default does not handle any client operations.
   */
  public clientOpsHandler: (
    clientOps?: ClientOp[]
  ) => Promise<ClientOpRunInfo[]> = (clientOps?: ClientOp[]) => {
    return new Promise<ClientOpRunInfo[]>((resolve, reject) => {
      // Do nothing and return an empty array indicating no errors.  This is a default implementation
      // that is not responsible for handling anything.
      resolve([] as ClientOpRunInfo[]);
    });
  };

  /***
   * Gets the IHttpAdapter used to make HTTP calls to the Soundbite API.
   */
  public get httpAdapter(): IHttpAdapter {
    if (this._httpAdapter === null) {
      throw "HTTP adapter has not been set in SoundbiteApiConfig";
    } else {
      return this._httpAdapter;
    }
  }

  /***
   * Sets the IHttpAdapter used to make HTTP calls to the Soundbite API.
   */
  public set httpAdapter(value: IHttpAdapter) {
    this._httpAdapter = value;
  }
}

export const SoundbiteApiConfig = new SoundbiteApiConfigClass();
