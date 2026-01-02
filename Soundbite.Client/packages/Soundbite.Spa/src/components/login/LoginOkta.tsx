import React, { useEffect, useState } from "react";
import { CardHeader, Card } from "reactstrap";
import { Link } from "react-router-dom";
import { observer } from "mobx-react-lite";
import {
  OktaAuthService,
  OrgOktaSettingsWithOrgRoute,
  Utils,
} from "@soundbite/api";
import { Loader } from "@soundbite/widgets-react";
import Public from "../views/Public";
import { OktaProvider } from "../../sdk/providers/auth/OktaProvider";
import { BrowserStorageService } from "../../sdk/BrowserStorageService";

//////////[ Component Definition ]//////////////////////////////////////////////////////////////////

enum DisplayMode {
  Loading,
  GetOrgId,
  GetOrgIdFailed,
  NoOktaSettingsOnReturn,
  LoginRedirectFailed,
  SpaLoginFailed,
}

/**
 * Responsible for handling the OKTA oauth redirect
 */
export const LoginOkta: React.FC = observer(() => {
  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  const [hasCode] = useState<boolean>(
    window.location.search?.split("code=").length > 1
  );
  const [displayMode, setDisplayMode] = useState<DisplayMode>(
    DisplayMode.Loading
  );
  const [canSubmit, setCanSubmit] = useState(false);
  const [email, setEmail] = useState<string | null>(null);

  /**
   * Responsible for trying to determine the organization ID without interacting with the user. If
   * this method succeeds, then the OKTA settings can be retrieved without requiring the user to
   * enter any identifying information.  Otherwise, the user must enter some identity information.
   */
  function getOrgRouteNonInteractive(): string | null {
    //TODO: not currently fully implemented.  Most of the time users are coming in from links that
    //   contain the org ID so it could be stripped from the URL and stored off for use here. We
    //   could also allow for passing it in on the query string, though that would probably not be
    //   widely used as it would require sending out a link explicitly to the login page and you
    //   might as well just send out a link to a soundbite or feed which would contain the org ID
    //   anyway.

    if (!Utils.isNullOrEmpty(BrowserStorageService.lastSelectedOrg)) {
      return BrowserStorageService.lastSelectedOrg;
    }

    return null;
  }

  /**
   * Initializes the component on first use.
   */
  async function initialize() {
    // Determine whether the user is returning from the redirect process
    if (hasCode) {
      // User is returning from the redirect so we need to make sure we have
      // the OKTA settings available to handle the redirect.
      if (BrowserStorageService.lastOktaSettings) {
        // Apply the settings and handle the redirect
        OktaProvider.applySettings(BrowserStorageService.lastOktaSettings);
        await OktaProvider.handleAuthRedirect();
      } else {
        // OKTA settings are not available
        setDisplayMode(DisplayMode.NoOktaSettingsOnReturn);
      }
    } else {
      let userInteractive: boolean = true;
      const orgRoute = getOrgRouteNonInteractive();

      if (!Utils.isNullOrEmpty(orgRoute)) {
        const oktaSettings = await OktaAuthService.getSettingsByRoute(
          orgRoute as string
        );
        if (oktaSettings) {
          BrowserStorageService.lastOktaSettings = oktaSettings;
          BrowserStorageService.lastSelectedOrg = oktaSettings.orgRoute;
          OktaProvider.applySettings(oktaSettings);
          await OktaProvider.loginAsync(false);
          userInteractive = false;
        }
      }

      if (userInteractive) {
        setDisplayMode(DisplayMode.GetOrgId);
      }
    }
  }

  // Calling useEffect with [] as the second parameter ensures one-time execution

  useEffect(() => {
    initialize();
    /* eslint-disable-next-line */
  }, []);

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onEmailChanged(e: React.ChangeEvent<HTMLInputElement>): void {
    setEmail(e.target.value);
    setCanSubmit(Utils.isEmail(e.target.value));
  }

  /**
   * Attempts to acquire OKTA configuration settings based on the user's email address
   */
  async function getOktaConfigByEmail(): Promise<void> {
    if (email) {
      let settings: OrgOktaSettingsWithOrgRoute =
        await OktaAuthService.getSettingsByEmail(email);
      if (settings) {
        BrowserStorageService.lastOktaSettings = settings;
        OktaProvider.applySettings(settings);
        await OktaProvider.loginAsync(false);
      } else {
        setDisplayMode(DisplayMode.NoOktaSettingsOnReturn);
      }
    }
  }

  //////////[ Render Methods ]//////////////////////////////////////////////////////////////////////

  /**
   * Renders a form requiring the user to identify themselves to our system.  We need to know who
   * they are so we can determine their organization and from that acquire the organization specific
   * OKTA settings so they can be used to begin the login process.
   */
  function RenderGetOrgId() {
    return (
      <>
        <div>
          We need to know who you are in order to load the correct OKTA settings
          for your organization. Please enter your email address into the text
          box below and click the submit button to continue logging into
          Soundbite with OKTA...
        </div>
        <br />
        <div>
          <b>Email Address:</b>
        </div>
        <div>
          <input
            name="email"
            key="email"
            type="text"
            defaultValue={email ?? ""}
            style={{ width: "80%", textAlign: "center" }}
            onChange={onEmailChanged}
          />
        </div>
        <br />
        <div>
          <button
            className="btn btn-primary"
            disabled={!canSubmit}
            onClick={getOktaConfigByEmail}
          >
            Submit
          </button>
        </div>
      </>
    );
  }

  /**
   * Renders a spinner with a message that O365 is loading.  Upon successful authentication the user
   * is redirected to another page effectively ending the spinner.
   */
  function RenderLoading() {
    return (
      <>
        <Loader isLoadedWhen={false} message={"Loading..."} />
      </>
    );
  }

  /**
   * Displays a message with options to return to login page.
   * @param msg - message to display
   */
  function ShowErrorWithBackToLogin(msg: string) {
    return (
      <>
        <h3>OKTA Authentication Error</h3>
        <div className="text-muted text-center mt-2 mb-3">{msg}</div>
        <div className="text-muted text-center mt-2 mb-3">
          <Link to="/public/sso">Return to Login Page</Link>
        </div>
      </>
    );
  }

  function RenderNoOktaSettingsOnReturn() {
    return (
      <>
        <div className="text-muted text-center mt-2 mb-3">
          It appears that you have redirected from the OKTA authentication
          process but we cannot locate the OKTA settings associated with your
          organization. Please attempt to login again or contact support if the
          issue persists.
        </div>
        <div className="text-muted text-center mt-2 mb-3">
          <Link to="/public/sso">Return to Login Page</Link>
        </div>
      </>
    );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  function RenderContent() {
    switch (displayMode) {
      case DisplayMode.Loading:
        return RenderLoading();

      case DisplayMode.GetOrgId:
        return RenderGetOrgId();

      case DisplayMode.NoOktaSettingsOnReturn:
        return RenderNoOktaSettingsOnReturn();

      default:
        return ShowErrorWithBackToLogin(
          "You have reached the OKTA OAuth redirect handler page but " +
            "either no authorization code was passed to this page or authentication has otherwise " +
            "failed. Please try to login again. If the issue persists, please contact support."
        );
    }
  }

  return (
    <Public>
      <Card>
        <CardHeader className="rounded">
          <div className="text-muted text-center mt-2 mb-3">
            <img alt="OKTA Logo" src="/img/Logos/Okta.svg" width="100px" />
            <br />
            <br />
            {RenderContent()}
          </div>
        </CardHeader>
      </Card>
    </Public>
  );
});
