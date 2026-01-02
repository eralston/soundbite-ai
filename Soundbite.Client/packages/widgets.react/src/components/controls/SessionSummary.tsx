/** @jsx jsx */
import { jsx, css, SerializedStyles } from "@emotion/react";
import React, { useEffect, useState } from "react";
import classnames from "classnames";

import { observer } from "mobx-react-lite";
import { SessionSummaryType } from "@soundbite/api";
import Control from "react-select/src/components/Control";
import { Loader } from "./Loader";
import { Nav, NavItem, NavLink, TabContent, TabPane } from "reactstrap";
import { SummaryViewer } from "./SummaryViewer";

/***************************************************************************************************
 *  Constants / Global Variables
 **************************************************************************************************/

const getStyles = () => {
  const ret = css``;
  return ret;
};

let styles: SerializedStyles | undefined = undefined;

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute?: string;
  className?: string;
  title?: any;
}

/** React component for showing a session's transcript */
export const SessionSummary: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  const [currentType, setCurrentType] = useState<
    SessionSummaryType | undefined
  >(undefined);

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onToggleTab(tabValue: SessionSummaryType) {
    if (currentType !== tabValue) {
      setCurrentType(tabValue);
    }
  }

  //////////[ Component UI Helper Methods ]/////////////////////////////////////////////////////////

  function createTab(title: string, value: SessionSummaryType) {
    const className = classnames(
      { active: currentType === value },
      "sb-tab-btn"
    );

    return (
      <NavItem>
        <NavLink
          role="tab"
          aria-selected={currentType === value}
          className={className}
          onClick={() => {
            onToggleTab(value);
          }}
          title={title}
        >
          {title}
        </NavLink>
      </NavItem>
    );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  if (styles == null) {
    styles = getStyles();
  }

  return (
    <div className={props.className} css={styles}>
      <div className="form-control-label ttitle">(Beta) Wizard</div>
      <Nav className="sb-3-tab nav-fill flex-column flex-md-row" role="tablist">
        {createTab("Summary", SessionSummaryType.Paragraph)}
        {createTab("Social", SessionSummaryType.Social)}
        {createTab("Tweets", SessionSummaryType.Tweet)}
        {createTab("Blog", SessionSummaryType.Blog)}
        {createTab("News", SessionSummaryType.Newsletter)}
      </Nav>
      <TabContent activeTab={currentType}>
        <TabPane tabId={SessionSummaryType.Paragraph}>
          <SummaryViewer
            orgRoute={props.orgRoute}
            sessionRoute={props.sessionRoute}
            summaryType={currentType}
            isExpanded={true}
          />
        </TabPane>
        <TabPane tabId={SessionSummaryType.Social}>
          <SummaryViewer
            orgRoute={props.orgRoute}
            sessionRoute={props.sessionRoute}
            summaryType={SessionSummaryType.Social}
            isExpanded={true}
          />
        </TabPane>
        <TabPane tabId={SessionSummaryType.Tweet}>
          <SummaryViewer
            orgRoute={props.orgRoute}
            sessionRoute={props.sessionRoute}
            summaryType={SessionSummaryType.Tweet}
            isExpanded={true}
          />
        </TabPane>

        <TabPane tabId={SessionSummaryType.Blog}>
          <SummaryViewer
            orgRoute={props.orgRoute}
            sessionRoute={props.sessionRoute}
            summaryType={SessionSummaryType.Blog}
            isExpanded={true}
          />
        </TabPane>
        <TabPane tabId={SessionSummaryType.Newsletter}>
          <SummaryViewer
            orgRoute={props.orgRoute}
            sessionRoute={props.sessionRoute}
            summaryType={SessionSummaryType.Newsletter}
            isExpanded={true}
          />
        </TabPane>
      </TabContent>
    </div>
  );
});
