import React, { Fragment, useState } from "react";
import { observer } from "mobx-react-lite";
import { NavLink } from "react-router-dom";
import { Card, CardHeader, Row, Col, Button } from "reactstrap";

import { Group, OrganizationWithPermissions } from "@soundbite/api";
import {
  EmptyCardBody,
  Loader,
  ShowWhen,
  WidgetStore,
} from "@soundbite/widgets-react";

import { NewGroupDialog } from "../dialogs/NewGroupDialog";
import { DeleteGroupDialog } from "../dialogs/DeleteGroupDialog";
import SpaStore from "../../store/Spa.Store";
import { useEffect } from "react";

interface IProps {
  orgRoute: string;
}

export const GroupsCard: React.FC<IProps> = observer((props: IProps) => {
  // New Group Dialog
  const [createIsOpen, setCreateIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [groups, setGroups] = useState<Group[] | undefined>(undefined);
  const [org, setOrg] = useState<OrganizationWithPermissions | undefined>(
    undefined
  );

  const load = async (allowCache = true): Promise<void> => {
    if (isLoading) {
      return;
    }

    setIsLoading(true);
    if (!allowCache) {
      setGroups(undefined);
      setOrg(undefined);
    }

    const allGroupsPromise = WidgetStore.organizations.readGroupsAsync(
      props.orgRoute,
      allowCache
    );

    const newOrgPromise = WidgetStore.organizations.readOrgAsync(
      props.orgRoute,
      allowCache
    );

    const [newGroups, newOrg] = await Promise.all([
      allGroupsPromise,
      newOrgPromise,
    ]);
    const sortedNewGroups =
      newGroups.slice().sort((a, b) => (a > b ? -1 : 1)) ?? [];
    setGroups(sortedNewGroups);
    setOrg(newOrg);
    setIsLoading(false);
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line
  }, [props.orgRoute, WidgetStore.organizations.currentOrg]);

  const onClose = () => {
    setCreateIsOpen(false);
  };

  const onOpenCreate = () => {
    setCreateIsOpen(true);
  };

  const onRefresh = async () => {
    load(false);
  };

  const TableBody = () => {
    const onSubmitDelete = async (grp?: Group) => {
      if (grp) {
        await WidgetStore.groups.deleteGroupAsync(props.orgRoute, grp.route);
        await load();
      }
    };

    const openDeleteGroupDialog = (grp: Group) => {
      DeleteGroupDialog.open(grp, onSubmitDelete);
    };

    return (
      <div className="table-responsive">
        <ShowWhen is={!!WidgetStore.organizations.currentOrg}>
          <table className="table align-items-center sb-max-width-table">
            <thead className="thead-light">
              <tr>
                <th scope="col" className="sort" data-sort="name">
                  Name
                </th>
                <th
                  scope="col"
                  className="sort  d-none d-lg-table-cell"
                  data-sort="reminder"
                >
                  Description
                </th>
                <th
                  scope="col"
                  className="sort"
                  data-sort="action"
                  style={{ width: "64px" }}
                ></th>
              </tr>
            </thead>
            <tbody className="list">
              {groups?.map((grp: Group) => {
                return (
                  <tr key={grp.route} data-team-route={grp.route}>
                    <th>
                      <NavLink
                        activeClassName="active"
                        className="nav-link"
                        to={`/organizations/${props.orgRoute}/groups/${grp.route}`}
                      >
                        {grp.name}
                      </NavLink>
                    </th>
                    <td className="d-none d-lg-table-cell">
                      {grp.description}
                    </td>
                    <td className="text-right">
                      <button
                        className="btn btn-outline-secondary btn-sm"
                        type="button"
                        onClick={() => openDeleteGroupDialog(grp)}
                        title="Delete Group"
                      >
                        <span className="btn-inner--icon">
                          <i className="fas fa-trash"></i>
                        </span>
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </ShowWhen>
        <ShowWhen is={(groups?.length ?? 0) <= 1}>
          <EmptyCardBody
            hasBorder
            prompt="Better make more groups to crank productivity up to 11"
          >
            <div>
              <ShowWhen is={SpaStore.config.featureFlags.teamCreationEnabled}>
                <button
                  className="btn btn-icon btn-primary"
                  type="button"
                  onClick={onOpenCreate}
                  title="Create Group"
                >
                  <span className="btn-inner--icon">
                    <i className="fas fa-users"></i>
                  </span>
                  <span className="btn-inner--text">
                    <Fragment>&nbsp;</Fragment>Create Group
                  </span>
                </button>
              </ShowWhen>
            </div>
          </EmptyCardBody>
        </ShowWhen>
      </div>
    );
  };

  return (
    <Card>
      <Loader
        isLoadedWhen={!!org && !!groups}
        style={{ minHeight: "12rem" }}
        message="Loading Groups..."
      >
        <CardHeader className="border-0">
          <Row className="align-items-center">
            <Col xs="8" md="7">
              <h1 className="mb-0">
                <i className="fas fa-users-cog d-none d-sm-inline"></i>
                {groups?.length} {org?.details.name} Groups
              </h1>
            </Col>
            <Col className="text-right" xs="4" md="5">
              <Button
                outline={true}
                color="secondary"
                className="btn-icon btn-sm"
                onClick={onRefresh}
                title="Refresh Groups"
              >
                <span className="btn-inner--icon">
                  <i className="fas fa-sync-alt"></i>
                </span>
              </Button>
              <ShowWhen is={SpaStore.config.featureFlags.teamCreationEnabled}>
                <button
                  className="btn btn-icon btn-primary btn-sm"
                  type="button"
                  onClick={onOpenCreate}
                  title="Create Group"
                >
                  <span className="btn-inner--icon">
                    <i className="fas fa-users"></i>
                  </span>
                  <span className="btn-inner--text">
                    <span className="d-none d-sm-inline">
                      <Fragment>&nbsp;</Fragment>Create
                    </span>
                    <span className="d-none d-md-inline">
                      <Fragment>&nbsp;</Fragment>Group
                    </span>
                  </span>
                </button>
              </ShowWhen>
            </Col>
          </Row>
        </CardHeader>
        <TableBody />
      </Loader>
      <NewGroupDialog
        orgRoute={props.orgRoute}
        isOpen={createIsOpen}
        onClose={onClose}
      />
    </Card>
  );
});
