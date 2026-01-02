/** @jsx jsx */
import { jsx, css, SerializedStyles } from "@emotion/react";
import React, { useEffect, useState } from "react";
import { Button } from "reactstrap";

import { observer } from "mobx-react-lite";
import {
  Logger,
  SessionSummary,
  SessionsService,
  SessionSummaryType,
} from "@soundbite/api";

import { Loader } from "./Loader";
import { ShowWhen } from "./ShowWhen";
import { GlobalTheme } from "../..";
/***************************************************************************************************
 *  Constants / Global Variables
 **************************************************************************************************/

const getStyles = () => {
  const ret = css`
    & > ul:last-child {
      margin-bottom: 0;
    }

    & .sb-participant-block svg {
      margin-right: 0.25rem;
    }

    .sb-pg-expandable:hover {
      background-color: ${GlobalTheme.current.colors.neutrals.n300};
      cursor: pointer;
    }

    .sb-block-list {
      padding-left: 0;
    }

    .sb-block-list:empty {
      display: none;
    }

    .sb-block-list li {
      display: inline-block;
      list-style: none;
      color: ${GlobalTheme.current.colors.neutrals.n800};
      background-color: ${GlobalTheme.current.colors.neutrals.n100};
      border-radius: 4px;
      padding: 2px 6px;
      margin: 2px;
    }

    .sb-block-list li:first-of-type {
      margin-left: 0;
    }

    .sb-transcript-viewer > ul:last-child {
      margin-bottom: 0;
    }

    .sb-transcript-viewer .sb-participant-block i {
      margin-right: 0.25rem;
    }

    .ttitle {
      margin-top: 8px;
    }

    .tsection {
      margin-bottom: 8px;
      margin-top: 8px;
    }
  `;
  return ret;
};

let styles: SerializedStyles | undefined = undefined;

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute?: string;
  summaryType?: SessionSummaryType;
  isExpanded: boolean;
  className?: string;
}

/***************************************************************************************************
 *  State Enum
 **************************************************************************************************/
enum ControlState {
  NotRequested,
  Requested,
  Retrieved,
  Error,
  CannotRequest,
}

/** React component for showing a session's transcript */
export const SummaryViewer: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  const [isExpanded, setIsExpanded] = useState<boolean>(props.isExpanded);
  const [state, setState] = useState<ControlState>(ControlState.NotRequested);
  const [summary, setSummary] = useState<SessionSummary | undefined>(undefined);

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  // Make sure to reset the control
  useEffect(() => {
    setState(ControlState.NotRequested);
    setSummary(undefined);
  }, [props.orgRoute, props.sessionRoute]);

  useEffect(() => {
    setIsExpanded(props.isExpanded);
    if (state != ControlState.Retrieved && props.isExpanded === true) {
      load();
    }
  }, [props.isExpanded]);

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  function title() {
    switch (props.summaryType) {
      case SessionSummaryType.Paragraph:
        return "Summary";
      case SessionSummaryType.Social:
        return "Social";
      case SessionSummaryType.Tweet:
        return "Tweets";
      case SessionSummaryType.Blog:
        return "Blog";
      case SessionSummaryType.Newsletter:
        return "News";
      default:
        return "Summary";
    }
  }

  /**
   * Calls our API to acquire the summary
   */
  async function load() {
    // Make sure we have all necessary information to continue
    if (props.orgRoute && props.sessionRoute) {
      setState(ControlState.Requested);

      // Attempt to get the URL of the transcript
      try {
        let sum = await SessionsService.summaryAsync(
          props.orgRoute,
          props.sessionRoute,
          props.summaryType ?? SessionSummaryType.Paragraph
        );

        if (sum == null || sum.summary == null) {
          setState(ControlState.CannotRequest);
        } else {
          sum.summary = sum.summary?.trim();
          setSummary(sum);
          setState(ControlState.Retrieved);
        }
      } catch (ex: any) {
        var msg = `Failed to retrieve session summary for session '${props.sessionRoute}' in organization ${props.orgRoute}`;
        Logger.LogError(msg, ex);
        setState(ControlState.Error);
        throw new Error(msg);
      }
    } else {
      // We are missing key information so we cannot get the transcript
      var msg = "Failed to retrieve summary because of missing information.";
      Logger.LogError(msg);
      setState(ControlState.CannotRequest);
      throw new Error(msg);
    }
  }

  /**
   * Determines the format/version of the transcript and offloads the processing
   * to the appropriate transcript text renderer.
   */
  function RenderSummary() {
    try {
      if (
        summary != null &&
        summary.summary != null &&
        summary.summary.trim().length > 0
      ) {
        return (
          <div style={{ whiteSpace: "pre-line" }}>
            <div>{summary?.summary}</div>
            <div className="text-muted text-sm mt-1">
              Powered by generative AI. May occasionally generate biased or
              incorrect information with limited knowledge up to the year 2021.
            </div>
          </div>
        );
      } else {
        return <div>Summary not available</div>;
      }
    } catch {
      return <div>Error Displaying Summary</div>;
    }
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  //////////[ Component UI Helper Methods ]/////////////////////////////////////////////////////////

  function renderComponent() {
    if (styles == null) {
      styles = getStyles();
    }

    return (
      <span className={`sb-transcript-viewer ${props.className}`} css={styles}>
        <ShowWhen is={isExpanded && state == ControlState.Requested}>
          <Loader />
        </ShowWhen>
        <ShowWhen is={isExpanded && state == ControlState.Retrieved}>
          {RenderSummary()}
        </ShowWhen>
        <ShowWhen is={isExpanded && state == ControlState.Error}>
          <div>There was an issue loading the summary</div>
        </ShowWhen>
        <ShowWhen is={isExpanded && state == ControlState.CannotRequest}>
          <div>Session is missing information to request the summary</div>
        </ShowWhen>
      </span>
    );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return renderComponent();
});
