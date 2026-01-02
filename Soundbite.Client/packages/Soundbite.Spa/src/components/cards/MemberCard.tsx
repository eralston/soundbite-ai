import React, { Fragment, useRef } from "react";
import { observer } from "mobx-react-lite";
import { Card, Row, Col, CardHeader } from "reactstrap";
import {
  Group,
  GroupDetails,
  IndexPageResponse,
  Invite,
  Member,
  MemberRole,
  MembersService,
  OrganizationWithPermissions,
  PageUtils,
  User,
} from "@soundbite/api";
import {
  Avatar,
  DialogManager,
  EmptyCardBody,
  InfiniteScrollDataHelper,
  InfiniteTable,
  Loader,
  ShowWhen,
  Toggle,
  WidgetStore,
} from "@soundbite/widgets-react";

import { DeleteMemberDlg, IMemberAndGroup } from "../dialogs/DeleteMemberDlg";
import { AddMemberDialog, IAddMember } from "../dialogs/AddMemberDialog";
import { useState } from "react";
import { useEffect } from "react";
import { useParams } from "react-router-dom";

export interface IMemberCardProps {
  title?: string;
  myPersonRoute?: string;
  orgRoute: string;
  groupRoute: string;
}

export const MemberCard: React.FC<IMemberCardProps> = observer(
  (props: IMemberCardProps) => {
    const { orgRoute, groupRoute } = useParams<{
      orgRoute: string;
      groupRoute: string;
    }>();

    /** Infiniate Scroll State **/
    const maxTake = useRef<number>(128);
    const maxTotalItems = useRef<number>(2147483647);
    const [count, setCount] = useState<number | undefined>(undefined);
    const items = useRef<Map<number, Member | null>>(
      new Map<number, Member | null>()
    );
    const setItems = (page: IndexPageResponse<Member>) => {
      PageUtils.addPageToMap(page, items.current);
    };

    const [isLoading, setIsLoading] = useState(false);
    const [org, setOrg] = useState<OrganizationWithPermissions | undefined>(
      undefined
    );
    const [group, setGroup] = useState<GroupDetails | undefined>(undefined);

    const myPersonRoute = props.myPersonRoute ?? org?.details.me?.route;
    const [isOwnerOrMore, setIsOwnerOrMore] = useState(false);
    const [isAddMemberEnabled, setIsAddMemberEnabled] = useState(false);

    async function load() {
      if (isLoading) {
        return;
      }
      setIsLoading(true);
      setOrg(undefined);
      setGroup(undefined);

      const orgPromise = WidgetStore.organizations.readOrgAsync(props.orgRoute);
      const groupPromise = WidgetStore.groups.readGroupAsync(
        props.orgRoute,
        props.groupRoute
      );
      const membersPromise = MembersService.readAllAsync(
        props.orgRoute,
        props.groupRoute,
        { includesCounts: true }
      );
      const [newOrg, newGroup, firstMembersPage] = await Promise.all([
        orgPromise,
        groupPromise,
        membersPromise,
      ]);

      setItems(firstMembersPage);
      setCount(firstMembersPage.totalCount);
      maxTake.current = firstMembersPage.maxTake;

      setIsOwnerOrMore(WidgetStore.isMemberRole(MemberRole.Owner, newGroup));
      setIsAddMemberEnabled(
        WidgetStore.isMemberRole(
          newOrg.permissions.minRoleForTeamInvite,
          newGroup
        )
      );
      setOrg(newOrg);
      setGroup(newGroup);
      setIsLoading(false);
    }

    useEffect(() => {
      load();
      // eslint-disable-next-line
    }, [orgRoute, groupRoute]);

    async function onSubmitAddMember(addMember?: IAddMember) {
      if (!addMember) return;

      const invites = addMember.newPeopleTokens.map((p): Invite => {
        return { token: p };
      });
      await WidgetStore.members.addMembersAsync(
        props.orgRoute,
        props.groupRoute,
        invites
      );
      await load();
    }

    function onAddMember() {
      let addMember: IAddMember = {
        newPeopleTokens: [],
        orgRoute,
      };
      AddMemberDialog.open(addMember, onSubmitAddMember);
    }

    async function onSubmitRemoveMember(memberAndGroup?: IMemberAndGroup) {
      if (memberAndGroup) {
        await WidgetStore.members.deleteMemberAsync(
          props.orgRoute,
          props.groupRoute,
          memberAndGroup.member.route
        );
        await load();
      }
    }

    function onDeleteMember(group: Group, member: Member) {
      DeleteMemberDlg.open({ group, member }, onSubmitRemoveMember);
    }

    async function onToggleOwner(ownerChecked: boolean, member: Member) {
      member.memberRole = ownerChecked ? MemberRole.Owner : MemberRole.Member;
      await WidgetStore.members.updateMemberAsync(
        props.orgRoute,
        props.groupRoute,
        member
      );
    }

    function onMemberClick(user: User) {
      DialogManager.ShowProfileDialog(user, false);
    }

    const TableHeader: React.FC = () => {
      return (
        <thead className="thead-light">
          <tr>
            <th scope="col" className="sort" data-sort="name">
              Name
            </th>
            <th
              scope="col"
              className="sort d-none d-lg-table-cell"
              data-sort="role"
            >
              E-Mail
            </th>
            <th scope="col" className="sort" data-sort="deadline">
              Owner
            </th>
            <th scope="col" className="sort" data-sort="action"></th>
          </tr>
        </thead>
      );
    };

    const cardHeader = () => {
      return (
        <CardHeader className="border-0">
          <Row className="align-items-center">
            <Col xs="10" sm="9" md="8">
              <h1 className="mb-0">
                <i className="fas fa-users d-none d-sm-inline"></i>
                {props.title ?? `${group && group.name} Members`}
              </h1>
              <ShowWhen is={group != null}>
                <div className="mt-0 text-muted">{count} Members</div>
              </ShowWhen>
            </Col>
            <Col className="text-right" xs="2" sm="3" md="4">
              <ShowWhen is={org && group && isAddMemberEnabled}>
                <button
                  className="btn btn-icon btn-primary btn-sm"
                  type="button"
                  onClick={onAddMember}
                  title="Invite Members"
                >
                  <span className="btn-inner--icon">
                    <i className="fas fa-user-plus"></i>
                  </span>
                  <span className="btn-inner--text">
                    <span className="d-none d-sm-inline">
                      <Fragment>&nbsp;</Fragment>Invite
                    </span>
                    <span className="d-none d-md-inline">
                      <Fragment>&nbsp;</Fragment>Members
                    </span>
                  </span>
                </button>
              </ShowWhen>
            </Col>
          </Row>
        </CardHeader>
      );
    };

    const noMembers = () => {
      return (
        <ShowWhen is={!isLoading && (count ?? 0) <= 1 && isAddMemberEnabled}>
          <EmptyCardBody
            hasBorder
            prompt="Lonely? Try adding or inviting your teammates"
          >
            <div>
              <button
                className="btn btn-icon btn-primary"
                type="button"
                onClick={onAddMember}
                title="Invite Members"
              >
                <span className="btn-inner--icon">
                  <i className="fas fa-user-plus"></i>
                </span>
                <span className="btn-inner--text">
                  <Fragment>&nbsp;</Fragment>Invite Members
                </span>
              </button>
            </div>
          </EmptyCardBody>
        </ShowWhen>
      );
    };

    /************************************************************************************************/
    /** Infinite Scroll */
    /************************************************************************************************/
    let itemCount = count ?? 0;

    if (itemCount > maxTotalItems.current) {
      itemCount = maxTotalItems.current + 1;
    }

    const isItemLoaded = (index: number) => {
      const item = items.current.get(index);
      return item != null;
    };

    const scrollData: InfiniteScrollDataHelper = new InfiniteScrollDataHelper(
      (startIndex: number, stopIndex: number) => {
        return new Promise<void>(async (resolve, reject) => {
          try {
            // Indicate
            for (let index = startIndex; index <= stopIndex; index++) {
              items.current.set(index, null);
            }

            const page = await MembersService.readAllAsync(
              orgRoute,
              groupRoute,
              {
                skip: startIndex - 1,
                take: maxTake.current,
              }
            );

            PageUtils.addPageToMap(page, items.current);

            resolve();
          } catch (err) {
            reject(err);
          }
        });
      }
    );

    const ItemRow: React.FC<{ item: Member; index: number }> = (props: {
      item: Member;
      index: number;
    }) => {
      const item = props.item;
      const isRowForCurrentUser = myPersonRoute === item.person.route;
      return (
        <tr
          key={item.route}
          data-member-route={item.route}
          data-person-route={item.personRoute}
          data-user-route={item.person.user.route}
        >
          <th scope="row">
            <Avatar
              user={item.person.user}
              onClick={() => onMemberClick(item.person.user)}
              title="Show Member Profile"
            />
          </th>
          <td className="d-none d-lg-table-cell">
            <a
              href={`mailto:${item.person.user.email}`}
              title={`Send e-mail to ${item.person.user.email}`}
            >
              {item.person.user.email}
            </a>
          </td>
          <td>
            {/* When the curent user is an admin, display a button for toggling members who are not themselves into owner*/}
            <ShowWhen is={isOwnerOrMore && isAddMemberEnabled}>
              <ShowWhen is={isRowForCurrentUser}>
                <div className="text-muted">Owner</div>
              </ShowWhen>
              <ShowWhen is={!isRowForCurrentUser}>
                <Toggle
                  defaultValue={item.memberRole >= MemberRole.Owner}
                  onToggle={(isEnabled: boolean) =>
                    onToggleOwner(isEnabled, item)
                  }
                  title="Member is Team Owner"
                />
              </ShowWhen>
            </ShowWhen>
            {/* When the current user is NOT an admin, then simply show static text when this row's user is an owner*/}
            <ShowWhen is={!isOwnerOrMore}>
              <ShowWhen is={item.memberRole >= MemberRole.Owner}>
                <div className="text-muted">Owner</div>
              </ShowWhen>
            </ShowWhen>
          </td>
          <td className="text-right">
            <ShowWhen is={!isRowForCurrentUser && isOwnerOrMore}>
              <button
                className="btn btn-outline-secondary btn-sm"
                type="button"
                onClick={() => group && onDeleteMember(group, item)}
                title="Remove User from Team"
              >
                <span className="btn-inner--icon">
                  <i className="fas fa-times"></i>
                </span>
              </button>
            </ShowWhen>
          </td>
        </tr>
      );
    };

    const ItemRowLoading: React.FC<{ index?: number }> = (props: {
      index?: number;
    }) => {
      return (
        <tr data-row-index={props.index} style={{ height: "73px" }}>
          <th scope="row" className="text-muted">
            Loading {props.index != null ? `${props.index}...` : "..."}
          </th>
          <td className="d-none d-lg-table-cell"></td>
          <td></td>
          <td></td>
        </tr>
      );
    };

    const ItemRowLast: React.FC = () => {
      return (
        <tr style={{ height: "73px" }}>
          <th scope="row" className="text-muted">
            Cannot load more than {maxTotalItems.current} at this time
          </th>
          <td className="d-none d-lg-table-cell"></td>
          <td></td>
          <td></td>
        </tr>
      );
    };

    /** The Row component. This should be a table row, and noted that we don't use the style that regular `react-window` examples pass in.*/
    function TableRow({ index }: { index: number }) {
      const item = items.current.get(index);
      if (index >= maxTotalItems.current) {
        return <ItemRowLast />;
      } else if (item == null) {
        return <ItemRowLoading index={index} />;
      } else {
        return <ItemRow item={item} index={index} />;
      }
    }

    /************************************************************************************************/

    return (
      <Card>
        <Loader
          isLoadedWhen={!isLoading && !!org && !!group}
          style={{ minHeight: "12rem" }}
          message="Loading Members..."
        >
          {cardHeader()}
          <div className="table-responsive">
            <ShowWhen is={count != null}>
              <InfiniteTable
                count={itemCount}
                header={<TableHeader />}
                isItemLoaded={isItemLoaded}
                itemSize={73}
                loadMore={(startIndex: number, endIndex: number) =>
                  scrollData.getMoreItems(startIndex, endIndex)
                }
                minimumBatchSize={128}
                row={TableRow}
              />
            </ShowWhen>
            {noMembers()}
          </div>
        </Loader>
      </Card>
    );
  }
);
