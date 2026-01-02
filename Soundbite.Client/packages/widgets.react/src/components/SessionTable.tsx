import React, { useEffect, useRef, useState, forwardRef } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import moment from "moment";
import { faSignal, faTrash } from "@fortawesome/free-solid-svg-icons";

import {
  GroupDetails,
  IndexPageRequest,
  IndexPageResponse,
  MemberRole,
  PageUtils,
  ParticipantRole,
  ParticipantState,
  Person,
  PersonRole,
  SessionPreview,
  SessionSecurityType,
  Utils,
} from "@soundbite/api";

import { RecordBtn } from "../components/RecordBtn";
import { WidgetStore } from "../store/WidgetStore";
import { Loader, PlayBtn, ShowWhen } from "../components/controls";
import { ErrorDlg } from "./ErrorDlg";
import { InfiniteTable } from "./controls/InfiniteTable";
import { observer } from "mobx-react-lite";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  currentPerson?: Person;
  groupRoute?: string; // Route of the group to which sessions are limited if sessions are loaded from the current organization feed.
  orgRoute?: string; // Optional organization route
  isReadOnly?: boolean;

  disableReport?: boolean;
  disablePlay?: boolean;
  disableRecord?: boolean;
  disableDelete?: boolean;
  itemSize: number;

  // Session Load Lifecycle
  onFirstLoad: (
    allowCache: boolean,
    page: IndexPageRequest
  ) => Promise<IndexPageResponse<SessionPreview>>;
  onFurtherLoad: (
    allowCache: boolean,
    page: IndexPageRequest
  ) => Promise<IndexPageResponse<SessionPreview>>;
}

/** Imperative handle for this component */
export interface SessionTableHandle {
  refresh: () => Promise<void>;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const SessionTable = observer(
  React.forwardRef<SessionTableHandle, IProps>((props: IProps, ref) => {
    //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

    // State
    const [isLoading, setIsloading] = useState<boolean | undefined>();
    const sessions = useRef<Map<number, SessionPreview | null>>(
      new Map<number, SessionPreview | null>()
    );
    const [group, setGroup] = useState<GroupDetails | undefined>();
    const [count, setCount] = useState<number | undefined>(undefined);
    // Optional columns only show if the data demands it
    const [showLimit, setShowLimit] = useState(false);
    const [showReminder, setShowReminder] = useState(false);
    const [showPublish, setShowPublish] = useState(false);
    const maxTake = useRef<number>(32);
    const maxTotalItems = useRef<number>(1024);

    // Calculated
    const isAdmin = WidgetStore.isPersonRole(PersonRole.Admin);
    const isOwner = WidgetStore.isMemberRole(MemberRole.Owner, group);

    //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

    /**
     * Examines the given sessions to see if they activate showing any optional columns; if sessions is undefined, then this does nothing
     * @param sessions
     */
    const checkShowColumns = (sessions?: SessionPreview[]) => {
      if (sessions == null) {
        return;
      }

      for (const s of sessions) {
        if (!showReminder && s.reminder) {
          setShowReminder(true);
        }
        if ((!showPublish && s.publishSent != null) || s.publish != null) {
          setShowPublish(true);
        }
        if (!showLimit && s.limit > 0) {
          setShowLimit(true);
        }
      }
    };

    async function firstLoad(allowCache: boolean = true): Promise<void> {
      try {
        // Ensure defaults are set before proceeding
        if (
          isLoading ||
          props.currentPerson == null ||
          props.orgRoute == null
        ) {
          return;
        }

        setIsloading(true);

        if (!allowCache) {
          sessions.current.clear();
        }

        const promises: Promise<any>[] = [
          props.onFirstLoad(allowCache, { includesCounts: true }),
        ];

        if (props.groupRoute != null) {
          promises.push(
            WidgetStore.groups.readGroupAsync(
              props.orgRoute,
              props.groupRoute,
              allowCache
            )
          );
        }

        const results = await Promise.all(promises);

        const firstSessions = results[0] as IndexPageResponse<SessionPreview>;
        const newGroup = results[1] as GroupDetails | undefined;

        checkShowColumns(firstSessions.result);
        PageUtils.addPageToMap(firstSessions, sessions.current);
        maxTake.current = firstSessions.maxTake;
        maxTotalItems.current = firstSessions.maxTotalResults;
        setGroup(newGroup);
        setCount(firstSessions?.totalCount ?? 0);

        setIsloading(false);
      } catch (err) {
        ErrorDlg.show(err, "Error on first load of sessions");
      }
    }

    const loadMore = async (
      startIndex: number,
      stopIndex: number
    ): Promise<void> => {
      return new Promise<void>(async (resolve, reject) => {
        try {
          // Indicate
          for (let index = startIndex; index <= stopIndex; index++) {
            sessions.current.set(index, null);
          }

          const page = await props.onFurtherLoad(true, {
            skip: startIndex - 1,
            take: maxTake.current,
          });
          PageUtils.addPageToMap(page, sessions.current);

          resolve();
        } catch (err) {
          ErrorDlg.show(
            err,
            "Error Loading Sessions",
            `Error querying for sessions ${startIndex} to ${stopIndex}:`
          );
          reject();
        }
      });
    };

    //////////[ Lifecycle ]/////////////////////////////////////////////////////////////////////////////

    React.useImperativeHandle(ref, () => ({
      refresh() {
        return firstLoad(false);
      },
    }));

    useEffect(() => {
      firstLoad(false);
    }, [props.currentPerson, props.orgRoute, props.groupRoute]);

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
      // With dynamic group ownership, it is no longer possible to predict if the current user can or cannot play something
      return true;
    }

    function isReadyToRecord(session: SessionPreview): boolean {
      if (session.myParticipants == null) {
        return false;
      }

      const myParticipants = session.myParticipants.filter(
        (p: any) =>
          p.participantState === ParticipantState.ContributionRequested
      );

      return myParticipants.length > 0;
    }

    //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

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
      const isPrivileged = isAdmin || isOwner || iAmHost(session);
      const canRecord =
        !props.disableRecord &&
        !props.isReadOnly &&
        isPrivileged &&
        isReadyToRecord(session);
      const canDelete =
        !props.disableDelete && !props.isReadOnly && isPrivileged;
      const canReport = !props.disableReport && isPrivileged;
      const canPlay = !props.disablePlay && isReadyToPlay(session);
      return (
        <React.Fragment>
          <ShowWhen is={canReport}>
            <button
              className="btn btn-outline-primary btn-sm sb-edit-btn"
              type="button"
              onClick={() => onReport(session)}
              title="Show Session Report"
            >
              <span className="btn-inner--icon">
                <FontAwesomeIcon icon={faSignal} />
              </span>
              <span className="btn-inner--text"></span>
            </button>
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
          <ShowWhen is={canDelete}>
            <button
              className="btn btn-outline-secondary btn-sm sb-delete-btn"
              type="button"
              onClick={() => onDeleteSession(session)}
              title="Delete Session"
            >
              <span className="btn-inner--icon">
                <FontAwesomeIcon icon={faTrash} />
              </span>
              <span className="btn-inner--text">Delete</span>
            </button>
          </ShowWhen>
          <ShowWhen is={canRecord}>
            <RecordBtn
              orgRoute={props.orgRoute}
              sessionRoute={session.route}
              participantRole={ParticipantRole.Host}
            />
          </ShowWhen>
          <ShowWhen is={canPlay}>
            <PlayBtn orgRoute={props.orgRoute} sessionRoute={session.route} />
          </ShowWhen>
        </React.Fragment>
      );
    }

    const SessionRow: React.FC<{
      session: SessionPreview;
      index: number;
    }> = (rowProps: { session: SessionPreview; index: number }) => {
      // Emphasize either the reminder or publish date
      const reminderMoment = moment.utc(rowProps.session.reminder);
      const reminderTxt =
        showReminder && reminderMoment.isValid()
          ? reminderMoment.local().calendar()
          : "None";
      let reminderClass = reminderMoment.isValid() ? "" : "text-muted";

      const deadlineMoment = moment.utc(
        rowProps.session.publishSent ?? rowProps.session.publish
      );
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
        <tr
          data-session-route={rowProps.session.route}
          data-session-index={rowProps.index}
        >
          <th>
            {rowProps.session.name}
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
          <ShowWhen is={showLimit}>
            <td>{Utils.secondsToString(rowProps.session.limit)}</td>
          </ShowWhen>
          <td className="text-right">{renderButtons(rowProps.session)}</td>
        </tr>
      );
    };

