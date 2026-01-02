import React from "react";
import { observer } from "mobx-react-lite";

import {
  ClipType,
  ClipWithContributor,
  ParticipantState,
  PersonRole,
  SessionDetails,
  SessionSecurityType,
} from "@soundbite/api";

import { Loader, ShowWhen, SoloPlayer, GroupPlayer } from "../components";
import { WidgetStore } from "../store";
import { IMediaPlayerContext } from "../components/video/context/IMediaPlayerContext";
import { SessionDetailWidget } from "../components/SessionDetailWidget";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute?: string;
  session?: SessionDetails;
  showButtons: boolean;
  currentUserRoute?: string;
  onClose?: () => void;
  onError?: (error: any) => boolean;
  autoPlay?: boolean;
  onContext?: (context: IMediaPlayerContext) => void;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const PlayerWidget: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  let [isSolo, setIsSolo] = React.useState(true);
  let [session, setSession] = React.useState<SessionDetails>();
  let [isMediaError, setIsMediaError] = React.useState<boolean>(false);

  const isAdminOrMore = WidgetStore.isPersonRole(PersonRole.Admin);

  React.useEffect(() => {
    async function init(): Promise<void> {
      // Attempt to use the sessions passed into the component
      let currentSession: SessionDetails | undefined = props.session;

      // Determine if there is a valid currentSession
      if (!currentSession) {
        // No currentSession so see if required props to load a currentSession are present
        if (props.orgRoute && props.sessionRoute) {
          // Attempt to load the currentSession
          currentSession = await WidgetStore.sessions.readSessionDetailsAsync(
            props.orgRoute,
            props.sessionRoute
          );
        }
      }

      // Determine if there is a valid currentSession loaded
      if (currentSession) {
        setSession(props.session);

        // Not all clips are valid for display
        setIsSolo(getValidClips(currentSession).length === 1);
      } else {
        // No currentSession passed in and no currentSession could be loaded
        // TODO: do we need to do something here???
      }
    }
    init();
  }, []);

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Gets an array containing only those clips in the session which are valid for consumption.
   */
  function getValidClips(session?: SessionDetails): ClipWithContributor[] {
    return (
      session?.prompts[0]?.clips?.filter(
        (i) =>
          i.clipType != ClipType.RawClip && i.clipType != ClipType.RawTranscript
      ) ?? []
    );
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onClose() {
    if (props.onClose) {
      props.onClose();
    }
  }

  async function onAcknowledge() {
    if (props.orgRoute && session) {
      const remove = session.sessionSecurity !== SessionSecurityType.Public;
      await WidgetStore.sessions.acknowledgeSessionAsync(
        props.orgRoute,
        session.route,
        session,
        remove
      );
      onClose();
    }
  }

  function onMediaError(error: Error) {
    setIsMediaError(true);
  }

  const onContext = (context: IMediaPlayerContext) => {
    if (props.onContext != null) {
      props.onContext(context);
    }
  };

  // Determine whether the user has a participant role making them eligible to acknowledge the soundbite
  const canAcknowledge = session?.myParticipation?.some(
    (i) =>
      i.person.user.route === WidgetStore.users.currentUser?.route &&
      i.participantState === ParticipantState.ConsumptionRequested
  );

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
        <div>Player Widget cannot display due to the following:</div>
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
        message="Loading Player..."
      >
        <div>
          <div>
            {isSolo ? (
              <SoloPlayer
                session={session}
                allowDownload={isAdminOrMore}
                onMediaError={onMediaError}
                autoPlay={props.autoPlay}
                onContext={onContext}
              />
            ) : (
              <GroupPlayer
                session={session}
                currentUserRoute={props.currentUserRoute}
              />
            )}
            <SessionDetailWidget
              orgRoute={props.orgRoute}
              sessionRoute={props.sessionRoute}
              session={session}
              currentUserRoute={props.currentUserRoute}
            />
            <ShowWhen is={props.showButtons}>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-outline-secondary sb-close-btn"
                  onClick={onClose}
                  title="Close"
                >
                  Close
                </button>
                <ShowWhen is={canAcknowledge && !isMediaError}>
                  <button
                    type="button"
                    className="btn btn-primary sb-acknowledge-btn"
                    onClick={onAcknowledge}
                    title="Acknowledge Session"
                  >
                    Acknowledge
                  </button>
                </ShowWhen>
              </div>
            </ShowWhen>
          </div>
        </div>
      </Loader>
    );
  }
});
