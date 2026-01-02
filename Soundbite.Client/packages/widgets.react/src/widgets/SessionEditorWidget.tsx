/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React, { useRef, useState } from "react";
import {
  Row,
  Col,
  Button,
  InputGroup,
  FormGroup,
  Alert,
  InputGroupAddon,
} from "reactstrap";
import moment from "moment";

import { faLock, faLockOpen } from "@fortawesome/free-solid-svg-icons";

import {
  ClipHostingType,
  ClipType,
  FileType,
  Group,
  GroupDetails,
  MemberRole,
  NewClip,
  NewParticipant,
  NewParticipantGroup,
  NewSession,
  Participant,
  ParticipantGroup,
  ParticipantRole,
  PersonRole,
  PublicError,
  Recurrence,
  Series,
  Session,
  SessionCommentPolicy,
  SessionDetails,
  SessionSecurityType,
  SessionType,
  UserRole,
  Utils,
} from "@soundbite/api";

import { OrganizationStore, WidgetStore } from "../store";
import { DialogState, ClipSource } from "../enums";
import {
  AskAccess,
  DateTimePicker,
  ErrorCtrl,
  FaPrepend,
  LabelInfo,
  Loader,
  ParticipantGroupPicker,
  ParticipantPicker,
  PeoplePicker,
  RecordAndPlay,
  RichText,
  SbProgress,
  SchedulePicker,
  ShowWhen,
  Toggle,
} from "../components";
import { CreateSessionResult } from "../interfaces";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

const styles = css`
  .sb-session-editor-widget-footer .col-md-12 {
    justify-content: flex-end;
    display: flex;
  }
`;

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  session?: SessionDetails; // Reference to the session to edit
  series?: Series | null; // Reference to series information for the session - undefined is unknown - null means session is not a series
  groupRoute?: string; // Reference to the group in which this was created
  onClose?: (isSaved: boolean) => void; // Called when the close button is clicked
  onSaved?: (session: SessionDetails) => void; // Called when the session is successfully saved
  onExpand?: () => void; // Called when the user chooses to expand the widget
  orgRoute?: string;
  startExpanded?: boolean; // Optionally declares if the widget should start expanded; default false
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
/**
 * Widget for constructing a session, including recording/uploading conte and optionally recurring sessions
 * @param props
 */
