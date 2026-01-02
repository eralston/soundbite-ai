import React, { useState } from "react";
import {
  Row,
  Col,
  TabContent,
  TabPane,
  NavLink,
  NavItem,
  Nav,
} from "reactstrap";
import classnames from "classnames";
import { SessionType } from "@soundbite/api";

export interface ISessionTypePickerProps {
  defaultType: SessionType;
  onChange: (type: SessionType) => void;
}

const SessionTypePicker = (props: ISessionTypePickerProps) => {
  const [activeTabValue, setActiveTabValue] = useState<SessionType>(
    props.defaultType
  );

  const toggle = (tabValue: SessionType) => {
    if (activeTabValue !== tabValue) {
      props.onChange(tabValue);
      setActiveTabValue(tabValue);
    }
  };

  const createTab = (title: string, value: SessionType) => {
    return (
      <NavItem>
        <NavLink
          role="tab"
          aria-selected={activeTabValue === value}
          className={classnames(
            { active: activeTabValue === value },
            "sb-tab-btn"
          )}
          onClick={() => {
            toggle(value);
          }}
        >
          {title}
        </NavLink>
      </NavItem>
    );
  };

  // TODO: Accessibility - Not all fields have a label
  return (
    <React.Fragment>
      <Nav className="sb-3-tab nav-fill flex-column flex-md-row" role="tablist">
        {createTab("Announcement", SessionType.Announcement)}
        {createTab("Survey", SessionType.Survey)}
        {createTab("Meeting", SessionType.Meeting)}
      </Nav>
      <TabContent activeTab={activeTabValue}>
        <TabPane tabId={SessionType.Announcement}>
          <Row>
            <Col>
              <h2 className="text-muted mt-2">One-to-Many</h2>
              <p className="text-muted">
                Announcement Sessions enable you to broadcast one-way to the
                other participants of the session. They're useful for news and
                kudos.
              </p>
              <img
                className="d-none d-md-inline sb-explainer-image"
                src="/img/Sessions/Announcement.png"
                alt=""
              />
            </Col>
          </Row>
        </TabPane>
        <TabPane tabId={SessionType.Survey}>
          <Row>
            <Col>
              <h2 className="text-muted mt-2">Many-to-One</h2>
              <p className="text-muted">
                Survey Sessions are a way to reach out to your team and gather
                their feedback. They're useful for building alignment and
                simple, instant collaboration.
              </p>
              <img
                className="d-none d-md-inline sb-explainer-image"
                src="/img/Sessions/Survey.png"
                alt=""
              />
            </Col>
          </Row>
        </TabPane>
        <TabPane tabId={SessionType.Meeting}>
          <Row>
            <Col>
              <h2 className="text-muted mt-2">Many-to-Many</h2>
              <p className="text-muted">
                Meeting Sessions enable your team to share their input
                round-robin to the rest of the group. They're useful for status
                and scrum-style updates.
              </p>
              <img
                className="d-none d-md-inline sb-explainer-image"
                src="/img/Sessions/Meeting.png"
                alt=""
              />
            </Col>
          </Row>
        </TabPane>
      </TabContent>
    </React.Fragment>
  );
};

export default SessionTypePicker;
