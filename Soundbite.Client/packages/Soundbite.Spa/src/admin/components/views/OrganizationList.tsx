import React, { Fragment, useEffect, useState } from "react";
import { observer } from "mobx-react-lite";
import { Card, Row, CardHeader, Col } from "reactstrap";
import { useHistory } from "react-router";
import {
  PeopleService,
  OrganizationExtended,
  Person,
  PersonRole,
} from "@soundbite/api";
import { Loader, ShowWhen, WidgetStore } from "@soundbite/widgets-react";

import AdminStore from "../../store/AdminStore";
import Layout from "../Layout";
import { DeleteOrgDialog } from "../../../components/dialogs/DeleteOrgDlg";
import adminStore from "../../store/AdminStore";

//////////[ Component Properties ]//////////////////////////////////////////////////////////////////

interface IProps {
  children?: React.ReactNode;
}

//////////[ Component Definition ]//////////////////////////////////////////////////////////////////

const Component: React.FC<IProps> = observer((props: IProps) => {
  //////////[ State Management ]////////////////////////////////////////////////////////////////////

  let [isLoaded, setIsLoaded] = useState(false);
  let [showArchived, setShowArchived] = useState(false);
  let [items, setItems] = useState<OrganizationExtended[]>([]);

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  const history = useHistory();

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onCreateItem(): void {
    // Navigate to the item editor for the item.
    history.push(`/admin/orgs/create`);
  }

  async function onDeleteItem(item: OrganizationExtended): Promise<void> {
    DeleteOrgDialog.open(item, async () => {
      if (item) {
        await AdminStore.orgStore.deleteOrg(item.route);
        setItems(adminStore.orgStore.orgs);
      }
    });
  }

  function onEditItem(item: OrganizationExtended): void {
    if (item) {
      // Navigate to the item editor for the item.
      history.push(`/admin/orgs/${item.route}`);
    }
  }

  async function onRestoreItem(item: OrganizationExtended): Promise<void> {
    if (item) {
      await AdminStore.orgStore.restoreOrg(item.route);
      setItems(adminStore.orgStore.orgsArchived);
    }
  }

  async function onToggleShowArchived() {
    if (!showArchived) {
      setIsLoaded(false);
      const archivedItems = await adminStore.orgStore.loadArchivedOrgs();
      setItems(archivedItems);
      setIsLoaded(true);
    } else {
      setItems(adminStore.orgStore.orgs);
    }
    setShowArchived(!showArchived);
  }

  async function ensureAdmin(org: OrganizationExtended) {
    const token = WidgetStore.users.currentUser?.route;

    if (token === undefined) {
      alert("User information not loaded");
      return;
    }

    const inviteResult = await WidgetStore.people.invitePersonAsync(org.route, [
      { token },
    ]);
    const { personRoute } = inviteResult[0];
    await PeopleService.updateAsync(org.route, personRoute, {
      personRole: PersonRole.Admin,
    } as Person);
    alert(`Invited to ${org.name} as admin`);
  }

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  async function initialize() {
    var items = await AdminStore.orgStore.loadOrgs();
    setIsLoaded(true);
    setItems(items);
  }

  // Calling useEffect with [] as the second parameter ensures one-time execution
  useEffect(() => {
    initialize();
  }, []);

  const archiveTxt = showArchived ? "Show Active" : "Show Archived";

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////
  return (
    <Layout>
      <Card>
        <CardHeader className="bg-transparent">
          <Row className="align-items-center">
            <Col>
              <h1 className="mb-0">
                Manage{showArchived ? " Archived " : " "}Organizations
              </h1>
            </Col>
            <Col className="text-right sb-stackable-btn">
              <button
                className="btn btn-icon btn-primary btn-sm"
                type="button"
                onClick={onToggleShowArchived}
                title={archiveTxt}
              >
                <span className="btn-inner--icon">
                  <i
                    className={showArchived ? "far fa-eye-slash" : "far fa-eye"}
                  ></i>
                </span>
                <span className="btn-inner--text">
                  <Fragment>&nbsp;</Fragment>
                  {archiveTxt}
                </span>
              </button>

              <button
                className="btn btn-icon btn-primary btn-sm"
                type="button"
                onClick={onCreateItem}
                title="Create Organization"
              >
                <span className="btn-inner--icon">
                  <i className="fas fa-plus"></i>
                </span>
                <span className="btn-inner--text">
                  <Fragment>&nbsp;</Fragment>Create Organization
                </span>
              </button>
            </Col>
          </Row>
        </CardHeader>
        <div className="table-responsive">
          <Loader isLoadedWhen={isLoaded}>
            <ShowWhen is={items.length > 0}>
              <table className="table align-items-center">
                <thead className="thead-light">
                  <tr>
                    <th scope="col" className="sort" data-sort="name">
                      Image
                    </th>
                    <th scope="col" className="sort" data-sort="name">
                      Name
                    </th>
                    <th scope="col" className="sort" data-sort="action"></th>
                  </tr>
                </thead>
                <tbody className="list">
                  {items.map((org: OrganizationExtended) => {
                    return (
                      <tr key={org.route}>
                        <th style={{ width: "100px" }}>
                          <span className="avatar rounded-circle mr-3 d-none d-sm-block">
                            <img
                              src={
                                org.imageSrc || "/img/default-avatar-group.png"
                              }
                              alt="Organization Logo"
                            />
                          </span>
                        </th>
                        <td>{org.name}</td>
                        <td className="text-right">
                          <ShowWhen is={!showArchived}>
                            <button
                              className="btn btn-outline-primary btn-sm"
                              type="button"
                              onClick={() => ensureAdmin(org)}
                              title="Ensure Admin"
                            >
                              <span className="btn-inner--icon">
                                <i className="fas fa-user-plus"></i>
                              </span>
                            </button>
                          </ShowWhen>
                          <ShowWhen is={!showArchived}>
                            <button
                              className="btn btn-outline-primary btn-sm"
                              type="button"
                              title="Edit Organization"
                              onClick={() => onEditItem(org)}
                            >
                              <span className="btn-inner--icon">
                                <i className="fas fa-edit"></i>
                              </span>
                            </button>
                          </ShowWhen>
                          <ShowWhen is={!showArchived}>
                            <button
                              className="btn btn-outline-primary btn-sm"
                              type="button"
                              title="Delete Organization"
                              onClick={() => onDeleteItem(org)}
                            >
                              <span className="btn-inner--icon">
                                <i className="fas fa-trash"></i>
                              </span>
                            </button>
                          </ShowWhen>
                          <ShowWhen is={showArchived}>
                            <button
                              className="btn btn-outline-primary btn-sm"
                              type="button"
                              title="Restore"
                              onClick={() => onRestoreItem(org)}
                            >
                              <span className="btn-inner--icon">
                                <i className="fas fa-trash-restore"></i>
                              </span>
                            </button>
                          </ShowWhen>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </ShowWhen>
            <ShowWhen is={items.length === 0}>
              There are no organizations to display
            </ShowWhen>
          </Loader>
        </div>
      </Card>
    </Layout>
  );
});

export default Component;
