import React, { useRef, useState } from "react";
import sanitizeHtml from "sanitize-html";
import {
  SessionDetails,
  NewClip,
  ClipType,
  FileType,
  ParticipantRole,
  ClipHostingType,
} from "@soundbite/api";

import {
  Loader,
  ParticipantSummary,
  RecordAndPlay,
  ShowWhen,
} from "../components";
import { DialogState } from "../enums";
import { WidgetStore } from "../store";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  participantRole: ParticipantRole;
  onRecordChange?: (state: DialogState) => void;
  session?: SessionDetails;
  sessionRoute?: string;
  orgRoute: string;
  showButtons: boolean;
  onClose?: () => void;
  onSave?: () => void;
  onError?: (err: any) => void;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
/**
 * Widget for the standalone recording of content
 * @param props
 */
export const RecordWidget: React.FC<IProps> = (props: IProps) => {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  let [isSaving, setIsSaving] = useState(false);
  let [recorderState, setRecorderState] = useState(DialogState.Empty);
  let recordAndPlay = useRef<RecordAndPlay>(null);
  let [session, setSession] = React.useState<SessionDetails>();

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  React.useEffect(() => {
    async function init(): Promise<void> {
      if (props.session) {
        setSession(props.session);
      } else if (!props.session && props.orgRoute && props.sessionRoute) {
        const result = await WidgetStore.sessions.readSessionDetailsAsync(
          props.orgRoute,
          props.sessionRoute
        );
        if (result) {
          setSession(result);
        }
      }
    }
    init();
  }, [props.session, props.sessionRoute, props.orgRoute]);

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onRecordChange(state: DialogState): void {
    setRecorderState(state);
    if (props.onRecordChange) {
      props.onRecordChange(state);
    }
  }

  async function onSave(): Promise<void> {
    try {
      if (!props.orgRoute) throw new Error("No current organization loaded");
      if (!recordAndPlay.current)
        throw new Error("Cannot upload clip without live recorder");

      setIsSaving(true);

      const file = recordAndPlay.current.file;

      if (!file) throw new Error("Could not load recording as file");
      if (!session) throw new Error("Could not load Soundbite Session");

      const promptRoute = session.prompts[0].route;

      const newClip: NewClip = {
        hostingType: ClipHostingType.AzureStorage,
        clipType: ClipType.Prompt,
        fileType: FileType.Mp3,
        participantRole: props.participantRole,
        stream: file,
        seconds: recordAndPlay.current.duration,
      };

      await WidgetStore.sessions.uploadClipAsync(
        props.orgRoute,
        session.route,
        promptRoute,
        newClip
      );

      if (props.onSave) {
        props.onSave();
      }

      if (props.onClose) {
        props.onClose();
      }
    } catch (err: any) {
      onError(err);
      //TODO: do we need to show something directly in the widget?
    } finally {
      setIsSaving(false);
    }
  }

  function isInvalid(): boolean {
    if (!session) return true;
    if (recorderState !== DialogState.Ready) return true;
    return false;
  }

  function onError(err: any): void {
    if (props.onError) {
      props.onError(err);
    }
  }

  function onClose(): void {
    if (props.onClose) {
      props.onClose();
    }
  }

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  const agendaText = session?.prompts[0].text ?? "";
  const cleanPromptContext = sanitizeHtml(agendaText);
  let errors: string[] = [];

  if (!props.orgRoute) {
    errors.push("Organization route not provided");
  }

  if (!props.session && !props.sessionRoute) {
    errors.push("Session or session route was not provided.");
  }

  if (errors.length > 0) {
    return (
      <React.Fragment>
        <div>Record Widget cannot display due to the following:</div>
        <ul>
          {errors.map((error) => {
            return <li>{error}</li>;
          })}
        </ul>
      </React.Fragment>
    );
  } else {
    return (
      <Loader
        isLoadedWhen={session !== undefined}
        style={{ minHeight: "12rem" }}
        message="Loading Recorder..."
      >
        <div className="sb-record-widget">
          <div>
            <RecordAndPlay ref={recordAndPlay} onChange={onRecordChange} />
            <ShowWhen is={(agendaText?.length ?? 0) > 0}>
              <h3 className="mb-0 mt-3">Call to Action</h3>
              <div dangerouslySetInnerHTML={{ __html: cleanPromptContext }} />
            </ShowWhen>
            <ParticipantSummary
              className="mt-2"
              session={session}
              hideAudienceRoleWarning={true}
            />
          </div>
          <ShowWhen is={props.showButtons}>
            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-outline-secondary"
                onClick={onClose}
                title="Close"
              >
                Close
              </button>{" "}
              <button
                type="button"
                className="btn btn-primary"
                onClick={onSave}
                disabled={isInvalid() || isSaving}
                title="Save Session Recording"
              >
                Save
              </button>
            </div>
          </ShowWhen>
        </div>
      </Loader>
    );
  }
};
