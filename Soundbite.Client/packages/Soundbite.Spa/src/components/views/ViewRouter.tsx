import React, { useEffect } from "react";
import { CardHeader, Card } from "reactstrap";
import { Route, Switch, Redirect, useParams } from "react-router";
import { observer } from "mobx-react-lite";

import { PersonRole, SessionSecurityType, UserRole } from "@soundbite/api";
import {
  AuthState,
  Loader,
  ShowWhen,
  WidgetStore,
} from "@soundbite/widgets-react";

import { Calendar } from "./Calendar";
import { Feed } from "./Feed";
import { Group } from "./Group";
import { Groups } from "./Groups";
import { Home } from "./Home";
import { IFrameFeed } from "./IFrameFeed";
import { IFrameSessionEditor } from "./IFrameSessionEditor";
import { LoginO365 } from "../login/LoginO365";
import { LoginOkta } from "../login/LoginOkta";
import { Notifications } from "./Notifications";
import { Past } from "./Past";
import { Pending } from "./Pending";
import { People } from "./People";
import { Performance } from "./Performance";
import { Permissions } from "./Permissions";
import { PublicSession } from "./PublicSession";
import { Reports } from "./Reports";
import { SingleSignOn } from "../login/SingleSignOn";
import { Sync } from "./Sync";
import AdminHomePage from "../../admin/components/views/Home";
import AdminOrgEditPage from "../../admin/components/views/OrganizationEdit";
import AdminOrgsPage from "../../admin/components/views/OrganizationList";
import NavService from "../../sdk/NavService";
import Public from "../views/Public";
import SimpleDlg from "../dialogs/SimpleDlg";
import SpaStore from "../../store/Spa.Store";
import UnAuthorized from "./UnAuthorized";
import { faPeopleGroup } from "@fortawesome/free-solid-svg-icons";
import { SessionSettings } from "./SessionSettings";
import { AzureSettings } from "./AzureSettings";
import { OrgTokenSettings } from "./OrgTokenSettings";
import { Sandbox } from "./Sandbox";
import Theme from "./Theme";

/**
 * Contains view routing information for the application.
 */
