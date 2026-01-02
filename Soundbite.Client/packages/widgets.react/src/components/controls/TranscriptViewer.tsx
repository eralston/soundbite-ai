/** @jsx jsx */
import { jsx, css, SerializedStyles } from "@emotion/react";
import React, { useEffect, useState } from "react";
import { Button } from "reactstrap";
import { ShowWhen } from "./ShowWhen";
import { GlobalTheme } from "../..";
import { observer } from "mobx-react-lite";
import {
  ClipService,
  Logger,
  SoundbiteApiConfig,
  TranscriptState,
} from "@soundbite/api";
import Control from "react-select/src/components/Control";
import { Loader } from "./Loader";
import moment from "moment";

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
  clipRoute?: string;
  promptRoute?: string;
  isPublic?: boolean;
  isExpanded: boolean;
  className?: string;
  transcriptState?: TranscriptState;
  title?: any;
}

/***************************************************************************************************
 *  State Enum
 **************************************************************************************************/
enum controlState {
  NotRequested,
  Requested,
  Retrieved,
  Error,
  CannotRequest,
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const TranscriptViewer: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  const [isExpanded, setIsExpanded] = useState<boolean>(props.isExpanded);
  const [state, setState] = useState<controlState>(controlState.NotRequested);
  const [transcript, setTranscript] = useState<any>(null);

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  // Make sure to reset the control
  useEffect(() => {
    setState(controlState.NotRequested);
    setTranscript(null);
  }, [
    props.orgRoute,
    props.sessionRoute,
    props.promptRoute,
    props.clipRoute,
    props.isPublic,
  ]);

  useEffect(() => {
    setIsExpanded(props.isExpanded);
    if (state != controlState.Retrieved && props.isExpanded === true) {
      getTranscript();
    }
  }, [props.isExpanded]);

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Calls our API to acquire the URL of the transcript
   */
  async function getTranscript() {
    // Make sure we have all necessary information to continue
    if (props.orgRoute && props.sessionRoute && props.clipRoute) {
      setState(controlState.Requested);
      var transcriptUrl: string | null = null;

      // Attempt to get the URL of the transcript
      try {
        transcriptUrl = await ClipService.getTranscriptUrl(
          props.orgRoute,
          props.sessionRoute,
          props.clipRoute,
          props.isPublic ?? false
        );
      } catch (ex: any) {
        var msg = "Failed to retrieve transcript URL.";
        Logger.LogError(msg, ex);
        setState(controlState.Error);
        throw new Error(msg);
      }

      // Attempt to get the transcript at the URL acquired above
      try {
        var transcriptLocal = await SoundbiteApiConfig.httpAdapter.get<any>(
          transcriptUrl,
          { includeToken: false }
        );
        setTranscript(transcriptLocal);
        setState(controlState.Retrieved);
      } catch (ex: any) {
        var msg = "Failed to retrieve transcript JSON.";
        Logger.LogError(msg, ex);
        setState(controlState.Error);
        throw new Error(msg);
      }
    } else {
      // We are missing key information so we cannot get the transcript
      var msg = "Failed to retrieve transcript because of missing information.";
      Logger.LogError(msg);
      setState(controlState.CannotRequest);
      throw new Error(msg);
    }
  }

  /**
   * Determines the format/version of the transcript and offloads the processing
   * to the appropriate transcript text renderer.
   */
  function RenderTranscript() {
    try {
      if (transcript) {
        // AMS does not have a format parameter so we have to "figure it out" here
        if (
          transcript.format == undefined &&
          transcript.transcript?.length > 0
        ) {
          transcript.format = "AMS";
        }

        switch (transcript.format) {
          case "AMS" /* Azure Media Services (Video) */:
            return RenderAmsTranscriptV1_0_0();
          case "ASTT" /* Azure Speach To Text */:
            return RenderAzureTranscriptV1_0_0(transcript.version);
          default:
            TranscriptNotAvailable();
        }
      } else {
        return TranscriptNotAvailable();
      }
    } catch {
      return <div>Error Displaying Transcript</div>;
    }
  }

  function RenderAmsTranscriptV1_0_0() {
    ///////////////////////////////////////////////////////////////////////////////
    // NOTE: This code should be kept in sync with the TranscriptionResult class //
    //       code in the Masticore.Transcription project                         //
    //////////////////////////////////////////////////////////////////////////////

    const amsGapThresholdInMs = 500;

    let sections: string[] = [""];
    let isNewSectionNeeded = false;
    let last = moment.duration("00:00:00");
    let currentSection = 0;

    // Iterate over all of the transcript parts
    (transcript.transcript as any[]).forEach((part) => {
      // Determine whether this is a new section
      let instance = part.instances[0];
      if (instance) {
        const start = moment.duration(instance.start);
        isNewSectionNeeded =
          start.subtract(last).asMilliseconds() > amsGapThresholdInMs;
        last = moment.duration(instance.end);
      }

      // Append the new section if needed
      if (isNewSectionNeeded) {
        // Only append a new section if there is content in the current section
        if (sections[currentSection]) {
          currentSection++;
          sections.push("");
        }
      }

      // Make sure we have text with which to work
      if (part.text) {
        if (sections[currentSection].length > 0) {
          sections[currentSection] += " ";
        }
        sections[currentSection] += part.text;
      }
    });

    return (
      <React.Fragment>
        {!!props.title ? props.title : null}
        {sections.map((section, index) => (
          <div className="tsection" id={`tsec${index}`}>
            <p>{section}</p>
          </div>
        ))}
      </React.Fragment>
    );
  }

  /**
   * Renders the transcript using the Azure Speech To Text (v1.0.0) format.
   */
  function RenderAzureTranscriptV1_0_0(version: string) {
    return (
      <React.Fragment>
        {!!props.title ? props.title : null}
        {(transcript.sections as []).map((section, index) => (
          <div className="tsection" id={`tsec${index}`}>
            <p>{(section as any).text}</p>
          </div>
        ))}
      </React.Fragment>
    );
  }

  function TranscriptNotAvailable() {
    return <div>Transcript not available.</div>;
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  //////////[ Component UI Helper Methods ]/////////////////////////////////////////////////////////

  function renderComponent() {
    if (styles == null) {
      styles = getStyles();
    }

    return (
      <span className={`sb-transcript-viewer ${props.className}`} css={styles}>
        <ShowWhen is={isExpanded && state == controlState.Requested}>
          <Loader />
        </ShowWhen>
        <ShowWhen is={isExpanded && state == controlState.Retrieved}>
          {RenderTranscript()}
        </ShowWhen>
        <ShowWhen is={isExpanded && state == controlState.Error}>
          There was an issue loading the transcript.
        </ShowWhen>
        <ShowWhen is={isExpanded && state == controlState.CannotRequest}>
          Clip is missing information required to request the transcript.
        </ShowWhen>
      </span>
    );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return renderComponent();
});
