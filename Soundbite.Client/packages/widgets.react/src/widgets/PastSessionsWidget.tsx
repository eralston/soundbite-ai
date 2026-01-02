import React, { Fragment, useState } from "react";
import { observer } from "mobx-react-lite";
import { Card, CardHeader, Row, Col, Button } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faCalendarCheck,
  faPlus,
  faSyncAlt,
  IconDefinition,
} from "@fortawesome/free-solid-svg-icons";

import {
  Group,
  GroupDetails,
  IndexPageRequest,
  IndexPageResponse,
  MemberRole,
  PersonRole,
  SessionPreview,
  SessionsService,
} from "@soundbite/api";

import { WidgetStore } from "../store";
import { ShowWhen } from "../components/controls";
import { EmptyCardBody } from "../components";
import { SessionTable, SessionTableHandle } from "../components/SessionTable";

interface IProps {
  groupRoute?: string;
  icon?: IconDefinition;
  isReadOnly?: boolean;
  orgRoute: string;
  showCreateButton?: boolean; // Flag indicating whether the create button should be displayed
  showNoFeedImage?: boolean; // Flag indicating whether the empty card art image should be displayed when there is no feed data
  showTitle?: boolean;
  title?: string;
}

/**
 * Starts the request for more SessionPreview objects
 * @param orgRoute
 * @param groupRoute
 * @param page
 */
function readPastSessions(
  orgRoute: string,
  groupRoute?: string,
  page?: IndexPageRequest
) {
  if (groupRoute == null) {
    return SessionsService.readPastAsync(orgRoute, page);
  } else {
    return SessionsService.readGroupPastAsync(orgRoute, groupRoute, page);
  }
}

/** Display an expandable list of sessions that are pending for the given org or group */
export const PastSessionsWidget: React.FC<IProps> = observer(
  (props: IProps) => {
    let { orgRoute, groupRoute } = props;

    // Component state
    const [count, setCount] = useState<number | undefined>(undefined);
    const [isCreateEnabled, setCreateEnabled] = useState(!props.isReadOnly);
    const [title, setTitle] = useState("");
    const tableRef = React.useRef<SessionTableHandle>(null);

    // Calculated
    const showTitle = props.showTitle === false ? false : true;
    const icon = props.icon ?? faCalendarCheck;
    const currentPerson = WidgetStore.organizations.currentOrg?.details.me;

    /**
     *  Async retrieve the title given the current context and optionally a current group
     * @param group
     */
    async function getTitle(group?: Group): Promise<string> {
      if (props.title) {
        return props.title;
      }

      if (group) {
        return `${group.name} History`;
      }

      if (props.orgRoute) {
        const org = await WidgetStore.organizations.readOrgAsync(
          props.orgRoute
        );
        if (org) {
          return `${org.details.name} History`;
        }
      }

      return "My History";
    }

    React.useEffect(() => {
      const load = async () => {
        const grp =
          groupRoute != null && groupRoute.trim().length > 0
            ? await WidgetStore.groups.readGroupAsync(orgRoute, groupRoute)
            : undefined;
        const isCreateEnabled =
          !props.isReadOnly && (props.showCreateButton ?? true);
        setCreateEnabled(isCreateEnabled);
        const newTitle = await getTitle(grp);
        setTitle(newTitle);
      };
      load();
      // eslint-disable-next-line
    }, [
      props.orgRoute,
      props.groupRoute,
      WidgetStore.sessions.groupFeed,
      WidgetStore.sessions.orgFeed,
      WidgetStore.organizations.currentOrg,
    ]);

    // Events

    const onRefresh = async () => {
      tableRef.current?.refresh();
    };

    function onOpenNewSession() {
      if (props.orgRoute) {
        WidgetStore.showNewSession(
          props.orgRoute,
          props.groupRoute,
          undefined,
          () => {
            onRefresh();
          }
        );
      }
    }

    const onFirstLoad = async (
      allowCache: boolean,
      page: IndexPageRequest
    ): Promise<IndexPageResponse<SessionPreview>> => {
      page.includesCounts = true;
      const ret = await readPastSessions(orgRoute, groupRoute, page);
      setCount(ret.totalCount ?? 0);
      return ret;
    };

    const onFurtherLoad = async (
      allowCache: boolean,
      page: IndexPageRequest
    ): Promise<IndexPageResponse<SessionPreview>> => {
      const ret = await readPastSessions(orgRoute, groupRoute, page);
      return ret;
    };

    const Body: React.FC = () => {
      let emptyMsg = "No Past Sessions";
      if (isCreateEnabled) {
        emptyMsg +=
          ". Create a new session and it will appear here after publishing.";
      }
      if (count === 0) {
        return (
          <EmptyCardBody hasBorder prompt={emptyMsg} showImage={true}>
            {/*Toggle based on create role*/}
            <ShowWhen is={isCreateEnabled}>
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
        );
      }

      return (
        <SessionTable
          currentPerson={currentPerson}
          itemSize={54}
          onFirstLoad={onFirstLoad}
          onFurtherLoad={onFurtherLoad}
          orgRoute={orgRoute}
          groupRoute={groupRoute}
          ref={tableRef}
        />
      );
    };

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
                <Button
                  color="secondary"
                  outline={true}
                  onClick={onRefresh}
                  size="sm"
                  title="Refresh Session History"
                >
                  <span className="btn-inner--icon">
                    <FontAwesomeIcon icon={faSyncAlt} />
                  </span>
                </Button>
                <ShowWhen is={!props.isReadOnly && isCreateEnabled}>
                  <button
                    className="btn btn-icon btn-primary btn-sm"
                    type="button"
                    onClick={onOpenNewSession}
                    title="Create Soundbite Session"
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
                        <Fragment>&nbsp;</Fragment>Session
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

    return (
      <Card className="sb-feed-card">
        <Header />
        <Body />
      </Card>
    );
  }
);
