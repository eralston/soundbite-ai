import React, { useEffect } from "react";
import sanitizeHtml from "sanitize-html";
import { observer } from "mobx-react-lite";

import {
  ClipType,
  ClipWithContributor,
  Participant,
  ParticipantRole,
  PersonRole,
  SessionCommentPolicy,
  SessionDetails,
  SessionSecurityType,
  TranscriptState,
} from "@soundbite/api";

import { ParticipantSummary, ShowWhen } from "../components";
import { WidgetStore } from "../store";
import { TranscriptViewer } from "../components/controls/TranscriptViewer";
import { Button } from "reactstrap";
import { SessionSummary } from "../components/controls/SessionSummary";
import { SessionReactions } from "../components/SessionReactions";
import { SessionComments } from "../components/controls/SessionComments";

interface IDetailsProps {
  orgRoute?: string;
  sessionRoute?: string;
  session?: SessionDetails;
  currentUserRoute?: string;
}

export const SessionDetailWidget: React.FC<IDetailsProps> = observer(
  (props: IDetailsProps) => {
    let [cleanPromptContext, setCleanPromptContext] =
      React.useState<string>("");
    let [showTranscript, setShowTranscript] = React.useState<boolean>(false);
    let [showWizard, setShowWizard] = React.useState<boolean>(false);
    let [showComments, setShowComments] = React.useState<boolean>(false);
    let [showAudience, setShowAudience] = React.useState<boolean>(false);

    const isAdminOrMore = WidgetStore.isPersonRole(PersonRole.Admin);
    const isPublic =
      props.session?.sessionSecurity === SessionSecurityType.Public;

    const isCommentsEnabled =
      (WidgetStore.organizations.currentOrg?.settings.sessions
        .sessionCommentsEnabled ??
        true) &&
      (props.session?.sessionCommentPolicy ?? SessionCommentPolicy.Allowed) ===
        SessionCommentPolicy.Allowed;

    useEffect(() => {
      setCleanPromptContext(sanitizeHtml(props.session?.prompts[0].text || ""));
    }, []);

    function getTranscriptButtonMsg() {
      const validClips: ClipWithContributor[] = getValidClips(props.session);
      const state: TranscriptState =
        validClips[0]?.transcriptState ?? TranscriptState.None;
      if (showTranscript) {
        return "Transcript";
      } else {
        switch (state) {
          case TranscriptState.Available:
            return "Transcript";
          case TranscriptState.Requested:
            return "Transcript Processing";
          default:
            return "Transcript Error";
        }
      }
    }

    function getWizardBtnMsg() {
      const validClips: ClipWithContributor[] = getValidClips(props.session);
      const state: TranscriptState =
        validClips[0]?.transcriptState ?? TranscriptState.None;
      if (showWizard) {
        return "Wizard";
      } else {
        switch (state) {
          case TranscriptState.Available:
            return "Wizard";
          case TranscriptState.Requested:
            return "Wizard Processing";
          default:
            return "Wizard Error";
        }
      }
    }

    function isTranscriptBtnVisible(): boolean {
      const validClips: ClipWithContributor[] = getValidClips(props.session);
      const state: TranscriptState =
        validClips[0]?.transcriptState ?? TranscriptState.None;
      return state != TranscriptState.None;
    }

    function isTranscriptBtnEnabled() {
      const validClips: ClipWithContributor[] = getValidClips(props.session);
      const state: TranscriptState =
        validClips[0]?.transcriptState ?? TranscriptState.None;
      return state == TranscriptState.Available;
    }

    function onShowTranscript() {
      const isShow = !showTranscript;
      setShowTranscript(isShow);

      if (isShow) {
        setShowAudience(false);
        setShowWizard(false);
        setShowComments(false);
      }
    }

    function onShowWizard() {
      const isShow = !showWizard;
      setShowWizard(isShow);

      if (isShow) {
        setShowAudience(false);
        setShowTranscript(false);
        setShowComments(false);
      }
    }

    function onShowComments() {
      const isShow = !showComments;
      setShowComments(isShow);

      if (isShow) {
        setShowAudience(false);
        setShowTranscript(false);
        setShowWizard(false);
      }
    }

    function onShowAudienceClick() {
      const isShow = !showAudience;
      setShowAudience(isShow);

      if (isShow) {
        setShowComments(false);
        setShowTranscript(false);
        setShowWizard(false);
      }
    }

    //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

    function iAmHost(session?: SessionDetails): boolean {
      if (session?.participants == null) {
        return false;
      }

      const myHostParticipants = session.myParticipation.filter(
        (p: Participant) =>
          p.participantRole >= ParticipantRole.Host &&
          p.person.user.route === props.currentUserRoute
      );

      return myHostParticipants.length > 0;
    }

    function iAmParticipant(session?: SessionDetails): boolean {
      if (session?.participants == null) {
        return false;
      }

      const myAudienceParticipants = session.myParticipation.filter(
        (p: Participant) =>
          (p.participantRole === ParticipantRole.Audience ||
            p.participantRole === ParticipantRole.Participant) &&
          p.person.user.route === props.currentUserRoute
      );

      return myAudienceParticipants.length > 0;
    }

    /**
     * Gets an array containing only those clips in the session which are valid for consumption.
     */
    function getValidClips(session?: SessionDetails): ClipWithContributor[] {
      return (
        session?.prompts[0]?.clips?.filter(
          (i) =>
            i.clipType != ClipType.RawClip &&
            i.clipType != ClipType.RawTranscript
        ) ?? []
      );
    }

    const isAudienceEnabled = WidgetStore.isPersonRole(
      WidgetStore.organizations.currentOrg?.permissions?.minRoleForAudience ??
        PersonRole.Unknown
    );

    return (
      <div className="sb-cover-modal-body">
        <SessionReactions
          orgRoute={props.orgRoute}
          sessionRoute={props.sessionRoute}
          session={props.session}
          readOnly={!iAmParticipant(props.session)}
        />
        <ShowWhen is={cleanPromptContext.trim().length > 0}>
          <h3 className="mb-0 mt-3">Call to Action</h3>
          <p dangerouslySetInnerHTML={{ __html: cleanPromptContext }} />
        </ShowWhen>

        <ShowWhen is={isTranscriptBtnVisible()}>
          <Button
            disabled={!isTranscriptBtnEnabled()}
            onClick={onShowTranscript}
            color="secondary"
            size="sm"
            type="button"
            outline={!showTranscript}
            title="Transcript"
          >
            {getTranscriptButtonMsg()}
          </Button>
        </ShowWhen>

        {/*<ShowWhen*/}
        {/*  is={*/}
        {/*    isTranscriptBtnEnabled() &&*/}
        {/*    (isAdminOrMore || iAmHost(props.session))*/}
        {/*  }*/}
        {/*>*/}
        {/*  <Button*/}
        {/*    onClick={onShowWizard}*/}
        {/*    color="secondary"*/}
        {/*    size="sm"*/}
        {/*    type="button"*/}
        {/*    outline={!showWizard}*/}
        {/*    title="Wizard"*/}
        {/*  >*/}
        {/*    {getWizardBtnMsg()}*/}
        {/*  </Button>*/}
        {/*</ShowWhen>*/}

        <ShowWhen is={isAudienceEnabled}>
          <Button
            onClick={onShowAudienceClick}
            color="secondary"
            size="sm"
            type="button"
            outline={!showAudience}
            title="Audience"
          >
            Audience
          </Button>
        </ShowWhen>

        <ShowWhen is={isCommentsEnabled}>
          <Button
            onClick={onShowComments}
            color="secondary"
            size="sm"
            type="button"
            outline={!showComments}
            title="Comments"
          >
            Comments
          </Button>
        </ShowWhen>

        <ShowWhen is={isPublic}>
          <a
            target="_blank"
            href={`/public/organizations/${props.orgRoute}/sessions/${props.sessionRoute}`}
            className="btn btn-outline-secondary btn-sm"
          >
            Public Player
          </a>
        </ShowWhen>

        <ShowWhen is={showTranscript}>
          <TranscriptViewer
            title={<div className="form-control-label ttitle">Transcript</div>}
            orgRoute={props.orgRoute}
            sessionRoute={props.session?.route}
            promptRoute={props.session?.prompts[0]?.route}
            clipRoute={getValidClips(props.session)[0]?.route}
            isExpanded={showTranscript}
            transcriptState={getValidClips(props.session)[0].transcriptState}
          />
        </ShowWhen>

        <ShowWhen is={false}>
          <SessionSummary
            className="pt-3"
            orgRoute={props.orgRoute}
            sessionRoute={props.session?.route}
          />
        </ShowWhen>

        <ShowWhen is={showComments}>
          <SessionComments
            className="pt-3"
            orgRoute={props.orgRoute}
            sessionRoute={props.session?.route}
          />
        </ShowWhen>

        <ShowWhen is={isAudienceEnabled}>
          <div className="mt-2">
            <ParticipantSummary
              session={props.session}
              isExpandable={false}
              isExpanded={showAudience}
              onToggleExpansion={(isExpanded) => {
                if (isExpanded != showAudience) {
                  setShowAudience(isExpanded);
                }
              }}
            />
          </div>
        </ShowWhen>
      </div>
    );
  }
);
