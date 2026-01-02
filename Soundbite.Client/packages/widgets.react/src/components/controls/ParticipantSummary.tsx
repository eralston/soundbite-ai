/** @jsx jsx */
import { jsx, css, SerializedStyles } from "@emotion/react";
import React, { useEffect, useState } from "react";
import { Button, Label } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

import {
  ParticipantRole,
  SessionType,
  ParticipantState,
  SessionDetails,
  ParticipantGroup,
  Group,
  Utils,
  PersonRole,
  Participant,
  SessionsService,
} from "@soundbite/api";

import { LabelInfo } from "./LabelInfo";
import { ShowWhen } from "./ShowWhen";
import {
  faCheck,
  faHeadphones,
  faPeopleGroup,
  faSquareMinus,
  faSquarePlus,
} from "@fortawesome/free-solid-svg-icons";
import { GlobalTheme, WidgetStore } from "../..";
import { observer } from "mobx-react-lite";

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

    .sb-participant-summary > ul:last-child {
      margin-bottom: 0;
    }

    .sb-participant-summary .sb-participant-block i {
      margin-right: 0.25rem;
    }
  `;
  return ret;
};

let styles: SerializedStyles | undefined = undefined;

/***************************************************************************************************
 *  Component Classes
 **************************************************************************************************/

class ComponentContent {
  hostInfo: string;
  participantTitle: string;
  participantInfo: string;

  constructor(hostInfo: string, title: string, info: string) {
    this.hostInfo = hostInfo;
    this.participantTitle = title;
    this.participantInfo = info;
  }
}

export class GroupParticipantWrapper {
  item: ParticipantGroup;
  isExpanded: boolean = false;
  members: Participant[] = [];
  isInitialized: boolean = false;
  hasMoreMembers?: boolean = undefined;
  isLoading: boolean = false;
  totalCount?: number = undefined;
  skipCount: number = 0;

  constructor(item: ParticipantGroup) {
    this.item = item;
  }
}

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  session?: SessionDetails;
  isExpandable?: boolean;
  isExpanded?: boolean;
  hideAudienceRoleWarning?: boolean;
  className?: string;
  onToggleExpansion?: (isExpanded: boolean) => void;
  autoExpand?: boolean;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const ParticipantSummary: React.FC<IProps> = observer(
  (props: IProps) => {
    //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

    const [isExpanded, setIsExpanded] = useState<boolean>(false);
    const [reRenderIndex, setReRenderIndex] = useState<number>(0);
    const [hosts, setHosts] = useState<Participant[]>([]);
    const [hostGroups, setHostGroups] = useState<GroupParticipantWrapper[]>([]);
    const [participants, setParticipants] = useState<Participant[]>([]);
    const [participantGroups, setParticipantGroups] = useState<
      GroupParticipantWrapper[]
    >([]);

    //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

    useEffect(() => {
      if (isExpanded != props.isExpanded) {
        toggleExpanded();
      }
    }, [props.isExpanded]);

    useEffect(() => {
      if (props.session) {
        setHosts(
          props.session.participants?.filter(
            (p) => p.participantRole === ParticipantRole.Host
          ) || []
        );
        setParticipants(
          props.session.participants?.filter(
            (p) => p.participantRole !== ParticipantRole.Host
          ) || []
        );
        setHostGroups(uniqueGroups(props.session.groups, true));
        setParticipantGroups(uniqueGroups(props.session.groups, false));

        if (props.autoExpand === true && !isExpanded && !isBig()) {
          toggleExpanded();
        }
      }
    }, [props.session]);

    //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

    /**
     * Determines whether there are an excessive number of items to show initially.
     */
    function isBig(): boolean {
      return (props.session?.participants.length ?? 0) > 128;
    }

    /**
     * Determines the appropriate terminology to use in the control based on session type.
     */
    function getContent(session: SessionDetails): ComponentContent {
      switch (session.sessionType) {
        case SessionType.Announcement:
          return new ComponentContent(
            "Records announcement",
            "Audience",
            "Listens to the announcement"
          );
        case SessionType.Survey:
          return new ComponentContent(
            "Listens to survey results",
            "Audience",
            "Responds to the survey"
          );
        default:
          return new ComponentContent(
            "Can edit or delete this Session",
            "Participants",
            "Can contribute to this Session"
          );
      }
    }

    /**
     * Toggles the state of the expansion for the participant list
     */
    function toggleExpanded(): void {
      if (props.onToggleExpansion) {
        props.onToggleExpansion(!isExpanded);
      }
      setIsExpanded(!isExpanded);
    }

    /**
     * Reads the unique Groups with a set of ParticipantGroup, returning them in alphabetical order
     * @param arr - array of groups to filter and sort
     * @param lookForHost - flag indicating whether to look for the host (true) or audience (false) participants.
     */
    function uniqueGroups(
      arr: ParticipantGroup[],
      lookForHost: boolean
    ): GroupParticipantWrapper[] {
      const filtered =
        arr?.filter((p) =>
          lookForHost
            ? p.participantRole === ParticipantRole.Host
            : p.participantRole !== ParticipantRole.Host
        ) || undefined;

      if (!filtered) return [];

      // Pull out the unique groups
      const participantGroups: GroupParticipantWrapper[] = filtered.map(
        (i) => new GroupParticipantWrapper(i)
      );
      const unique: Map<string, GroupParticipantWrapper> = new Map<
        string,
        GroupParticipantWrapper
      >();
      participantGroups.forEach((i: GroupParticipantWrapper) => {
        unique.set(i.item.groupRoute, i);
      });

      // Return them sorted
      const ret: GroupParticipantWrapper[] = Array.from(unique.values());
      ret.sort((a, b) =>
        a.item.group.name > b.item.group.name
          ? 1
          : a.item.group.name === b.item.group.name
          ? a.item.group.name > b.item.group.name
            ? 1
            : -1
          : -1
      );
      return ret;
    }

    async function toggleGroupExpanded(groupToExpand: GroupParticipantWrapper) {
      groupToExpand.isExpanded = !groupToExpand.isExpanded;
      groupToExpand.isLoading = true;
      setReRenderIndex(reRenderIndex + 1);
      fetchMembers(groupToExpand);
    }

    async function fetchMembers(groupToExpand: GroupParticipantWrapper) {
      if (
        groupToExpand.hasMoreMembers != false &&
        WidgetStore.organizations.currentOrg &&
        props.session
      ) {
        // Acquire "next" page of group members
        const result = await SessionsService.readPariticpantsInGroup(
          WidgetStore.organizations.currentOrg.details.route,
          props.session.route,
          groupToExpand.item.route,
          undefined,
          {
            includesCounts: groupToExpand.totalCount === undefined,
            skip: groupToExpand.skipCount,
            take: 100,
          }
        );

        // Update group information
        groupToExpand.members = [...groupToExpand.members, ...result.result];
        groupToExpand.totalCount =
          groupToExpand.totalCount === undefined
            ? result.totalCount
            : groupToExpand.totalCount ?? 0;
        groupToExpand.skipCount = groupToExpand.members.length;
        groupToExpand.hasMoreMembers =
          groupToExpand.members.length < (groupToExpand.totalCount ?? 0);

        // Force re-rendering
        setReRenderIndex(reRenderIndex + 2);
      }
    }

    //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

    //////////[ Component UI Helper Methods ]/////////////////////////////////////////////////////////

    function renderComponent(session: SessionDetails) {
      const content = getContent(session);
      const showRoleWarning =
        (WidgetStore.organizations.currentOrg?.permissions
          ?.minRoleForAudience ?? PersonRole.Unknown) > PersonRole.Person &&
        !props.hideAudienceRoleWarning;
      const btnMsg = "Show Audience";

      if (styles == null) {
        styles = getStyles();
      }

      return (
        <span
          className={`sb-participant-summary ${props.className}`}
          css={styles}
        >
          <ShowWhen is={props.isExpandable && !isExpanded}>
            <Button
              onClick={toggleExpanded}
              color="secondary"
              size="sm"
              type="button"
              outline={true}
              title="Show Participants"
            >
              {btnMsg}
            </Button>
          </ShowWhen>
          <ShowWhen is={isExpanded}>
            <div>
              {RenderSection("Hosts", content.hostInfo, hosts, hostGroups)}
              {RenderSection(
                content.participantTitle,
                content.participantInfo,
                participants,
                participantGroups
              )}
              <ShowWhen is={showRoleWarning}>
                <small className="text-muted">
                  This audience is only visible to{" "}
                  {WidgetStore.organizations.currentOrg?.details.name} admins
                </small>
              </ShowWhen>
            </div>
          </ShowWhen>
        </span>
      );
    }

    function RenderParticipant(p: Participant) {
      return (
        <li
          key={p.person.route}
          className={
            "sb-participant-block sb-participant-state-" +
            ParticipantState[p.participantState].toLowerCase()
          }
        >
          <ShowWhen is={p.participantState === ParticipantState.Consumed}>
            <FontAwesomeIcon icon={faHeadphones} />
          </ShowWhen>
          {Utils.userDisplay(p.person.user)}
        </li>
      );
    }

    function RenderSection(
      label: string,
      labelDesc: string,
      people: Participant[],
      groups: GroupParticipantWrapper[]
    ) {
      return (
        <React.Fragment>
          <ShowWhen is={people.length > 0 || groups.length > 0}>
            <LabelInfo label={label} info={labelDesc} />
            <ShowWhen is={people.length > 0}>
              <ul className="sb-block-list">
                {people.map((p) => RenderParticipant(p))}
              </ul>
            </ShowWhen>
            <ShowWhen is={groups.length > 0}>
              <ul className="sb-block-list">
                {groups.map((group) => {
                  return (
                    <React.Fragment key={group.item.route}>
                      <li
                        className="sb-pg-expandable"
                        onClick={() => toggleGroupExpanded(group)}
                      >
                        <FontAwesomeIcon
                          icon={
                            reRenderIndex > 0 && group.isExpanded
                              ? faSquareMinus
                              : faSquarePlus
                          }
                        />{" "}
                        {group.item.group.name}
                      </li>
                      <ShowWhen is={group.isExpanded}>
                        {group.members.map((p, index) => RenderParticipant(p))}
                        <ShowWhen is={group.hasMoreMembers === true}>
                          <li
                            className={"sb-pg-expandable"}
                            onClick={() => fetchMembers(group)}
                          >
                            Load More
                          </li>
                        </ShowWhen>
                      </ShowWhen>
                    </React.Fragment>
                  );
                })}
              </ul>
            </ShowWhen>
          </ShowWhen>
        </React.Fragment>
      );
    }

    //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

    if (props.session !== undefined && props.session !== null) {
      return renderComponent(props.session);
    } else {
      // Do not render the component
      return <React.Fragment />;
    }
  }
);