export const ViewRouter: React.FC = observer(() => {
  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  async function initialize() {
    await SpaStore.initialize();
  }

  useEffect(() => {
    initialize();
  }, []);

  //////////[ Routes ]//////////////////////////////////////////////////////////////////////////////

  /**
   * Public Routes are accessible without any authentication requirements.  These routes will only
   * be loaded after application initialization, so SPA configuration is guaranteed ready before any
   * components are rendered.
   */
  const PublicRoutes: React.FC = () => {
    return (
      <Switch>
        <Route exact path="/public/sso" component={SingleSignOn} />
        <Route exact path="/public/sso/loginO365" component={LoginO365} />
        <Route exact path="/public/sso/loginOkta" component={LoginOkta} />
        <Route
          exact
          path="/public/organizations/:orgRoute/sessions/:sessionRoute"
          component={PublicSession}
        />
        <Redirect to="/" />
      </Switch>
    );
  };

  /**
   * Contains the authentication wall logic that stops a user from accessing protected routes until
   * they have been fully authenticated.
   */
  const AuthenticationWall: React.FC = () => {
    switch (WidgetStore.authState) {
      case AuthState.AuthFailed:
        return showMessage("Authorization failed.");
      case AuthState.ThirdPartyAuthFailed:
        return showMessage("External Authorization failed.");
      case AuthState.Authorizing:
        return showMessage("Authorizing...");
      case AuthState.AuthRequired:
        // Store off the location user was attempting to access
        NavService.setReturnPage(
          window.location.href.substring(
            window.origin.length,
            window.location.href.length
          )
        );

        // Nuance here,but if there is a match on / it means the user is logging in from the home
        // screen which is generally expected.  Otherwise they are attempting to access a protected
        // page so we show a different messaging indicating that they need to login to access it.
        return (
          <Switch>
            <Route
              exact
              path="/"
              render={() => <SingleSignOn message="Single Sign-On (SSO)" />}
            />
            <Route
              path="/"
              render={() => <SingleSignOn message="Single Sign-On (SSO)" />}
            />
          </Switch>
        );
      default:
        // By virtue of reaching this point the login return page should be cleared.
        NavService.setReturnPage(null);
        return <ProtectedRoutes />;
    }
  };

  /**
   * Protected Routes are accessible only after the user has successfully authenticate. These routes
   * will only be loaded after application initialization, so SPA configuration is guranteed ready
   * before any components are rendered.  User information is also guaranteed ready.
   */
  const ProtectedRoutes: React.FC = () => {
    return (
      <Switch>
        <Route exact path="/admin/" component={AdminHomePage} />
        <Route exact path="/admin/orgs/" component={AdminOrgsPage} />
        <Route
          exact
          path="/admin/orgs/:orgRoute"
          component={AdminOrgEditPage}
        />
        <Route path="/organizations/:orgRoute/" component={OrgRoutes} />
        {/* Fallback Redirect */}
        <Redirect path="/" to="/" />
      </Switch>
    );
  };

  /**
   * Organization Routes are protected routes that pertain to an organization.
   */
  const OrgRoutes: React.FC = () => {
    const params = useParams() as any;
    const orgRoute = params.orgRoute;
    WidgetStore.organizations.currentOrgRoute = orgRoute;

    return (
      <Switch>
        <Redirect
          exact
          from="/organizations/:orgRoute"
          to="/organizations/:orgRoute/feed"
        />
        <Route path="/organizations/:orgRoute/sandbox" component={Sandbox} />
        <Route
          path="/organizations/:orgRoute/sessionEditor/iframe"
          component={IFrameSessionEditor}
        />
        <Route
          path="/organizations/:orgRoute/feed/iframe"
          component={IFrameFeed}
        />
        <Route path="/organizations/:orgRoute/feed">
          <Feed
            orgRoute={orgRoute}
            groupRoute={""}
            securityType={SessionSecurityType.Protected}
          />
        </Route>
        <Route path="/organizations/:orgRoute/scheduled">
          <Pending orgRoute={orgRoute} />
        </Route>
        <Route path="/organizations/:orgRoute/history">
          <Past orgRoute={orgRoute} />
        </Route>
        <Route path="/organizations/:orgRoute/public">
          <Feed
            groupRoute={""}
            icon={faPeopleGroup}
            orgRoute={orgRoute}
            securityType={SessionSecurityType.Public}
          />
        </Route>
        <Route path="/organizations/:orgRoute/calendar" component={Calendar} />
        {/*Support legacy URLs before the refactor from "teams" to "groups"*/}
        <Redirect
          from="/organizations/:orgRoute/teams/:groupRoute"
          to="/organizations/:orgRoute/groups/:groupRoute"
        />
        <Redirect
          exact
          from="/organizations/:orgRoute/groups/:groupRoute"
          to="/organizations/:orgRoute/groups/:groupRoute/feed"
        />
        <Route
          path="/organizations/:orgRoute/groups/:groupRoute"
          component={OrgTeamRoutes}
        />
        <OrgAdminRoutes orgRoute={params.orgRoute} />
        <Redirect path="/" to="/" />
      </Switch>
    );
  };

  /**
   * Organization Routes that are only availabile to admins in an organization.
   */
  const OrgAdminRoutes: React.FC<{ orgRoute: string }> = (props: {
    orgRoute: string;
  }) => {
    const isAdminOrMore =
      (WidgetStore.users.currentUser?.userRole ?? 0) >= UserRole.God ||
      (WidgetStore.organizations.currentOrg?.details.me?.personRole ?? 0) >=
        PersonRole.Admin;

    if (isAdminOrMore) {
      return (
        <Switch>
          {/*Support Legacy URLs before renaming from "settings" to "directory"*/}
          <Redirect
            from="/organizations/:orgRoute/settings"
            to="/organizations/:orgRoute/directory"
          />
          <Redirect
            exact
            from="/organizations/:orgRoute/directory"
            to="/organizations/:orgRoute/directory/users"
          />
          <Route
            path="/organizations/:orgRoute/directory/users"
            component={People}
          />
          <Route
            path="/organizations/:orgRoute/directory/groups"
            component={Groups}
          />
          <Route
            path="/organizations/:orgRoute/directory/sync"
            component={Sync}
          />
          <Route
            path="/organizations/:orgRoute/notifications"
            component={Notifications}
          />
          <Route
            path="/organizations/:orgRoute/permissions"
            component={Permissions}
          />
          <Route
            path="/organizations/:orgRoute/sessions/settings"
            component={SessionSettings}
          />
          <Route
            path="/organizations/:orgRoute/azureSettings"
            component={AzureSettings}
          />
          <Route
            path="/organizations/:orgRoute/orgTokenSettings"
            component={OrgTokenSettings}
          />
          <Route path="/organizations/:orgRoute/reports" component={Reports} />
          <Route path="/organizations/:orgRoute/theme" component={Theme} />
          <Route
            path="/organizations/:orgRoute/performance"
            component={Performance}
          />
        </Switch>
      );
    } else {
      return null;
    }
  };

  /**
   * Team routes that pertain to a specific team within an organization.
   */
  const OrgTeamRoutes: React.FC = () => {
    const params = useParams() as any;
    const orgRoute = params.orgRoute;
    const groupRoute = params.groupRoute;

    return (
      <Switch>
        <Route path="/organizations/:orgRoute/groups/:groupRoute/feed">
          <Feed
            orgRoute={orgRoute}
            groupRoute={groupRoute}
            securityType={SessionSecurityType.Protected}
          />
        </Route>
        <Route path="/organizations/:orgRoute/groups/:groupRoute/public">
          <Feed
            orgRoute={orgRoute}
            groupRoute={groupRoute}
            securityType={SessionSecurityType.Public}
          />
        </Route>
        <Route
          path="/organizations/:orgRoute/groups/:groupRoute/calendar"
          component={Calendar}
        />
        <Route
          path="/organizations/:orgRoute/groups/:groupRoute/scheduled"
          component={Pending}
        >
          <Pending orgRoute={orgRoute} groupRoute={groupRoute} />
        </Route>
        <Route
          path="/organizations/:orgRoute/groups/:groupRoute/history"
          component={Pending}
        >
          <Past orgRoute={orgRoute} groupRoute={groupRoute} />
        </Route>
        <Route
          path="/organizations/:orgRoute/groups/:groupRoute/members"
          component={Group}
        />
      </Switch>
    );
  };

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  /**
   * Closes the welcome dialog
   */
  function onWelcomeClose(): void {
    if (WidgetStore.users.currentUser) {
      WidgetStore.users.currentUser.isAccepting = false;
    }
  }

  //////////[ UI Methods ]//////////////////////////////////////////////////////////////////////////

  /**
   * Displays a message to the user.
   * @param message - message to display to the user
   */
  function showMessage(message: string) {
    return (
      <Public>
        <Card>
          <CardHeader className="rounded">
            <div className="text-muted text-center mt-2 mb-3">
              <Loader isLoadedWhen={false} message={message} />
            </div>
          </CardHeader>
        </Card>
      </Public>
    );
  }

  /**
   * Displays the welcome dialog when a user has an invitation to sign up to Soundbite.
   */
  function showWelcome(): JSX.Element {
    return (
      <ShowWhen is={WidgetStore.users.currentUser?.isAccepting}>
        <SimpleDlg
          title="Welcome to Soundbite.ai"
          isOpen={WidgetStore.users.currentUser?.isAccepting}
          onClose={onWelcomeClose}
        >
          <h1>Welcome!</h1>
          <p>
            Congratulations! You are one step closer to taking control of your
            calendar and discovering a better, more flexible way of
            communicating!
          </p>
          <p>
            <a
              href="https://Soundbite.ai"
              target="_blank"
              rel="noopener noreferrer"
            >
              Soundbite.ai
            </a>{" "}
            lets you use podcast-style recordings to conduct virtual meetings,
            share personal messages, team announcements, and gather
            collaborative feedback - on your schedule.
          </p>
          <p>
            With Soundbite.ai, you will reduce the amount of time spent in
            meetings, collaborate and communicate better, and do so more
            flexibility and efficiency than with email.
          </p>
          <p>The Soundbite.ai team</p>
        </SimpleDlg>
      </ShowWhen>
    );
  }

  //////////[ Render Component ]////////////////////////////////////////////////////////////////////

  switch (WidgetStore.authState) {
    case AuthState.Initializing:
      // Do not allow route processing until the SPA has initialized
      return showMessage("Tuning up the orchestra...");
    case AuthState.OrganizationSwitch:
    case AuthState.OrganizationSelect:
      // Display the home component if the user needs to select an organization.
      return <Home />;
    default:
      // SPA is initialized so process routes
      return (
        <>
          <Switch>
            <Route exact path="/" component={Home} />
            <Route exact path="/unauthorized" component={UnAuthorized} />
            <Route path="/public" component={PublicRoutes} />
            <Route path="/" component={AuthenticationWall} />
          </Switch>
          {showWelcome()}
        </>
      );
  }
});