export const SessionEditorWidget: React.FC<IProps> = (props: IProps) => {
  //////////[ Define ]//////////////////////////////////////////////////////////////////////////////

  const orgRoute =
    props.orgRoute ?? WidgetStore.organizations.currentOrg?.details.route;
  const personRoute = WidgetStore.organizations.currentOrg?.details?.me?.route;
  const [group, setGroup] = useState<Group | undefined>(undefined);

  const isAdminOrMore =
    (WidgetStore.users.currentUser?.userRole ?? 0) >= UserRole.God ||
    (WidgetStore.organizations.currentOrg?.details.me?.personRole ?? 0) >=
      PersonRole.Admin;

  const isOrgSessionAllowed = WidgetStore.isPersonRole(
    WidgetStore.organizations.currentOrg?.settings.permissions
      ?.minRoleToCreateSession ?? PersonRole.Person
  );

  const areCommentsAllowed =
    WidgetStore.organizations.currentOrg?.settings.sessions
      .sessionCommentsEnabled ?? true;

  const [clipSource, setClipSource] = useState<ClipSource>(
    ClipSource.RecordAudio
  );
  const [possibleGroups, setPossibleGroups] = useState<Group[] | undefined>(
    undefined
  );

  let [title, setTitle] = useState<string>("");
  let [agenda, setAgenda] = useState<string>("");
  let [participants, setParticipants] = useState<NewParticipant[]>([]);
  let [participantGroups, setParticipantGroups] = useState<
    NewParticipantGroup[]
  >([]);
  let [schedule, setSchedule] = useState<Session & Series>({
    publish: undefined,
    reminder: undefined,
    recurrence: Recurrence.NoRepeat,
    recurrenceData: "",
    limit: 0,
  } as any as Session & Series);

  let [error, setError] = useState<PublicError | undefined>(undefined);
  let [clipState, setClipState] = useState(DialogState.Empty);
  let [isSaving, setIsSaving] = useState(false);
  let [isExpanded, setIsExpanded] = useState(props.startExpanded ?? false);
  let [isTranscribed, setIsTranscribed] = useState(
    OrganizationStore.currentOrg?.settings?.sessions?.transcriptionEnabled ===
      true &&
      OrganizationStore.currentOrg?.settings?.sessions
        ?.transcriptionOnByDefault === true
  );
  let [isPublic, setIsPublic] = useState(
    WidgetStore.organizations.currentOrg?.settings.sessions
      .defaultSessionSecurity === SessionSecurityType.Public
  );
  let [isCommentsEnabled, setIsCommentsEnabled] = useState(
    (WidgetStore.organizations.currentOrg?.settings.sessions
      .defaultSessionCommentPolicy ?? SessionCommentPolicy.Allowed) ===
      SessionCommentPolicy.Allowed
  );
  let recordAndPlay = useRef<RecordAndPlay>(null);

  const [draftSession, setDraftSession] = useState<
    CreateSessionResult | undefined
  >(undefined);
  const [saveProgress, setSaveProgress] = useState<number>(0);

  const numCols = isExpanded ? 6 : 12;

  const [isPublicEnabled, setIsPublicEnabled] = useState(false);

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  function isInCreateRole(group?: GroupDetails) {
    if (group != null) {
      return WidgetStore.isMemberRole(
        WidgetStore.organizations.currentOrg?.settings.permissions
          ?.minRoleToCreateTeamSession ?? MemberRole.Member,
        group
      );
    } else {
      return WidgetStore.isPersonRole(
        WidgetStore.organizations.currentOrg?.settings.permissions
          ?.minRoleToCreateSession ?? PersonRole.Person
      );
    }
  }

  const loadPeople = async (): Promise<void> => {
    console.warn("Still moving people loading from session editor to picker");

    if (!orgRoute) {
      return;
    }

    if (props.groupRoute) {
      const newGroupPromise = await WidgetStore.groups.readGroupAsync(
        orgRoute,
        props.groupRoute
      );

      const [newGroup] = await Promise.all([newGroupPromise]);

      if (isInCreateRole(newGroup)) {
        setGroup(newGroup);
      } else {
        setGroup(undefined);
      }
    } else {
      setGroup(undefined);
    }
  };

  const loadGroups = async (): Promise<void> => {
    if (!orgRoute) {
      setPossibleGroups([]);
      return;
    }

    if (isAdminOrMore) {
      const groups = await WidgetStore.organizations.readGroupsAsync(orgRoute);
      setPossibleGroups(groups);
    } else {
      const groups = await WidgetStore.organizations.readMyTargetGroupsAsync(
        orgRoute
      );
      setPossibleGroups(groups);
    }
  };

  const loadParticipants = (): void => {
    // Determine if a session was passed into the editor
    if (props.session) {
      // Session was passed in so populate the editor with session data
      const participants: NewParticipant[] = props.session.participants.map(
        (p: Participant) => {
          return { personRoute: p.route, participantRole: p.participantRole };
        }
      );

      const participantGroups: NewParticipantGroup[] = props.session.groups.map(
        (g: ParticipantGroup): NewParticipantGroup => {
          return { groupRoute: g.route, participantRole: g.participantRole };
        }
      );

      setTitle(props.session.name);
      if (props.session.prompts && props.session.prompts.length > 0) {
        setAgenda(props.session.prompts[0].text);
      }
      setParticipants(participants);
      setParticipantGroups(participantGroups);
      setSchedule({
        publish: props.session.publish,
        reminder: props.session.reminder,
        recurrence: props.series?.recurrence || Recurrence.NoRepeat,
        recurrenceData: props.series?.recurrenceData,
        limit: props.session.limit,
      } as Series & Session);
    } else {
      if (personRoute) {
        // Session was NOT passed in so populate defaults into the editor
        setParticipants([
          {
            personRoute: personRoute,
            participantRole: ParticipantRole.Host,
          },
        ]);
      } else {
        setParticipants([]);
      }

      // When there is a group context automatically populate the group into the group participants
      if (props.groupRoute) {
        setParticipantGroups([
          {
            groupRoute: props.groupRoute,
            participantRole: ParticipantRole.Participant,
          } as NewParticipantGroup,
        ]);
      }
    }
  };

  const load = async (): Promise<void> => {
    if (props.orgRoute == null) {
      setIsPublicEnabled(false);
      return;
    }
    if (props.groupRoute == null || props.groupRoute.trim().length === 0) {
      const isPubEnabled = WidgetStore.isPersonRole(
        WidgetStore.organizations.currentOrg?.permissions
          ?.minRoleToCreatePublic ?? PersonRole.Unknown
      );
      setIsPublicEnabled(isPubEnabled);
    } else {
      const grp = await WidgetStore.groups.readGroupAsync(
        props.orgRoute,
        props.groupRoute
      );
      const isPubEnabled = WidgetStore.isMemberRole(
        WidgetStore.organizations.currentOrg?.permissions
          ?.minRoleToCreateTeamPublic ?? MemberRole.Unknown,
        grp
      );
      setIsPublicEnabled(isPubEnabled);
    }
  };

  React.useEffect(() => {
    loadParticipants();
    loadPeople();
    loadGroups();
    load();
  }, [props.orgRoute, props.groupRoute]);

  //////////[ Methods ] ////////////////////////////////////////////////////////////////////////////

  function hostInfo(sessionType: SessionType) {
    switch (sessionType) {
      case SessionType.Announcement:
        return "Records announcement";
      case SessionType.Survey:
        return "Listens to survey results";
      default:
        return "Can edit or delete this Soundbite Session";
    }
  }

  /**
   * Determines the appropriate participant information text based on session type
   * @param sessionType - session type whose descriptive text is being sought
   */
  function participantInfo(sessionType: SessionType): string {
    switch (sessionType) {
      case SessionType.Announcement:
        return "Listens to the announcement";
      case SessionType.Survey:
        return "Responds to the survey";
      default:
        return "Can contribute to this Soundbite Session";
    }
  }

  /**
   * Gets the participant title text based on the type of session.
   * @param sessionType - type of session from which the title text is determined.
   */
  function participantTitle(sessionType: SessionType): string {
    switch (sessionType) {
      case SessionType.Announcement:
        return "Audience";
      case SessionType.Survey:
        return "Audience";
      default:
        return "Participants";
    }
  }

  /**
   * Filters a list of participants by role
   * @param role - role by which to filter
   * @param participants - list of participants to filter
   */
  function filterParticipants(
    role: ParticipantRole,
    participants?: NewParticipant[]
  ): NewParticipant[] {
    if (!participants) return [];
    return participants.filter((p) => p.participantRole !== role);
  }

  /**
   * Filters a list of groups by role
   * @param role - role by which to filter
   * @param groups - list of groups to filter
   */
  function filterGroups(
    role: ParticipantRole,
    groups?: NewParticipantGroup[]
  ): NewParticipantGroup[] {
    if (!groups) return [];
    return groups.filter((p) => p.participantRole !== role);
  }

  /**
   * Builds a list of NewParticipant instances from a list of people.
   * @param role - specifies the participant role applied to the NewParticipant instances.
   * @param people - list of people to add as participants.
   */
  function peopleRoutesToParticipants(
    role: ParticipantRole,
    peopleRoutes?: string[]
  ): NewParticipant[] {
    if (!peopleRoutes) {
      return [];
    } else {
      return peopleRoutes.map((route: string) => {
        return { personRoute: route, participantRole: role };
      });
    }
  }

  /**
   * Builds a list of NewParticipantGroup instances from a list of groups.
   * @param role - specifies the participant role applied to the NewParticipantGroup instances.
   * @param people - list of groups to add as participants.
   */
  function groupToParticipantGroup(
    role: ParticipantRole,
    groups?: Group[]
  ): NewParticipantGroup[] {
    if (!groups) {
      return [];
    } else {
      return groups.map((p) => {
        return { groupRoute: p.route, participantRole: role };
      });
    }
  }

  /**
   * Determines whether inputs on the component are valid.
   * @returns true if the inputs are valid or false if they are invalid.
   */
  function isInvalid(): boolean {
    if (isSaving) return true;

    // TODO: Visualization of field validation
    if (!title || title.length === 0) {
      return true;
    }

    const hasParticipantHost =
      participants.filter((p) => p.participantRole === ParticipantRole.Host)
        .length > 0;

    const hasGroupHost =
      participantGroups.filter(
        (p) => p.participantRole === ParticipantRole.Host
      ).length > 0;

    const hasParticipantAudience =
      participants.filter(
        (p) => p.participantRole <= ParticipantRole.Participant
      ).length > 0;

    const hasGroupAudience =
      participantGroups.filter(
        (p) => p.participantRole <= ParticipantRole.Participant
      ).length > 0;

    if (!hasParticipantHost && !hasGroupHost) {
      return true;
    }
    if (!hasParticipantAudience && !hasGroupAudience) {
      return true;
    }

    if (schedule?.publish !== undefined && schedule.reminder !== undefined) {
      const deadline = moment(schedule.publish);
      const reminder = moment(schedule.reminder);
      if (reminder.isBefore(deadline)) {
        return true; // Reminders must happen BEFORE the deadline
      }
    }

    if (clipState !== DialogState.Ready) {
      return true;
    }

    return false;
  }

  /**
   * Sets the initial selected participant groups in the group picker.  There is a bit of
   * overlapping logic between this and the initialization method because the participantGroups
   * array does not contain a "value" for the group name.
   */
  function getSelectedGroups(): any[] {
    if (props.session) {
      return ParticipantGroupPicker.convertToOptions(
        props.session.groups.map((i) => i.group)
      );
    } else {
      if (props.groupRoute && group) {
        return ParticipantGroupPicker.convertToOptions([group]);
      } else {
        return [];
      }
    }
  }

  /**
   * Sets the initial selected pariticpants in the people picker.
   */
  function getSelectedPeople(): any[] {
    if (props.session) {
      // Existing session - select existing people on the session
      return ParticipantPicker.peopleToOptions(
        props.session.participants.map((i) => i.person)
      );
    } else {
      // New session - nobody selected by default
      return [];
    }
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onPeopleChange(peopleRoutes: string[]) {
    // TODO: Support invites via e-mail in peopleRoutes
    const oldParticipants = filterParticipants(
      ParticipantRole.Participant,
      participants
    );
    const newParticipants = peopleRoutesToParticipants(
      ParticipantRole.Participant,
      peopleRoutes
    );
    setParticipants(oldParticipants.concat(newParticipants));
  }

  function onScheduleChange(sessionSchedule: Session & Series): void {
    setSchedule(sessionSchedule);
  }

  function onGroupsChange(groups: Group[]): void {
    const oldGroups = filterGroups(
      ParticipantRole.Participant,
      participantGroups
    );
    const newGroups = groupToParticipantGroup(
      ParticipantRole.Participant,
      groups
    );
    setParticipantGroups(oldGroups.concat(newGroups));
  }

  /**
   * Executes when a user clicks the Close button
   * @param isSaved - flag specifiying whether the session was saved.
   */
  function onClose(isSaved: boolean): void {
    if (props.onClose) {
      props.onClose(isSaved);
    }
  }

  function onExpand(): void {
    if (isExpanded) return;

    setIsExpanded(true);

    if (props.onExpand) {
      props.onExpand();
    }
  }

  /**
   * Fires any time the Public/Protected toggle changes.
   * @param isEnabled - flag indicating whether toggle is enabled or disabled.
   */
  function onPublicToggled(isEnabled: boolean) {
    setIsPublic(isEnabled);
    schedule.recurrence = Recurrence.NoRepeat;
    schedule.recurrenceData = "";
    onScheduleChange(schedule);
  }

  /** Assembles the session from the UI */
  function buildSession(): NewSession {
    let session: NewSession = {
      name: title,
      sessionSecurity:
        isPublic === true
          ? SessionSecurityType.Public
          : SessionSecurityType.Protected,
      firstPrompt: agenda,
      recurrence: schedule.recurrence,
      participants: participants,
      groups: participantGroups,
      publish: schedule?.publish,
      reminder: schedule?.reminder,
      recurrenceData: schedule.recurrenceData ?? "",
      limit: 0,
      reminderCalEventId: props?.session?.reminderCalEventId ?? "",
      route: props?.session?.route,
      sessionType: props?.session?.sessionType ?? SessionType.Announcement,
      publishSent: props?.session?.publishSent ?? undefined,
      reminderSent: props?.session?.reminderSent ?? undefined,
      transcribe: isTranscribed,
      sessionCommentPolicy:
        isCommentsEnabled && areCommentsAllowed
          ? SessionCommentPolicy.Allowed
          : SessionCommentPolicy.Disallowed,
    } as NewSession;

    if (session.recurrence !== Recurrence.NoRepeat) {
      // Series must have deadlines
      if (session.publish === undefined) {
        session.publish = DateTimePicker.nextHalfHourIsoString();
      }

      // TODO: Make reminder configurable
      const dayBefore = moment(session.publish).add(-1, "d");
      session.reminder = dayBefore.toISOString();
    }
    return session;
  }

  function getFileType(file?: File): FileType {
    switch (clipSource) {
      case ClipSource.RecordAudio:
        return FileType.Mp3;
      default:
        if (file) {
          const fileType = Utils.GetFileTypeFromFileName(file.name);
          if (fileType === FileType.Unknown) {
            const parts = file.name.split(".");
            const ext = parts[parts.length - 1].toLowerCase();
            throw new Error(
              `Cannot determine file type becuase extension is unsuppported: ${ext}`
            );
          }
          return fileType;
        } else {
          throw new Error(
            "Cannot determine file type because there is no file."
          );
        }
    }
  }

  /** Read the UI to build up the new clip */
  function buildClip(): NewClip {
    // The user might still be recording
    recordAndPlay.current?.stop();

    const newClip: NewClip = {
      hostingType: ClipHostingType.AzureStorage,
      clipType: ClipType.Prompt,
      fileType: getFileType(recordAndPlay.current?.file),
      participantRole: ParticipantRole.Host,
      stream: recordAndPlay.current?.file,
      seconds: recordAndPlay.current?.duration ?? 0,
    };

    // right now, we can't defer creation, but in the future we may be able to create drafts
    if (newClip.stream === undefined) {
      throw new Error("Could not find content for current clip");
    }

    return newClip;
  }

  async function onSave(): Promise<void> {
    try {
      if (!orgRoute) {
        throw new PublicError(
          undefined,
          "Cannot find Organization",
          "Cannot save session; no organization set in the editor"
        );
      }

      setSaveProgress(0);
      setIsSaving(true);

      const session = buildSession();
      const newClip = buildClip();

      const result = await WidgetStore.sessions.createSessionAsync(
        orgRoute,
        session,
        newClip,
        undefined,
        (progress: number) => {
          setSaveProgress(progress);
        },
        draftSession
      );
      setDraftSession(result);

      // If there was an issue, then we reset and throw it now because we wanted to save the session first, but still handle the error the same
      if (result.session == null) {
        setIsSaving(false);

        if (result.error != null) {
          throw result.error;
        }

        if (result.upload?.error != null) {
          throw result.upload.error;
        }

        return;
      }

      if (props.onSaved) {
        props.onSaved(result.session);
      }

      onClose(true);
    } catch (error: any) {
      let err: PublicError = error;
      if (!(err instanceof PublicError)) err = new PublicError(error);
      setError(err);
      setIsSaving(false);
    }
  }

  /** Return the error on either the draft or upload - undefined if nothing found */
  function draftError(): Error | undefined {
    return draftSession?.error ?? draftSession?.upload?.error;
  }

  /** Generate the message for the draft alert - some of these are errors, but not all */
  function draftMessage() {
    // No draft session there is nothing to talk about yet
    if (draftSession == null) return "";

    // Having a final session means we succeeded and there is nothing to talk about
    if (draftSession.session != null) {
      return "";
    }

    // Errors would be highest precedent information
    // The error messages themselves should be displayed some other way

    // Session error would happen first
    if (draftSession.error != null) {
      return `Error trying to create session.`;
    }

    // Upload error would happen second
    if (draftSession.upload?.error != null) {
      return `Error trying to upload content.`;
    }

    // From here, we're guessing at the draft states
    // These have neutral language, but as of October 2021, these should probably never happen w/o an error until there is a real draft feature

    if (draftSession.sessionDraft == null) {
      return "Unable to create session data.";
    }

    if (draftSession.upload?.clipDraft == null) {
      return "Unable to create content data.";
    }

    if (!draftSession.upload?.isReady) {
      return "Upload incomplete.";
    }

    return "Uknown Issue.";
  }

  function draftAlert() {
    if (isSaving || draftSession == null) {
      return <React.Fragment />;
    }

    const colorName = draftError() != null ? "danger" : "light";
    const message = draftMessage();

    return (
      <Row>
        <Col>
          <Alert color={colorName}>
            {message} Please Retry. For more assistance, please contact{" "}
            <a
              className="alert-link"
              href="mailto:Support@Soundbite.Freshdesk.Com"
            >
              Soundbite Support
            </a>
          </Alert>
        </Col>
      </Row>
    );
  }

  /**
   * Renders the security type section in advanced form allowing users to make public soundbites.
   */
  const SecurityTypePicker: React.FC = () => {
    if (!isPublicEnabled) {
      return <React.Fragment />;
    }

    return (
      <Row>
        <Col>
          <LabelInfo label="Security" info="How is this session protected" />
          <FormGroup className="sb-datepicker">
            <InputGroup aria-disabled={isSaving}>
              <div className="input-group-prepend">
                <div className="input-group-text">
                  <FontAwesomeIcon
                    icon={isPublic === true ? faLockOpen : faLock}
                  />
                </div>
              </div>
              <InputGroupAddon
                addonType="prepend"
                className="sb-title-input-addon"
              >
                {isPublic === true ? "Public" : "Protected"}
              </InputGroupAddon>
              <Toggle
                defaultValue={isPublic}
                onToggle={(isEnabled: boolean) => onPublicToggled(isEnabled)}
                title="Session Security"
              />
              <label className="sb-datepicker-none-label text-muted">
                {isPublic === true ? "Anyone" : "Audience"}
              </label>
            </InputGroup>
          </FormGroup>
        </Col>
      </Row>
    );
  };

  /**
   * Renders the security type section in advanced form allowing users to make public soundbites.
   */
  const TranscriptionPicker: React.FC = () => {
    if (
      !isExpanded ||
      OrganizationStore.currentOrg?.settings?.sessions?.transcriptionEnabled !==
        true
    ) {
      return <React.Fragment />;
    }

    return (
      <Row>
        <Col>
          <LabelInfo
            label="Transcription"
            info="Process speech-to-text on session"
          />
          <FormGroup className="sb-datepicker">
            <InputGroup aria-disabled={isSaving}>
              <Toggle
                defaultValue={isTranscribed}
                onToggle={(isEnabled: boolean) => setIsTranscribed(isEnabled)}
                title="Transcribe Session"
              />
              <label className="sb-datepicker-none-label text-muted">
                {isTranscribed === true
                  ? "Transcribe Session"
                  : "Do Not Transcribe Session"}
              </label>
            </InputGroup>
          </FormGroup>
        </Col>
      </Row>
    );
  };

  /**
   * Renders the session comments permission section
   */
  const CommentsPicker: React.FC = () => {
    if (!isExpanded || !areCommentsAllowed) {
      return <React.Fragment />;
    }

    return (
      <Row>
        <Col>
          <LabelInfo label="Comments" info="Allow audience text comments" />
          <FormGroup className="sb-datepicker">
            <InputGroup aria-disabled={isSaving}>
              <Toggle
                defaultValue={isCommentsEnabled}
                onToggle={(isEnabled: boolean) => {
                  setIsCommentsEnabled(isEnabled);
                }}
                title="Allow Comments for Session"
              />
              <label className="sb-datepicker-none-label text-muted">
                {isCommentsEnabled === true
                  ? "Allow Comments"
                  : "Do Not Allow Comments"}
              </label>
            </InputGroup>
          </FormGroup>
        </Col>
      </Row>
    );
  };

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  if (props.startExpanded && props.onExpand) {
    props.onExpand();
  }

  const canShowPersonPicker = isOrgSessionAllowed;

  const hasPossibleGroups = (possibleGroups?.length ?? 0) > 0;
  const canShowGroupPicker = !!props.groupRoute
    ? group != null && hasPossibleGroups
    : hasPossibleGroups;

  const canCreate = canShowGroupPicker || canShowPersonPicker;

  const sessionActionTitle = props.session ? "Update" : "Create";

  function body() {
    return (
      <React.Fragment>
        <Row>
          <Col md={numCols}>
            <FormGroup>
              <LabelInfo label="Title" info="Display name for session" />
              <InputGroup aria-disabled={isSaving}>
                <FaPrepend icon="pen" />
                <input
                  type="text"
                  className="form-control"
                  placeholder="Title"
                  aria-label="Title"
                  aria-describedby="basic-addon1"
                  defaultValue={props.session?.name}
                  onChange={(e) => setTitle(e.target.value)}
                  disabled={isSaving}
                  title="Soundbite Session Title"
                />
              </InputGroup>
            </FormGroup>
            <ShowWhen is={canShowPersonPicker || canShowGroupPicker}>
              <FormGroup>
                <LabelInfo
                  label={participantTitle(SessionType.Announcement)}
                  info={participantInfo(SessionType.Announcement)}
                />
                <ShowWhen is={canShowPersonPicker}>
                  <PeoplePicker
                    orgRoute={orgRoute}
                    groupRoute={props.groupRoute}
                    onChange={onPeopleChange}
                    disabled={isSaving}
                    title="Individuals for Session Audience"
                  />
                </ShowWhen>
                <ShowWhen is={canShowPersonPicker && canShowGroupPicker}>
                  <div className="mt-2"></div>
                </ShowWhen>
                <ShowWhen is={canShowGroupPicker}>
                  <ParticipantGroupPicker
                    onChangeGroup={onGroupsChange}
                    groups={possibleGroups}
                    defaultGroups={getSelectedGroups()}
                    disabled={isSaving}
                    title="Groups for Session Audience"
                  />
                </ShowWhen>
              </FormGroup>
            </ShowWhen>
            <RecordAndPlay
              ref={recordAndPlay}
              onChange={(state) => setClipState(state)}
              onClipSourceChange={(value: ClipSource) => setClipSource(value)}
              disabled={isSaving}
            />
          </Col>
          <ShowWhen is={isExpanded}>
            <Col md={6}>
              <TranscriptionPicker />
              <CommentsPicker />
              <FormGroup>
                <LabelInfo label="Call to Action" info="Text Description" />
                <RichText
                  placeholder="Call to Action for this Soundbite Session"
                  defaultValue={props.session?.prompts[0].text}
                  onChange={(value) => setAgenda(value)}
                  disabled={isSaving}
                  title="Text description for this Soundbite Session"
                />
              </FormGroup>
              <SecurityTypePicker />
              <SchedulePicker
                allowRecurrence={!isPublic}
                session={schedule}
                onSessionChange={onScheduleChange}
                disabled={isSaving}
              />
            </Col>
          </ShowWhen>
        </Row>
        <ErrorCtrl error={error} />
        {draftAlert()}
      </React.Fragment>
    );
  }

  function lockout() {
    return (
      <AskAccess
        className="mb-4"
        orgRoute={orgRoute}
        roleMsg="Creator"
        title="Need behind the velvet rope? Have your DJ drop you an exclusive
            backstage pass"
      />
    );
  }

  return (
    <React.Fragment>
      <div className="sb-session-editor-widget" css={styles}>
        <ShowWhen is={canCreate}>
          <Loader isLoadedWhen={!isSaving} message="Processing...">
            {body()}
          </Loader>
        </ShowWhen>
        <ShowWhen is={!canCreate}>{lockout()}</ShowWhen>
        <Row className="sb-session-editor-widget-footer">
          <Col md={12}>
            <ShowWhen is={!isSaving}>
              <ShowWhen is={canCreate}>
                <ShowWhen is={!isExpanded}>
                  <Button
                    onClick={onExpand}
                    color="secondary"
                    size="sm"
                    type="button"
                    className="sb-link-btn"
                    title="Show Advanced Soundbite Session Options"
                  >
                    Show Advanced
                  </Button>
                </ShowWhen>
              </ShowWhen>
              <button
                type="button"
                className="btn btn-outline-secondary"
                onClick={() => onClose(false)}
                title="Close"
              >
                Close
              </button>{" "}
              <ShowWhen is={canCreate}>
                <button
                  type="button"
                  className="btn btn-primary"
                  onClick={onSave}
                  title={`${sessionActionTitle} Session`}
                  disabled={isInvalid()}
                >
                  {sessionActionTitle}
                </button>
              </ShowWhen>
            </ShowWhen>
            <ShowWhen is={isSaving}>
              <SbProgress
                title="Creating Soundbite Session & Uploading Clip..."
                progress={saveProgress}
              />
            </ShowWhen>
          </Col>
        </Row>
      </div>
    </React.Fragment>
  );
};
