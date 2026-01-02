/** @jsx jsx */
import { jsx } from "@emotion/react";
import React, { Fragment, useEffect, useState } from "react";
import { Alert, Card, CardHeader, Col, Row } from "reactstrap";
import { observer } from "mobx-react-lite";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import moment from "moment";
import {
  faPlus,
  faPodcast,
  faSignal,
  faSyncAlt,
  faTrash,
} from "@fortawesome/free-solid-svg-icons";

import {
  Group,
  GroupDetails,
  MemberRole,
  ParticipantRole,
  ParticipantState,
  Person,
  PersonRole,
  PublicError,
  SessionPreview,
  SessionSecurityType,
  Utils,
} from "@soundbite/api";

import { RecordBtn } from "../components/RecordBtn";
import { EmptyCardBody } from "../components/EmptyCardBody";
import { WidgetStore } from "../store/WidgetStore";
import { Loader, PlayBtn, ShowWhen } from "../components/controls";
import { IconProp } from "@fortawesome/fontawesome-svg-core";
import { SbButton, SbButtonSize, SbButtonType } from "../components/SbButton";

/***************************************************************************************************
 *  Constants
 **************************************************************************************************/

const defaultFeedName = "Feed";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  currentUser?: Person;
  groupRoute?: string; // Route of the group to which sessions are limited if sessions are loaded from the current organization feed.
  icon?: IconProp;
  isReadOnly?: boolean;
  noFeedMessage?: string; // Message displayed when no feed is available
  orgRoute?: string; // Optional organization route
  securityType?: SessionSecurityType;
  sessions?: SessionPreview[]; // Sessions to display in the feed.  When not provided sessions are acquired from the current organization feed.
  showCreateButton?: boolean; // Flag indicating whether the create button should be displayed
  showNoFeedImage?: boolean; // Flag indicating whether the empty card art image should be displayed when there is no feed data
  showTitle?: boolean;
  title?: string;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const TableFeedWidget: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////
  const [currentUser, setCurrentUser] = useState<Person | undefined>(undefined);
  const [isLoading, setIsloading] = useState(false);
  const [feedSessions, setFeedSessions] = useState<
    SessionPreview[] | undefined
  >();
  const [title, setTitle] = useState(defaultFeedName);
  const [emptyMessage, setEmptyMessage] = useState(
    "You are all caught up on your Soundbite Sessions"
  );
  const [isCreateEnabled, setCreateEnabled] = useState(!props.isReadOnly);
  const [errorTitle, setErrorTitle] = useState<string | undefined>(undefined);

  const showNoFeedImage = props.showNoFeedImage === false ? false : true;
  const showTitle = props.showTitle === false ? false : true;

  const isAdmin = WidgetStore.isPersonRole(PersonRole.Admin);
  const icon = props.icon ?? faPodcast;

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  function orderSessions(sessions?: SessionPreview[]): SessionPreview[] {
    if (sessions == null) {
      return [];
    }
    const ret = sessions.sort((a: SessionPreview, b: SessionPreview) => {
      return (a.publishSent ?? a.publish ?? "") <
        (b.publishSent ?? b.publish ?? "")
        ? 1
        : (a.publishSent ?? a.publish ?? "") >
          (b.publishSent ?? b.publish ?? "")
        ? -1
        : 0;
    });
    return ret;
  }

  function applySessions(sessions?: SessionPreview[]): void {
    const feed = orderSessions(sessions);
    setFeedSessions(feed);
    setIsloading(false);
  }

  async function loadSessions(
    inGroup: boolean,
    allowCache: boolean = true
  ): Promise<void> {
    if (isLoading) {
      return;
    }

    setIsloading(true);

    // Ensure defaults are set before proceeding
    if (currentUser == null || props.orgRoute == null) return;

    // Determine whether the sessions were passed directly to the widget
    if (props.sessions) {
      // Use the explicitly specified sessions
      applySessions(props.sessions);
      return;
    }

    if (!allowCache) {
      setFeedSessions(undefined);
    }

    // Determine whether a group context was specified
    if (props.groupRoute) {
      // Determine whether the current user is part of the specified group
      let canLoadGroup = inGroup;
      if (!inGroup) {
        let outsideGroup = await WidgetStore.groups.readGroupAsync(
          WidgetStore.organizations.currentOrg?.details.route ?? "",
          props.groupRoute
        );
        canLoadGroup = outsideGroup != null;
      }
      if (canLoadGroup) {
        const securityType =
          props.securityType ??
          WidgetStore.organizations.currentOrg?.settings.sessions
            .defaultSessionSecurity ??
          SessionSecurityType.Protected;
        const feed = await WidgetStore.sessions.readGroupFeedAsync(
          props.orgRoute,
          props.groupRoute,
          securityType,
          allowCache
        );
        applySessions(feed);
      } else {
        throw new PublicError(
          undefined,
          "Cannot load feed for team, no access"
        );
      }
    } else {
      // Use sessions from "My Feed"
      const feed = await WidgetStore.sessions.readOrgFeedAsync(
        props.orgRoute,
        props.securityType ??
          WidgetStore.organizations.currentOrg?.settings.sessions
            .defaultSessionSecurity ??
          SessionSecurityType.Protected,
        allowCache
      );
      applySessions(feed);
    }
  }

  /** Gets the current group for the context and whether or not the current user appears to be in it */
  async function getGroup(
    allowCache: boolean
  ): Promise<{ group?: GroupDetails; inGroup: boolean }> {
    if (!props.orgRoute || !props.groupRoute) {
      return { inGroup: false };
    }

    try {
      const group = await WidgetStore.groups.readGroupAsync(
        props.orgRoute,
        props.groupRoute,
        allowCache
      );
      const inGroup = group.memberRole >= MemberRole.Member;
      return { group, inGroup };
    } catch (err) {
      setErrorTitle(`Cannot access group feed for ${props.groupRoute}`);
      return { inGroup: false };
    }
  }

  /**
   *  Async retrieve the title given the current context and optionally a current group
   * @param group
   */
  async function getTitle(group?: Group): Promise<string> {
    if (props.title) {
      return props.title;
    }
    if (group) {
      return `${group.name} Feed`;
    }

    if (props.orgRoute) {
      const org = await WidgetStore.organizations.readOrgAsync(props.orgRoute);
      if (org) {
        return `${org.details.name} Feed`;
      }
    }

    return "My Feed";
  }

  async function loadTitle(group?: Group): Promise<void> {
    const newTitle = await getTitle(group);
    setTitle(newTitle);
  }

  async function load(allowCache: boolean = true): Promise<void> {
    // Set defaults
    setCurrentUser(
      props.currentUser ?? WidgetStore.organizations.currentOrg?.details.me
    );

    if (!allowCache) {
      setIsloading(false);
      setFeedSessions(undefined);
    }

    const { group, inGroup } = await getGroup(allowCache);
    await loadTitle(group);
    await loadSessions(inGroup, allowCache);

    const isCreateEnabled = props.showCreateButton ?? true;

    setCreateEnabled(isCreateEnabled);
    const noun =
      props.groupRoute != null && props.groupRoute.trim().length > 0
        ? "team"
        : "organization";
    const noFeedMessage = isCreateEnabled
      ? props.noFeedMessage ??
        `Try making a Soundbite Session for your ${noun} to start collaborating`
      : `You are all caught up on Soundbite Sessions for your ${noun}`;
    setEmptyMessage(noFeedMessage);
  }

  useEffect(() => {
    load();
  }, [
    props.orgRoute,
    props.groupRoute,
    props.currentUser,
    props.sessions,
    props.securityType,
    currentUser,
    WidgetStore.sessions.orgFeed,
    WidgetStore.sessions.groupFeed,
  ]);

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  function iAmHost(session: SessionPreview): boolean {
    if (session.myParticipants == null) {
      return false;
    }

    const myParticipants = session.myParticipants.filter(
      (p: any) => p.participantRole === ParticipantRole.Host
    );

    return myParticipants.length > 0;
  }

  function isReadyToPlay(session: SessionPreview): boolean {
    if (!session.myParticipants || session.myParticipants.length === 0) {
      // When there are no participants but the session is public then it is playable
      return session.sessionSecurity === SessionSecurityType.Public;
    }

    const myParticipants = session.myParticipants.filter(
      (p: any) => p.participantState === ParticipantState.ConsumptionRequested
    );

    return myParticipants.length > 0;
  }

  function isReadyToRecord(session: SessionPreview): boolean {
    if (session.myParticipants == null) {
      return false;
    }

    const myParticipants = session.myParticipants.filter(
      (p: any) => p.participantState === ParticipantState.ContributionRequested
    );

    return myParticipants.length > 0;
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onOpenNewSession() {
    if (props.orgRoute) {
      WidgetStore.showNewSession(props.orgRoute, props.groupRoute);
    }
  }

  function onDeleteSession(session: SessionPreview) {
    if (props.orgRoute) {
      WidgetStore.showDeleteSession(props.orgRoute, session);
    }
  }

  //function onEditSession(session: SessionPreview): void {
  //  if (props.orgRoute) {
  //    WidgetStore.showEditSession(props.orgRoute, session);
  //  }
  //}

  function onReport(session: SessionPreview): void {
    if (props.orgRoute) {
      WidgetStore.showReports(props.orgRoute, session);
    }
  }

  //////////[ Component UI Helper Methods ]/////////////////////////////////////////////////////////

  function renderButtons(session: SessionPreview) {
    const isHost = iAmHost(session);
    const isRecordable = !props.isReadOnly && isReadyToRecord(session);
    const isRemovable = !props.isReadOnly && (isHost || isAdmin);
    const isReportable = (isHost || isAdmin) && !isRecordable;
    const isPlayable = isReadyToPlay(session);
    return (
      <React.Fragment>
        <ShowWhen is={isReportable}>
          <SbButton
            type={SbButtonType.PrimaryOutline}
            size={SbButtonSize.Small}
            onClick={() => onReport(session)}
            title="Show SessionReport"
            icon={faSignal}
          />
        </ShowWhen>
        {/*<ShowWhen is={!props.isReadOnly && false}>*/}
        {/*  <button*/}
        {/*    className="btn btn-outline-primary btn-sm sb-edit-btn"*/}
        {/*    type="button"*/}
        {/*    onClick={() => onEditSession(session)}*/}
        {/*  >*/}
        {/*    <span className="btn-inner--icon">*/}
        {/*      <FontAwesomeIcon icon={faEdit} />*/}
        {/*    </span>*/}
        {/*    <span className="btn-inner--text">Edit</span>*/}
        {/*  </button>*/}
        {/*</ShowWhen>*/}
        <ShowWhen is={isRemovable}>
          <SbButton
            type={SbButtonType.SecondaryOutline}
            size={SbButtonSize.Small}
            onClick={() => onDeleteSession(session)}
            title="Delete Session"
            icon={faTrash}
          />
        </ShowWhen>
        <ShowWhen is={isRecordable}>
          <RecordBtn
            orgRoute={props.orgRoute}
            sessionRoute={session.route}
            participantRole={ParticipantRole.Host}
            onSave={() => {
              load(false);
            }}
          />
        </ShowWhen>
        <ShowWhen is={isPlayable}>
          <PlayBtn orgRoute={props.orgRoute} sessionRoute={session.route} />
        </ShowWhen>
      </React.Fragment>
    );
  }

  function tableHeader(
    showTime: boolean,
    showReminder: boolean,
    showPublish: boolean
  ) {
    return (
      <thead className="thead-light">
        <tr>
          <th scope="col" className="sort" data-sort="name">
            Name
          </th>
          <ShowWhen is={!props.isReadOnly && showReminder}>
            <th
              scope="col"
              className="sort d-none d-lg-table-cell"
              data-sort="reminder"
            >
              Reminder
            </th>
          </ShowWhen>
          <ShowWhen is={showPublish}>
            <th
              scope="col"
              className="sort d-none d-md-table-cell"
              data-sort="publish"
            >
              Publish
            </th>
          </ShowWhen>
          <ShowWhen is={showTime}>
            <th scope="col" className="sort" data-sort="time">
              Time
            </th>
          </ShowWhen>
          <th
            scope="col"
            className="sort"
            data-sort="action"
            style={{ width: "206px" }}
          ></th>
        </tr>
      </thead>
    );
  }

  function tableRow(
    session: SessionPreview,
    key: any,
    showTime: boolean,
    showReminder: boolean,
    showPublish: boolean
  ) {
    // Emphasize either the reminder or publish date
    const reminderMoment = moment.utc(session.reminder);
    const reminderTxt =
      showReminder && reminderMoment.isValid()
        ? reminderMoment.local().calendar()
        : "None";
    let reminderClass = reminderMoment.isValid() ? "" : "text-muted";

    const deadlineMoment = moment.utc(session.publishSent ?? session.publish);
    const publishTxt =
      showPublish && deadlineMoment.isValid()
        ? deadlineMoment.local().calendar()
        : "None";
    let publishClass = deadlineMoment.isValid() ? "" : "text-muted";

    // If we're showing both and they're valid, then we have to pick one to emphasize
    if (
      showReminder &&
      showPublish &&
      reminderMoment.isValid() &&
      deadlineMoment.isValid()
    ) {
      // If we're still waiting on the deadline
      if (deadlineMoment.isBefore()) {
        reminderClass = "text-muted";
      } else {
        publishClass = "text-muted";
      }
    }

    return (
      <tr key={key} data-session-route={session.route}>
        <th>
          {session.name}
          <div className="d-table-cell d-md-none">
            <span className="text-muted sb-cell-detail">{publishTxt}</span>
          </div>
        </th>
        <ShowWhen is={!props.isReadOnly && showReminder}>
          <td className="d-none d-lg-table-cell">
            <span className={reminderClass}>{reminderTxt}</span>
          </td>
        </ShowWhen>
        <ShowWhen is={showPublish}>
          <td className="d-none d-md-table-cell">
            <span className={publishClass}>{publishTxt}</span>
          </td>
        </ShowWhen>
        <ShowWhen is={showTime}>
          <td>{Utils.secondsToString(session.limit)}</td>
        </ShowWhen>
        <td className="text-right">{renderButtons(session)}</td>
      </tr>
    );
  }

  function Header() {
    return (
      <ShowWhen is={showTitle}>
        <CardHeader className="bg-transparent">
          <Row className="align-items-center">
            <Col xs="8" md="6">
              <h1 className="mb-0">
                <FontAwesomeIcon
                  icon={icon}
                  className="d-none d-sm-inline mr-1"
                />
                {title}
              </h1>
            </Col>
            <Col className="text-right" xs="4" md="6">
              <ShowWhen is={props.sessions == null}>
                <SbButton
                  type={SbButtonType.SecondaryOutline}
                  size={SbButtonSize.Small}
                  onClick={() => {
                    load(false);
                  }}
                  title="Refresh Feed"
                  icon={faSyncAlt}
                />
              </ShowWhen>
              <ShowWhen is={isCreateEnabled}>
                <SbButton
                  type={SbButtonType.Primary}
                  size={SbButtonSize.Small}
                  icon={faPlus}
                  title="Create Soundbite Session"
                  onClick={onOpenNewSession}
                >
                  <span className="d-none d-sm-inline">Create</span>
                  <span className="d-none d-lg-inline">
                    <Fragment>&nbsp;</Fragment>Soundbite
                  </span>
                  <span className="d-none d-md-inline">
                    <Fragment>&nbsp;</Fragment>Session
                  </span>
                </SbButton>
              </ShowWhen>
            </Col>
          </Row>
        </CardHeader>
      </ShowWhen>
    );
  }

  function Body() {
    let showTime = false;
    let showReminder = false;
    let showDeadline = false;
    if (feedSessions) {
      for (const s of feedSessions) {
        if (s.reminder) showReminder = true;
        if (s.publishSent != null || s.publish != null) showDeadline = true;
        if (s.limit > 0) showTime = true;
      }
    }

    return (
      <div className="table-responsive">
        <ShowWhen is={feedSessions && feedSessions.length > 0}>
          <table className="sb-max-width-table table align-items-center">
            {tableHeader(showTime, showReminder, showDeadline)}
            <tbody className="list">
              {feedSessions?.map((s) =>
                tableRow(s, s.route, showTime, showReminder, showDeadline)
              )}
            </tbody>
          </table>
        </ShowWhen>
        <ShowWhen is={feedSessions && feedSessions.length === 0}>
          <EmptyCardBody
            hasBorder
            prompt={emptyMessage}
            showImage={showNoFeedImage}
          >
            <ShowWhen is={isCreateEnabled && !props.isReadOnly}>
              <button
                className="btn btn-icon btn-primary"
                type="button"
                onClick={onOpenNewSession}
                title="Create Soundbite Session"
              >
                <span className="btn-inner--icon">
                  <FontAwesomeIcon icon={faPlus} />
                </span>
                <span className="btn-inner--text">
                  <Fragment>&nbsp;</Fragment>
                  Create Soundbite Session
                </span>
              </button>
            </ShowWhen>
          </EmptyCardBody>
        </ShowWhen>
      </div>
    );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <Card className="sb-feed-card">
      <Loader
        isLoadedWhen={!isLoading && feedSessions != null}
        style={{ minHeight: "12rem" }}
        message="Loading Feed..."
      >
        <ShowWhen is={errorTitle == null}>
          <Header />
          <Body />
        </ShowWhen>
        <ShowWhen is={errorTitle != null}>
          <Alert color="danger">{errorTitle}</Alert>
        </ShowWhen>
      </Loader>
    </Card>
  );
});
