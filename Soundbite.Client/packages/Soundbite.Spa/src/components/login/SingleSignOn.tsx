import React, { Fragment, useState } from "react";
import { Link } from "react-router-dom";
import { CardHeader, Card } from "reactstrap";
import { observer } from "mobx-react-lite";
import { ProviderType } from "@soundbite/api";
import { AuthState, Loader, WidgetStore } from "@soundbite/widgets-react";
import Public from "../views/Public";
import { IAuthProvider } from "../../sdk/providers/auth/IAuthProvider";
import { getAadAuthProvider } from "../../sdk/providers/auth/AadAuthProvider";

//////////[ Component Props ]///////////////////////////////////////////////////////////////////////
interface IProps {
  message?: string;
}

//////////[ Component Definition ]//////////////////////////////////////////////////////////////////

export const SingleSignOn: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  const [isClicked, setIsClicked] = useState(false);

  async function onClickLogin(providerType: ProviderType) {
    switch (providerType) {
      case ProviderType.AAD:
        const provider: IAuthProvider = getAadAuthProvider();
        provider.loginAsync(false);
        setIsClicked(true);
        break;
    }
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  const isLoading = WidgetStore.authState === AuthState.Authorizing;

  return (
    <Public>
      <Card>
        <CardHeader>
          <div className="text-muted text-center mt-2 mb-3">
            <small>{props.message || "Single Sign-On (SSO)"}</small>
          </div>
          <div className="btn-wrapper text-center py-1">
            <Loader isLoadedWhen={!isClicked}>
              <button
                className="btn-neutral btn-icon btn btn-default btn-lg"
                onClick={() => onClickLogin(ProviderType.AAD)}
                disabled={isLoading}
                title="Sign In to Microsoft 365"
                style={{
                  width: "160px",
                  display: "block",
                  clear: "both",
                  margin: "15px auto auto auto",
                }}
              >
                <span className="btn-inner--icon">
                  <img
                    alt="Microsoft Office Logo"
                    src="/img/Microsoft/Office.svg"
                  />
                </span>
                <span className="btn-inner--text">
                  <Fragment>&nbsp;</Fragment>Microsoft 365
                </span>
              </button>
              <Link to="/public/sso/loginOkta">
                <button
                  className="btn-neutral btn-icon btn btn-default btn-lg"
                  disabled={isLoading}
                  title="Sign In to OKTA"
                  style={{
                    width: "160px",
                    display: "block",
                    clear: "both",
                    margin: "15px auto auto auto",
                  }}
                >
                  <img alt="OKTA Logo" src="/img/logos/okta.svg" width="50px" />
                </button>
              </Link>
            </Loader>
          </div>
        </CardHeader>
      </Card>
    </Public>
  );
});
