import React from "react";
import { observer } from "mobx-react-lite";
import { useHistory } from "react-router-dom";

import Public from "./Public";
import { Row, Col, Button, CardHeader, CardBody, Card } from "reactstrap";
import UserMenu from "../controls/UserMenu";
import SpaStore from "../../store/Spa.Store";

//////////[ Component Properties ]//////////////////////////////////////////////////////////////////

interface IProps {}

//////////[ Component Definition ]//////////////////////////////////////////////////////////////////

/**
 * Displays an unauthorized message to the user and provides a link back into the application.
 */
export default observer((props: IProps) => {
  window.scrollTo(0, 0);
  var history = useHistory();

  /**
   * Returns the user to a part of the application to which they should have access.  If the user
   * has successfully selected an organization previously, then go back to the default page in that
   * organization.  Otherwise, go back to the application home screen.
   */
  function goBack() {
    if (SpaStore.currentOrg) {
      history.push(`/organizations/${SpaStore.currentOrg}`);
    } else {
      history.push("/");
    }
  }

  return (
    <Public>
      <Card className="sb-public-card">
        <CardHeader>
          <Row className="align-items-center">
            <Col>
              <h3 className="mb-0">Not Authorized</h3>
            </Col>
            <Col className="text-right">
              <UserMenu />
            </Col>
          </Row>
        </CardHeader>
        <CardBody>
          <Row>
            <Col>
              You are not authorized to view the resource you requested.
              <br />
              &nbsp;
            </Col>
          </Row>
          <Row>
            <Col>
              <Button
                className="btn-icon btn-3 btn-block"
                type="button"
                color="secondary"
                onClick={goBack}
              >
                <span className="btn-inner--icon">
                  <i className="fas fa-arrow-left"></i>
                </span>
                <span className="btn-inner--text">Return to Soundbite App</span>
              </Button>
            </Col>
          </Row>
        </CardBody>
      </Card>
    </Public>
  );
});
