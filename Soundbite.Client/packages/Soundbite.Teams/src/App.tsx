import React, { useEffect } from "react";
import { Route, Switch } from "react-router-dom";
import { FeedTab } from "./views/FeedTab";
import { FeedTabConfig } from "./views/FeedTabConfig";
import { DefaultView } from "./views/DefaultView";
import { CreateSoundbiteView } from "./views/CreateSoundbiteView";
import "./App.css";
import { observer } from "mobx-react-lite";
import { TeamsAppStore } from "./services/TeamsAppStore";
import {
  AuthState,
  DialogManagerUi,
  Loader,
  SbBootstrapStyles,
  SbDashboardStyles,
  SbStyles,
  WidgetStore,
  VideoEditorContext,
} from "@soundbite/widgets-react";

VideoEditorContext.loadDevices();

const App: React.FC = observer(() => {
  async function initialize() {
    await TeamsAppStore.initialize();
  }

  useEffect(() => {
    initialize();
  }, []);

  const tm = (
    <span style={{ fontWeight: "bold", fontSize: "8px", verticalAlign: "top" }}>
      TM
    </span>
  );

  function renderContent() {
    switch (WidgetStore.authState) {
      case AuthState.Authorized:
        return renderRoutes();

      case AuthState.Authorizing:
      case AuthState.Initializing:
      case AuthState.ThirdPartyAuthorizing:
      case AuthState.ThirdPartyAuthorized:
        return renderLoading();

      case AuthState.InitializationFailed:
        return renderMessage(
          "App Initialization Failed",
          <>
            <div>
              Failed to retrieve application configuration data from the
              Soundbite{tm} API. Please try reloading the tab. If the issue
              persists, please contact support{" "}
              <b>
                <a href="mailto:support@soundbite.ai">support@soundbite.ai</a>
              </b>{" "}
              for assistance.
            </div>
          </>
        );

      case AuthState.AuthFailed:
      case AuthState.AuthRequired:
        return renderMessage(
          "Sign Up for Soundbite",
          <>
            <div>
              You do not apear to have an account with Soundbite{tm}. An account
              is required before you can use the Soundbite{tm} App for Microsoft
              Teams. If you believe you have reached this screen as a result of
              an error, then please try refreshing the tab or contacting support
              (
              <b>
                <a href="mailto:support@soundbite.ai">support@soundbite.ai</a>
              </b>
              ).
            </div>
            <br />
            <div>
              Please visit{" "}
              <b>
                <a href="www.soundbite.ai">www.Soundbite.ai</a>
              </b>{" "}
              for more information on setting up an account with Soundbite.
            </div>
          </>
        );

      case AuthState.OrganizationSelect:
      case AuthState.OrganizationSwitch:
        if ((WidgetStore.organizations.myOrgs?.length ?? 0) > 0) {
          return renderMessage(
            "Organization Config Issue",
            <div>
              You are in one or more organizations, but the organization could
              not be identified because it is not linked to Azure ID. Please
              contact Soundbite{tm} support (
              <b>
                <a href="mailto:support@soundbite.ai">support@soundbite.ai</a>
              </b>
              ) and request for your organization to be associated with the
              appropriate Azure ID.
            </div>
          );
        } else {
          return renderMessage(
            "No Organizations",
            <div>
              You have a valid Soundbite{tm} account, but you are not currently
              in any organizations. Please contact support (
              <b>
                <a href="mailto:support@soundbite.ai">support@soundbite.ai</a>
              </b>
              ) to ensure you have been assigned to an organization.
            </div>
          );
        }

      default:
        return renderMessage(
          "Invalid Auth State",
          <div>Application has entered an unknown authorization state</div>
        );
    }
  }

  function renderMessage(title: string, content: JSX.Element) {
    return (
      <div style={{ padding: "20px", fontSize: "10pt" }}>
        <h3>{title}</h3>
        <hr style={{ margin: "10px" }} />
        <div>{content}</div>
      </div>
    );
  }

  function renderRoutes() {
    return (
      <Switch>
        <Route path="/Create" component={CreateSoundbiteView} />
        <Route path="/FeedTab/:teamRoute?" component={FeedTab} />
        <Route path="/FeedTabConfig" component={FeedTabConfig} />
        <Route path="/" component={DefaultView} />
      </Switch>
    );
  }

  function renderLoading() {
    return <Loader />;
  }

  return (
    <div className="appContainer">
      <SbBootstrapStyles />
      <SbDashboardStyles />
      <SbStyles />
      <div className="appContainerInner">{renderContent()}</div>
      <DialogManagerUi />
    </div>
  );
});

export default App;
