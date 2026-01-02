import {
  Group,
  GroupsService,
  OrganizationsService,
  Person,
  SessionPreview,
  SessionsService,
  SoundbiteApiConfig,
} from "@soundbite/api";
import { action, makeObservable, observable } from "mobx";
import { LoadState } from "../models/Enums";

class AppStoreClass {
  ///////////[ Variables ]//////////////////////////////////////////////////////////////////////////

  private initializationPromise: Promise<void> | null = null;
  parentWindow: Window | null = null;

  ///////////[ Observable Data ]////////////////////////////////////////////////////////////////////

  AuthToken: string = "";
  AppStoreState: LoadState = LoadState.NotLoaded;
  AppStoreErrorMsg: string = "";
  CurrentWiget: string = "none";
  CurrentWidgetData: any;
  MyUser: Person = {} as Person;
  MyOrgRoute: string = "";
  MyFeed: SessionPreview[] = [];
  MyGroups: Group[] = [];

  ///////////[ Constructor ]////////////////////////////////////////////////////////////////////////

  constructor() {
    this.CurrentWiget = ""; //"Teams";

    makeObservable(this, {
      CurrentWiget: observable,
      CurrentWidgetData: observable,
      MyGroups: observable,
      MyUser: observable,
      MyOrgRoute: observable,
      initializeAsync: action,
      refreshOrgAsync: action,
      setCurrentWidget: action,
    });
  }

  ///////////[ Methods ]////////////////////////////////////////////////////////////////////////////

  /**
   * Responsible for initializing the application store.
   * @param authToken - authorization token used to make calls into the API.
   * @param orgRoute - orgRoute identifying the organization to which the user is associated.
   */
  async initializeAsync(authToken: string, orgRoute: string): Promise<void> {
    // Determine whether we can simply
    if (
      this.initializationPromise === null ||
      this.AppStoreState === LoadState.Failed
    ) {
      this.initializationPromise = new Promise(async (resolve, reject) => {
        // Make sure we have the appropriate data to continue
        if (!(authToken && orgRoute)) {
          this.AppStoreState = LoadState.Failed;
          if (!authToken) {
            this.AppStoreErrorMsg =
              "Cannot initialize widget application without authorization token.";
          } else if (!orgRoute) {
            this.AppStoreErrorMsg =
              "Cannot initialize widget application without organization route.";
          } else {
            this.AppStoreErrorMsg =
              "Cannot initialize widget application for unknown reason.";
          }
        }

        // Initialize as long as the data is present
        if (this.AppStoreState !== LoadState.Failed) {
          // Make sure that API calls receive the authentication token
          this.AuthToken = authToken;
          SoundbiteApiConfig.getToken = () => Promise.resolve(authToken);
          this.MyOrgRoute = orgRoute;
          await this.refreshOrgAsync();

          // Getting to this point without an error indicates successful loading
          this.AppStoreState = LoadState.Loaded;
        }

        // Always resolve (error data will be loaded into the application state)
        resolve();
      });
    }

    return this.initializationPromise;
  }

  setCurrentWidget(currentWidget: string, currentWidgetData: any): void {
    this.CurrentWiget = currentWidget;
    this.CurrentWidgetData = currentWidgetData;
  }

  async refreshOrgAsync(): Promise<void> {
    try {
      // Acquire user/organization/feed information
      const orgFeedPromise = SessionsService.readFeedAsync(this.MyOrgRoute);
      const orgDetailsPromise = OrganizationsService.readAsync(this.MyOrgRoute);
      const [orgFeed, orgDetails] = await Promise.all([
        orgFeedPromise,
        orgDetailsPromise,
      ]);

      if (orgDetails.details.me) {
        this.MyUser = orgDetails.details.me;
        this.MyGroups = await GroupsService.readMyGroupsAsync(this.MyOrgRoute);
        this.MyFeed = orgFeed;
      } else {
        this.MyOrgRoute = "";
        this.AppStoreState = LoadState.Failed;
        this.AppStoreErrorMsg =
          "Cannot initialize widget application because requested organization data is missing.";
      }
    } catch {
      this.AppStoreState = LoadState.Failed;
      this.AppStoreErrorMsg =
        "Cannot initialize widget application because request for organization data failed.";
    }
  }
}
export const AppStore = new AppStoreClass();
