import React, { useEffect, useState } from "react";
import { observer } from "mobx-react-lite";
import sanitizeHtml from "sanitize-html";
import {
  SessionDetails,
  SessionsService,
  TranscriptState,
  Utils,
} from "@soundbite/api";
import { ShowWhen, SoloPlayer } from "../components";
import { TranscriptViewer } from "../components/controls/TranscriptViewer";
import { Button } from "reactstrap";

/***************************************************************************************************
 *  Component Interfaces
 **************************************************************************************************/

export interface IPublicPlayerWidgetProps {
  allowDownload?: boolean;
  showCallToAction?: boolean;
  showTitle?: boolean;
  showSubTitle?: boolean;
  title?: string;
  subTitle?: string;
  callToActionTitle?: string;
  callToActionText?: string;
  hideOnError?: boolean;
  orgRoute: string;
  sessionRoute: string;
  session?: SessionDetails;
  userRoute?: string;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const PublicPlayerWidget: React.FC<IPublicPlayerWidgetProps> = observer(
  (props: IPublicPlayerWidgetProps) => {
    const [isLoading, setIsLoading] = useState(true);
    const [hasError, setHasError] = useState(false);
    const [errorMsg, setErrorMsg] = useState("");
    const [showTranscript, setShowTranscript] = React.useState<boolean>(false);
    const [session, setSession] = useState<SessionDetails | undefined>(
      undefined
    );
    const isValid = props.orgRoute && props.sessionRoute;

    /**
     * Runs the first time the play button is clicked after the widget loads.
     */
    function onFirstPlay() {
      if (props.userRoute) {
        SessionsService.acknowledgePublicSession(
          props.orgRoute,
          props.sessionRoute,
          props.userRoute
        );
      }
    }

    async function init(): Promise<void> {
      // Determine whether the configuration is valid and define error messages if not
      if (isValid) {
        try {
          const session: SessionDetails = await SessionsService.readPublicAsync(
            props.orgRoute,
            props.sessionRoute
          );
          if (session) {
            setSession(session);
            setHasError(false);
            setErrorMsg("");
            setIsLoading(false);
          } else {
            setHasError(true);
            setErrorMsg("Soundbite clip not found.");
            setIsLoading(false);
          }
        } catch (ex) {
          setHasError(true);
          setErrorMsg(
            "An error occurred trying to retrieve the Soundbite clip.  See logs for additional error details."
          );
        }
      } else {
        setHasError(true);
        if (!props.orgRoute && !props.sessionRoute) {
          setErrorMsg(
            "Public Player Widget requires organization route and session route to be set."
          );
        } else if (!props.orgRoute) {
          setErrorMsg(
            "Public Player Widget requires organization route to be set."
          );
        } else {
          setErrorMsg("Public Player Widget requires session route to be set.");
        }
      }
    }

    useEffect(() => {
      init();
    }, [props.orgRoute, props.sessionRoute]);

    function Agenda() {
      const promptText = session?.prompts[0].text?.trim() ?? "";
      if (props.showCallToAction !== false && promptText.length > 0) {
        return (
          <>
            <h3 className="mb-0 mt-3">
              {props.callToActionTitle ?? "Call to Action"}
            </h3>
            <p
              dangerouslySetInnerHTML={{
                __html: props.callToActionText ?? sanitizeHtml(promptText),
              }}
            />
          </>
        );
      } else {
        return <></>;
      }
    }

    function Title() {
      if (props.showTitle !== false) {
        return (
          <div className="bg-gradient-primary modal-header">
            <h5 className="modal-title">
              {props.title ?? session?.name}
              <SubTitle />
            </h5>
          </div>
        );
      } else {
        return <></>;
      }
    }

    function SubTitle() {
      if (props.showSubTitle !== false) {
        return (
          <span className="sb-modal-header-subtitle">
            {props.subTitle ?? Utils.formatRelativeDate(session?.publish)}
          </span>
        );
      } else {
        return <></>;
      }
    }

    function Render(): JSX.Element {
      if (hasError) {
        return RenderError();
      } else {
        if (isLoading === true) {
          return RenderLoading();
        } else {
          return RenderPlayer();
        }
      }
    }

    function RenderLoading() {
      return <>Loading...</>;
    }

    function RenderError(): JSX.Element {
      if (props.hideOnError === true) {
        return <></>;
      } else {
        // Default error display
        return <div>{errorMsg}</div>;
      }
    }

    function getTranscriptButtonMsg() {
      const state: TranscriptState =
        session?.prompts[0]?.clips[0]?.transcriptState ?? TranscriptState.None;
      if (showTranscript) {
        return "Hide Transcript";
      } else {
        switch (state) {
          case TranscriptState.Available:
            return "Show Transcript";
          case TranscriptState.Requested:
            return "Transcript Processing";
          default:
            return "Transcript Error";
        }
      }
    }

    function isTranscriptBtnVisible(): boolean {
      const state: TranscriptState =
        session?.prompts[0]?.clips[0]?.transcriptState ?? TranscriptState.None;
      return state != TranscriptState.None;
    }

    function isTranscriptBtnEnabled() {
      const state: TranscriptState =
        session?.prompts[0]?.clips[0]?.transcriptState ?? TranscriptState.None;
      return state == TranscriptState.Available;
    }

    async function onShowTranscript() {
      setShowTranscript(!showTranscript);
    }

    function TranscriptButton() {
      return (
        <Button
          disabled={!isTranscriptBtnEnabled()}
          onClick={onShowTranscript}
          color="secondary"
          size="sm"
          type="button"
          outline={true}
          title={getTranscriptButtonMsg()}
        >
          {getTranscriptButtonMsg()}
        </Button>
      );
    }

    function RenderPlayer(): JSX.Element {
      return (
        <div>
          <Title />
          <div className="modal-body">
            <SoloPlayer
              allowDownload={props.allowDownload}
              session={session}
              onFirstPlay={onFirstPlay}
            />
            <Agenda />

            <ShowWhen is={!!session}>
              <ShowWhen is={isTranscriptBtnVisible() && !showTranscript}>
                <TranscriptButton />
              </ShowWhen>

              <TranscriptViewer
                title={
                  <h3 className="mb-0 mt-3">
                    Transcript &nbsp;
                    <TranscriptButton />
                  </h3>
                }
                orgRoute={props.orgRoute}
                sessionRoute={session?.route}
                promptRoute={session?.prompts[0]?.route}
                clipRoute={session?.prompts[0]?.clips[0]?.route}
                isExpanded={showTranscript}
                isPublic={true}
                transcriptState={
                  props.session?.prompts[0].clips[0].transcriptState
                }
              />
            </ShowWhen>
          </div>
        </div>
      );
    }

    return Render();
  }
);
