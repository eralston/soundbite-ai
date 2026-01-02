import React from "react";
import { observer } from "mobx-react-lite";
import { useHistory } from "react-router-dom";
import { PublicError, SoundbiteApiConfig, Utils } from "@soundbite/api";
import { setupAxiosAdapter } from "@soundbite/api-axios";

import NavService from "./sdk/NavService";
import { ViewRouter } from "./components/views/ViewRouter";
import { SingletonDialogs } from "./components/dialogs/SingletonDialogs";
import { ClientOpService } from "./sdk/ClientOpService";
import { DialogManagerUi, ErrorDlg } from "@soundbite/widgets-react";
import SpaStore from "./store/Spa.Store";
import { VersionNumber } from "./components/controls/VersionNumber";
import { BrowserStorageService } from "./sdk/BrowserStorageService";

const App: React.FC = observer(() => {
  // Initialize Soundbite API
  setupAxiosAdapter();
  SoundbiteApiConfig.ApiPrefixUrl = (window as any).SbApiPrefixUrl;
  SoundbiteApiConfig.clientOpsHandler = ClientOpService.processClientOps;
  function getToken(): Promise<string | null> {
    return Promise.resolve(SpaStore.token);
  }
  SoundbiteApiConfig.getToken = getToken;

  // Initialize the navigation service and pass in a delegate to handle routing requests
  const history = useHistory();
  NavService.initialize((path) => {
    history.push(path);
  });

  /**
   * Setup top-level handler for unhandled errors.
   * @param event - contains event information about the exception.
   * @param source - identifies the source of the exception
   * @param lineno - line number identifying the location where the error occurred in code
   * @param colno - column number identifying the location where the error occurred in code
   * @param error - contains error information
   */
  window.onerror = (
    event: Event | string,
    source?: string,
    lineno?: number,
    colno?: number,
    error?: Error
  ): any => {
    if (error) {
      ErrorDlg.show(error);
    } else {
      console.log("[SB]-Unhandled Error", error);
    }
    return false;
  };

  /**
   * Setup top-level handler for any unhandled unresolved promise rejections.
   * @param event - contains information about the unhandled unresolved promise
   */
  window.onunhandledrejection = (event: PromiseRejectionEvent) => {
    if (event) {
      if (event.reason instanceof PublicError) {
        ErrorDlg.show(event.reason as PublicError);
      } else {
        ErrorDlg.show(new PublicError(undefined, "Unknown Error", ""));
      }
      event.cancelBubble = true;
    }
  };

  // If the application starts and the URL contains an organization route
  // then store it off because the user is clearly intending to enter the
  // application with that organization.
  const initialOrgRoute = Utils.getOrgRouteFromPath(window.location.pathname);
  if (!Utils.isNullOrEmpty(initialOrgRoute)) {
    BrowserStorageService.lastSelectedOrg = initialOrgRoute;
  }

  return (
    <React.Fragment>
      <ViewRouter />
      <DialogManagerUi />
      <SingletonDialogs />
      <VersionNumber />
    </React.Fragment>
  );
});

export default App;
