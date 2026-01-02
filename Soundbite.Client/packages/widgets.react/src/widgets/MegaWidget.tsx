import React, { useState } from "react";
import { Container, Nav, Navbar, NavItem } from "reactstrap";
import { Utils } from "@soundbite/api";

import { FeedMode, FeedWidget } from "./FeedWidget";
import { IconLink, Loader } from "../components";
import { PastSessionsWidget } from "./PastSessionsWidget";
import { PendingSessionsWidget } from "./PendingSessionsWidget";
import { SeriesWidget } from "./SeriesWidget";
import { IconProp } from "@fortawesome/fontawesome-svg-core";
import {
  faCalendar,
  faCalendarCheck,
  faCalendarDay,
  faPodcast,
} from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  title?: string;
  showTitle?: boolean;
  orgRoute: string;
  groupRoute?: string;
  initialTab?: string;
}

enum TabName {
  Feed = "Feed",
  Calendar = "Calendar",
  Scheduled = "Scheduled",
  History = "History",
}

interface ITab {
  name: TabName;
  icon: IconProp;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
/**
 * Widget for constructing a session, including recording/uploading conte and optionally recurring sessions
 * @param props
 */
export const MegaWidget: React.FC<IProps> = (props: IProps) => {
  //////////[ Types ]///////////////////////////////////////////////////////////////////////////////

  const tabs: ITab[] = [
    { name: TabName.Feed, icon: faPodcast },
    { name: TabName.Calendar, icon: faCalendar },
    { name: TabName.Scheduled, icon: faCalendarDay },
    { name: TabName.History, icon: faCalendarCheck },
  ];

  const [currentTab, setCurrentTab] = useState<string>(
    getInitialTab(props.initialTab)
  );

  /**
   * Ensures that the initial tab value is valid.  Essentially if it does not match
   * @param initialTabValue
   */
  function getInitialTab(initialTabValue?: string): string {
    let result: string = tabs[0].name;
    initialTabValue = initialTabValue?.trim().toLowerCase();
    for (var i = 0; i < tabs.length; i++) {
      if (tabs[i].name.toLowerCase() == initialTabValue) {
        result = tabs[i].name;
      }
    }
    return result;
  }

  /**
   * Header is a copy of the header from the SPA application.
   */
  function Header() {
    return (
      <div className="header sb-header bg-gradient-brand pb-4 pt-6 pt-md-8">
        <div className="separator separator-bottom separator-skew zindex-100">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            preserveAspectRatio="none"
            version="1.1"
            viewBox="0 0 2560 100"
            x="0"
            y="0"
          >
            <polygon
              className="fill-default"
              points="2560 0 2560 100 0 100"
            ></polygon>
          </svg>
        </div>
      </div>
    );
  }

  /**
   * Renders the navigation tabs allowing users to change the current view.
   */
  function Tabs() {
    return (
      <Navbar className="navbar-top navbar-dark bg-gradient-brand" expand="md">
        <Container>
          <Nav className="navbar-nav align-items-center">
            <ul className="sb-tabs">
              {tabs.map((tab) => (
                <NavItem>
                  <a
                    href="#"
                    onClick={(e) => {
                      e.preventDefault();
                      setCurrentTab(tab.name);
                    }}
                    className={currentTab == tab.name ? "active" : ""}
                  >
                    <IconLink title={tab.name} icon={tab.icon} />
                  </a>
                </NavItem>
              ))}
            </ul>
          </Nav>
        </Container>
      </Navbar>
    );
  }

  function getTitle(suffix?: string): string | undefined {
    if (Utils.isNullOrEmpty(props.title)) {
      return undefined;
    } else {
      return props.title + (suffix ?? "");
    }
  }

  function Content() {
    if (Utils.isNullOrEmpty(props.orgRoute)) {
      return <Loader />;
    } else {
      switch (currentTab) {
        case TabName.Feed:
          return (
            <FeedWidget
              orgRoute={props.orgRoute}
              groupRoute={props.groupRoute}
              title={getTitle()}
              showTitle={props.showTitle}
              mode={FeedMode.Card}
            />
          );
        case TabName.Calendar:
          return (
            <SeriesWidget
              orgRoute={props.orgRoute}
              groupRoute={props.groupRoute}
              title={getTitle(" Series")}
              showTitle={props.showTitle}
            />
          );
        case TabName.Scheduled:
          return (
            <PendingSessionsWidget
              orgRoute={props.orgRoute}
              groupRoute={props.groupRoute}
              title={getTitle(" History")}
              showTitle={props.showTitle}
            />
          );
        case TabName.History:
          return (
            <PastSessionsWidget
              orgRoute={props.orgRoute}
              groupRoute={props.groupRoute}
              title={getTitle(" History")}
              showTitle={props.showTitle}
            />
          );
        default:
          return <div>No UI for {currentTab}</div>;
      }
    }
  }

  return (
    <div className="sb-private-view">
      <div className="main-content">
        <Tabs />
        <Header />
        <Container className="sb-navigation-container">
          <Content />
        </Container>
      </div>
    </div>
  );
};
