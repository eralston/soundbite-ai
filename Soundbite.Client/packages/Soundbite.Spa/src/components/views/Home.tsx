import React, { Fragment, useEffect } from "react";
import { observer } from "mobx-react-lite";
import { NavLink, useHistory } from "react-router-dom";
import { Invite, Utils } from "@soundbite/api";
import { AuthState, ShowWhen, WidgetStore } from "@soundbite/widgets-react";

import Public from "./Public";
import { Row, Col, Button, CardHeader, CardBody, Card } from "reactstrap";
import UserMenu from "../controls/UserMenu";
import InviteBtn from "../controls/InviteBtn";
import SpaStore from "../../store/Spa.Store";
import { SingleSignOn } from "../login/SingleSignOn";

//////////[ Component Properties ]//////////////////////////////////////////////////////////////////

interface IProps {}

//////////[ Component Definition ]//////////////////////////////////////////////////////////////////

export const Home: React.FC<IProps> = observer((props: IProps) => {
  const history = useHistory();

  useEffect(() => {
    if (
      WidgetStore.authState === AuthState.Authorized ||
      WidgetStore.authState === AuthState.OrganizationSelect
    ) {
      WidgetStore.organizations.readMyOrgsAsync();
    }
  }, []);

  const onInviteAsync = async (emails: string[]): Promise<void> => {
    const invites = new Array<Invite>();
    for (const eml of emails) {
      invites.push({ token: eml });
    }
    await WidgetStore.users.inviteUsersAsync(invites);
  };

  /**
   * Retrieves a Soundbite API token for the specified organization.
   * @param orgRoute - route of the organization whose associated token is being sought.
   */
  async function selectOrg(orgRoute: string) {
    // NOTE: when users enter the application and they are not logged in, the Authorize component
    // intercepts the display and renders an appropriate message based on where the user is in the
    // login process.  One of the screens rendered by Authorize is the home control if the user
    // needs to select an organization. If the user is logging into the home page e.g. "/", then
    // the authorize will just re-render the home screen when the user successfully logs in and it
    // is very confusing to display the select organization screen again.This path check ensures
    // the user is forwarded on to the organization they choose in the login process.

    if (
      history.location.pathname === "/" ||
      history.location.pathname.indexOf(orgRoute) < 0
    ) {
      history.push(`/organizations/${orgRoute}`);
    }

    await SpaStore.loginToOrg(orgRoute);
  }

  window.scrollTo(0, 0);

  if (
    WidgetStore.authState === AuthState.Authorized ||
    WidgetStore.authState === AuthState.OrganizationSelect
  ) {
    return (
      <Public>
        <Card className="sb-public-card">
          <CardHeader>
            <Row className="align-items-center">
              <Col>
                <h1 className="mb-0">
                  Welcome,{" "}
                  {Utils.userDisplay(WidgetStore.users.currentUser, true)}
                </h1>
              </Col>
              <Col className="text-right">
                <UserMenu />
              </Col>
            </Row>
          </CardHeader>
          <CardBody>
            <ShowWhen is={WidgetStore.organizations.myOrgs?.length === 0}>
              <Row>
                <Col>
                  <p className="text-muted">
                    You are not a part of any organizations
                  </p>
                  <p>
                    If you would like a free trial of Soundbite, please contact{" "}
                    <a href="mailto:Support@Soundbite.Freshdesk.Com">
                      Support@Soundite.AI
                    </a>{" "}
                    to get started
                  </p>
                </Col>
              </Row>
            </ShowWhen>
            <ShowWhen is={(WidgetStore.organizations.myOrgs?.length || 0) > 0}>
              <Row>
                <Col>
                  {WidgetStore.organizations.myOrgs?.map((org) => (
                    <div
                      key={org.route}
                      className="sb-org-link"
                      onClick={() => selectOrg(org.route)}
                      title={`Go to the ${org.name} organization`}
                    >
                      <div
                        style={{
                          backgroundImage: `url(${
                            org.imageSrc || "/img/logomark_transparent.png"
                          })`,
                        }}
                        className="sb-org-logo"
                      ></div>
                      <span className="nav-link-text align-middle">
                        {org.name}
                      </span>
                    </div>
                  ))}
                </Col>
              </Row>
            </ShowWhen>
            <Row>
              <Col sm="12">
                <ShowWhen
                  is={
                    SpaStore.config.featureFlags.referralEnabled &&
                    (WidgetStore.organizations.myOrgs?.length ?? 0) > 0
                  }
                >
                  <InviteBtn onSubmitAsync={onInviteAsync}>
                    <Button
                      className="btn-icon btn-3 btn-block sb-home-action sb-link-btn"
                      type="button"
                      title="Share"
                    >
                      <span className="btn-inner--icon">
                        <i className="fas fa-share-alt"></i>
                      </span>
                      <span className="btn-inner--text">
                        <Fragment>&nbsp;</Fragment>Share
                      </span>
                    </Button>
                  </InviteBtn>
                </ShowWhen>
              </Col>
            </Row>
          </CardBody>
        </Card>
      </Public>
    );
  } else {
    return <SingleSignOn />;
  }
});
