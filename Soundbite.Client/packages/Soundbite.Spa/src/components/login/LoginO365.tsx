import React, { useEffect, useState } from "react";
import { CardHeader, Card } from "reactstrap";
import { Link } from "react-router-dom";
import { observer } from "mobx-react-lite";
import { Logger } from "@soundbite/api";
import { AuthState, Loader, WidgetStore } from "@soundbite/widgets-react";

import Public from "../views/Public";
import { AadAuthProvider } from "../../sdk/providers/auth/AadAuthProvider";

//////////[ Component Definition ]//////////////////////////////////////////////////////////////////

/**
 * Responsible for handling the O365 OAuth redirect that occurs after a user successfully
 * authenticates against Azure AD / Microsoft O365.  An authorization code is sent on the query
 * string that can be used to acquire access tokens for Soundbite.  The "code" in the URL is
 * initially present when the page first loads, and then disappears after consumption by MSAL.
 */
export const LoginO365: React.FC = observer(() => {
  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  let [authProvider] = useState<AadAuthProvider>(new AadAuthProvider());
  let [hasCode] = useState<boolean>(
    window.location.hash?.split("code=").length > 1
  );
  let [redirectHandled, setRedirectHandled] = useState<boolean>(false);

  async function initialize() {
    if (hasCode) {
      Logger.LogTrace("LoginO365 - Initializing with Auth Code Present");
      if (!redirectHandled) {
        Logger.LogTrace("LoginO365 - Handling Redirect");
        WidgetStore.authState = AuthState.ThirdPartyAuthorizing;
        setRedirectHandled(true);
        authProvider.handleAuthRedirect();
      }
    } else {
      Logger.LogTrace("LoginO365 - Auth Code not Present");
    }
  }

  // Calling useEffect with [] as the second parameter ensures one-time execution

  useEffect(() => {
    initialize();
    /* eslint-disable-next-line */
  }, []);

  //////////[ Render Methods ]//////////////////////////////////////////////////////////////////////

  /**
   * Renders a spinner with a message that O365 is loading.  Upon successful authentication the usr
   * is redirected to another page effectively ending the spinner.
   */
  function ShowO365LoginLoading() {
    return (
      <Public>
        <Card>
          <CardHeader className="rounded">
            <div className="text-muted text-center mt-2 mb-3">
              <Loader
                isLoadedWhen={false}
                message={"Processing O365 Login..."}
              />
            </div>
          </CardHeader>
        </Card>
      </Public>
    );
  }

  /**
   * Displays a message with options to return to login page.
   * @param msg - message to display
   */
  function ShowErrorWithBackToLogin(msg: string) {
    return (
      <Public>
        <Card>
          <CardHeader className="rounded">
            <div className="text-muted text-center mt-2 mb-3">
              <h3>O365 Authentication Error</h3>
              <div className="text-muted text-center mt-2 mb-3">{msg}</div>
              <div className="text-muted text-center mt-2 mb-3">
                <Link to="/public/sso">Return to Login Page</Link>
              </div>
            </div>
          </CardHeader>
        </Card>
      </Public>
    );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  if (hasCode || WidgetStore.authState === AuthState.ThirdPartyAuthorizing) {
    return ShowO365LoginLoading();
  } else {
    return ShowErrorWithBackToLogin(
      "You have reached the O365 OAuth redirect handler page but " +
        "either no authorization code was passed to this page or authentication has otherwise " +
        "failed. Please try to login again. If the issue persists, please contact support."
    );
  }
});
