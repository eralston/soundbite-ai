import { User, UserRole, ProviderType } from "@soundbite/api";
import SpaStore from "../store/Spa.Store";
import Sdk from "./Service";

/**
 * The mapping of intentions to endpoint URLs
 * */
export enum GraphEndpoints {
  Me = "https://graph.microsoft.com/v1.0/me/",
  MyPhoto = "https://graph.microsoft.com/v1.0/me/photo/$value",
}

// GraphEndpoints.Me
export interface IGraphMe {
  businessPhones: string[];
  displayName: string;
  givenName: string;
  jobTitle: string;
  mail: string;
  mobilePhone?: any;
  officeLocation?: any;
  preferredLanguage: string;
  surname: string;
  userPrincipalName: string;
  id: string;
}

/**
 * Class for interacting with MS Graph
 * This is to do directly from the client what we may not be able to do from the server w/o admin consent
 * Should mirror Soundbite.App.Graph.IGraph as much as possible
 * */
export default class GraphSdk extends Sdk {
  async tokenAsync() {
    return Promise.resolve(SpaStore.token); // Identity Token Needed for this
  }

  /**
   * Async retrives the "Me" object from graph
   * */
  public async myUserAsync(): Promise<User> {
    const me = await this.getAsync<IGraphMe>(GraphEndpoints.Me);

    const phone: string = me.businessPhones[0];
    const userFields: User = {
      email: me.mail,
      givenName: me.givenName,
      familyName: me.surname,
      phone: phone,
      title: me.jobTitle,
      allowNews: true,
      allowMarketing: true,
      allowEmail: true,
      allowSms: true,
      userRole: UserRole.Unknown,
      calendarSettings: "",
      providerType: ProviderType.AAD,
    } as User;

    return userFields;
  }

  /**
   * With great regret I must acknowledge that returning the image blob as an any seems the most practical
   * */
  public async myImageAsync(): Promise<Blob> {
    return await this.getBlobAsync(GraphEndpoints.MyPhoto);
  }
}
