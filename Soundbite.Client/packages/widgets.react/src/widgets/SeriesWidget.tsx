/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React, { Fragment, useEffect, useState } from "react";
import { Alert, Button, Card, CardHeader, Col, Row } from "reactstrap";
import { observer } from "mobx-react-lite";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faCalendar,
  faPlus,
  faSyncAlt,
  faTrash,
} from "@fortawesome/free-solid-svg-icons";

import {
  Group,
  GroupDetails,
  MemberRole,
  Participant,
  ParticipantRole,
  Person,
  PersonRole,
  PublicError,
  SeriesPreview,
} from "@soundbite/api";

import {
  EmptyCardBody,
  ShowWhen,
  RecurrenceSummary,
  Loader,
} from "../components";
import { WidgetStore } from "../store";
import SeriesDetailsBtn from "../components/controls/SeriesDetailsBtn";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  currentUser?: Person;
  noFeedMessage?: string; // Message displayed when no feed is available
  onRecordingSaved?: () => void;
  orgRoute: string;
  series?: SeriesPreview[]; // Sessions to display in the feed.  When not provided sessions are acquired from the current organization feed.
  showCreateButton?: boolean; // Flag indicating whether the create button should be displayed
  groupRoute?: string; // Route of the group to which sessions are limited if sessions are loaded from the current organization feed.
  title?: string;
  showTitle?: boolean;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const SeriesWidget: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////
  let [isLoading, setIsLoading] = React.useState(false);
  const [title, setTitle] = useState("Calendar");
  const [feedSeries, setFeedSeries] = useState<SeriesPreview[] | undefined>([]);
  const [isCreateEnabled, setCreateEnabled] = useState(false);
  const [emptyMessage, setEmptyMessage] = useState(
    "You have no recurring Soundbite Sessions"
  );
  const isGroupMode =
    props.groupRoute != null && props.groupRoute.trim().length > 0;
  const [errorTitle, setErrorTitle] = useState<string | undefined>(undefined);
  const showTitle = props.showTitle === false ? false : true;

  const orderSessions = (series?: SeriesPreview[]) => {
    if (series == null) {
      return [];
    }
    const ret = series.sort((a: SeriesPreview, b: SeriesPreview) => {
      return (a.template.publish ?? "") < (b.template.publish ?? "")
        ? 1
        : (a.template.publish ?? "") > (b.template.publish ?? "")
        ? -1
        : 0;
    });
    return ret;
  };

  const applySeries = (sessions: SeriesPreview[]) => {
    const feed = orderSessions(sessions);
    setFeedSeries(feed);
  };

  const loadSeries = async (
    inGroup: boolean,
    allowCache: boolean = true
  ): Promise<void> => {
    // Determine whether the sessions were passed directly to the widget
    if (props.series) {
      // Use the explicitly specified sessions
      applySeries(props.series);
      return;
    }

    // Determine whether a group context was specified
    if (props.groupRoute) {
      // Determine whether the current user is part of the specified group
      let canLoadGroup = inGroup;
      if (!inGroup) {
        let outsideGroup = await WidgetStore.groups.readGroupAsync(
          props.orgRoute,
          props.groupRoute,
          allowCache
        );
        canLoadGroup = outsideGroup != null;
      }
      if (canLoadGroup) {
        const series = await WidgetStore.sessions.readGroupSeriesAsync(
          props.orgRoute,
          props.groupRoute,
          allowCache
        );
        applySeries(series);
      } else {
        throw new PublicError(
          undefined,
          "Cannot load feed for team, no access"
        );
      }
    } else {
      // Use sessions from "My Feed"
      const feed = await WidgetStore.sessions.readOrgSeriesAsync(
        props.orgRoute,
        allowCache
      );
      applySeries(feed);
    }
  };

  /** Gets the current group for the context and whether or not the current user appears to be in it */
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
      setErrorTitle(`Cannot access group series for ${props.groupRoute}`);
      return { inGroup: false };
    }
  }

  /**
   *  Async retrieve the title given the current context and optionally a current group
   * @param group
   */
  const getTitle = async (
    group?: Group,
    allowCache: boolean = true
  ): Promise<string> => {
    if (props.title) {
      return props.title;
    }
    if (group) {
      return `${group.name} Calendar`;
    }

    if (props.orgRoute) {
      const org = await WidgetStore.organizations.readOrgAsync(
        props.orgRoute,
        allowCache
      );
      if (org) {
        return `${org.details.name} Calendar`;
      }
    }

    return "My Calendar";
  };

  const loadTitle = async (
    group?: Group,
    allowCache: boolean = true
  ): Promise<void> => {
    const newTitle = await getTitle(group, allowCache);
    setTitle(newTitle);
  };

  const load = async (allowCache: boolean = true): Promise<void> => {
    if (isLoading) {
      return;
    }

    setIsLoading(true);

    // TODO: Parallel?
    // Reset the feed first
    setFeedSeries(undefined);

    const { group, inGroup } = await getGroup(allowCache);
    await loadTitle(group), allowCache;
    await loadSeries(inGroup, allowCache);

    const isCreateEnabled = props.showCreateButton ?? true;
    setCreateEnabled(isCreateEnabled);
    const noun = isGroupMode ? "team" : "organization";
    const noFeedMessage = isCreateEnabled
      ? props.noFeedMessage ??
        `Try making a recurring Soundbite Session for your ${noun} to consistently collaborate`
      : `Your ${noun} does not yet have any recurring Soundbite Sessions`;
    setEmptyMessage(noFeedMessage);
    setIsLoading(false);
  };

  useEffect(() => {
    load();
  }, [
    props.orgRoute,
    props.groupRoute,
    props.currentUser,
    props.series,
    WidgetStore.sessions.orgSeries,
    WidgetStore.sessions.groupSeries,
  ]);

  React.useEffect(() => {
    applySeries(props.series ?? []);
  }, [props.series]);

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  function iAmHost(series: SeriesPreview): boolean {
    const myParticipants = series.template.myParticipants.filter(
      (p: Participant) => p.participantRole === ParticipantRole.Host
    );
    return myParticipants.length > 0;
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onOpenNewSession() {
    WidgetStore.showNewSession(props.orgRoute, props.groupRoute, true);
  }

  //////////[ Component UI Helper Methods ]/////////////////////////////////////////////////////////

  function Header() {
    return (
      <ShowWhen is={showTitle}>
        <CardHeader className="border-0">
          <Row className="align-items-center">
            <Col xs="8" md="6">
              <h1 className="mb-0">
                <FontAwesomeIcon
                  icon={faCalendar}
                  className="d-none d-sm-inline mr-1"
                />
                {title}
              </h1>
            </Col>
            <Col className="text-right" xs="4" md="6">
              <Button
                className="btn-icon"
                size="sm"
                color="secondary"
                outline={true}
                type="button"
                onClick={() => {
                  load(false);
                }}
                title="Refresh Series"
              >
                <span className="btn-inner--icon">
                  <FontAwesomeIcon icon={faSyncAlt} />
                </span>
              </Button>
              <ShowWhen is={isCreateEnabled}>
                <button
                  className="btn btn-icon btn-primary btn-sm"
                  type="button"
                  onClick={onOpenNewSession}
                  title="Create Soundbite Series"
                >
                  <span className="btn-inner--icon">
                    <FontAwesomeIcon icon={faPlus} />
                  </span>
                  <span className="btn-inner--text">
                    <span className="d-none d-sm-inline">
                      <Fragment>&nbsp;</Fragment>Create
                    </span>
                    <span className="d-none d-lg-inline">
                      <Fragment>&nbsp;</Fragment>Soundbite
                    </span>
                    <span className="d-none d-md-inline">
                      <Fragment>&nbsp;</Fragment>Series
                    </span>
                  </span>
                </button>
              </ShowWhen>
            </Col>
          </Row>
        </CardHeader>
      </ShowWhen>
    );
  }

  function Body() {
    return (
      <div className="table-responsive">
        <ShowWhen is={(feedSeries?.length ?? 0) === 0}>
          <EmptyCardBody hasBorder prompt={emptyMessage}>
            <ShowWhen is={isCreateEnabled}>
              <button
                className="btn btn-icon btn-primary"
                type="button"
                onClick={onOpenNewSession}
                title="Create Soundbite Series"
              >
                <span className="btn-inner--icon">
                  <FontAwesomeIcon icon={faPlus} />
                </span>
                <span className="btn-inner--text">
                  <Fragment>&nbsp;</Fragment>
                  Create Soundbite Series
                </span>
              </button>
            </ShowWhen>
          </EmptyCardBody>
        </ShowWhen>
        <ShowWhen is={(feedSeries?.length ?? 0) !== 0}>
          <table className="table align-items-center sb-max-width-table">
            <thead className="thead-light">
              <tr>
                <th scope="col" className="sort" data-sort="name">
                  Name
                </th>
                <th
                  scope="col"
                  className="sort d-none d-md-table-cell"
                  data-sort="appointment"
                >
                  Appointment
                </th>
                <th
                  scope="col"
                  className="sort"
                  style={{ width: "164px" }}
                ></th>
              </tr>
            </thead>
            <tbody className="list">{feedSeries?.map(renderRow)}</tbody>
          </table>
        </ShowWhen>
      </div>
    );
  }

  function renderRow(series: SeriesPreview, key: any) {
    return (
      <tr key={series.route} data-series-route={series.route}>
        <th>
          <span className="">{series.name}</span>
          <div className="d-table-cell d-md-none">
            <span className="text-muted sb-cell-detail">
              <RecurrenceSummary series={series} />
            </span>
          </div>
        </th>
        <td className="sort d-none d-md-table-cell">
          <RecurrenceSummary series={series} />
        </td>
        <td className="text-right">
          <SeriesDetailsBtn series={series} />
          <ShowWhen is={iAmHost(series)}>
            <button
              className="btn btn-outline-secondary btn-sm"
              type="button"
              onClick={() =>
                WidgetStore.showDeleteSeries(props.orgRoute, series)
              }
              title="Delete Series"
            >
              <span className="btn-inner--icon">
                <FontAwesomeIcon icon={faTrash} />
              </span>
            </button>
          </ShowWhen>
        </td>
      </tr>
    );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <Card className="sb-feed-card">
      <Loader
        isLoadedWhen={!isLoading && feedSeries != null}
        style={{ minHeight: "12rem" }}
        message="Loading Series..."
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