    const LoadingRow: React.FC<{ index?: number }> = (rowProps: {
      index?: number;
    }) => {
      return (
        <tr
          data-row-index={rowProps.index}
          style={{ height: `${props.itemSize}px` }}
        >
          <th scope="row" className="text-muted">
            Loading {rowProps.index != null ? `${rowProps.index}...` : "..."}
          </th>
          <ShowWhen is={!props.isReadOnly && showReminder}>
            <td className="d-none d-lg-table-cell"></td>
          </ShowWhen>
          <ShowWhen is={showPublish}>
            <td className="d-none d-md-table-cell"></td>
          </ShowWhen>
          <ShowWhen is={showLimit}>
            <td></td>
          </ShowWhen>
          <td></td>
        </tr>
      );
    };

    const LastRow: React.FC = () => {
      return (
        <tr style={{ height: "73px" }}>
          <th scope="row" className="sort" data-sort="name">
            Cannot load more than {maxTotalItems.current} at this time
          </th>
          <ShowWhen is={!props.isReadOnly && showReminder}>
            <td></td>
          </ShowWhen>
          <ShowWhen is={showPublish}>
            <td></td>
          </ShowWhen>
          <ShowWhen is={showLimit}>
            <td></td>
          </ShowWhen>
          <td></td>
        </tr>
      );
    };

    const TableRow: React.FC<{ index: number }> = ({
      index,
    }: {
      index: number;
    }) => {
      if (index >= maxTotalItems.current) {
        return <LastRow />;
      }

      const session = sessions.current.get(index);

      if (session == null) {
        return <LoadingRow index={index} />;
      }

      return <SessionRow session={session} index={index} />;
    };

    const TableHeader: React.FC = () => {
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
            <ShowWhen is={showLimit}>
              <th scope="col" className="sort" data-sort="time">
                Limit
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
    };

    const isItemLoaded = (index: number) => {
      const session = sessions.current.get(index);
      return session != null;
    };

    return (
      <Loader
        isDisplayBased={true}
        isLoadedWhen={!isLoading}
        message="Loading Sessions..."
        style={{ minHeight: "12rem" }}
      >
        <InfiniteTable
          className="sb-max-width-table table align-items-center"
          count={count ?? -1 + 1}
          header={<TableHeader />}
          isItemLoaded={isItemLoaded}
          itemSize={props.itemSize}
          loadMore={loadMore}
          minimumBatchSize={32}
          row={TableRow}
        />
      </Loader>
    );
  })
);
