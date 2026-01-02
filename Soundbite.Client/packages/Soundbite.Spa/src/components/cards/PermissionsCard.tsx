import { observer } from "mobx-react-lite";
import React, { Fragment, useState } from "react";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import {
  Button,
  Card,
  CardFooter,
  CardHeader,
  Col,
  Row,
  Nav,
  NavItem,
  NavLink,
} from "reactstrap";

import {
  MemberRole,
  OrganizationsService,
  OrgPermissions,
  PersonRole,
} from "@soundbite/api";
import { ErrorDlg, Loader, WidgetStore } from "@soundbite/widgets-react";
import classnames from "classnames";

interface PersonPermissionRowProps {
  title: string;
  defaultValue: PersonRole;
  onChange: (newRole: PersonRole) => void;
}

/** A row that provides a view into a selected PersonRole */
const PersonPermissionRow: React.FC<PersonPermissionRowProps> = observer(
  (props: PersonPermissionRowProps) => {
    const createTab = (
      title: string,
      value: PersonRole,
      onClick: () => void,
      currentMode: PersonRole
    ) => {
      const className = classnames(
        { active: value === currentMode },
        "sb-tab-btn"
      );

      return (
        <NavItem>
          <NavLink
            role="tab"
            aria-selected={value === currentMode}
            className={className}
            onClick={onClick}
          >
            {title}
          </NavLink>
        </NavItem>
      );
    };

    const Tabs: React.FC = () => {
      return (
        <div className="d-flex flex-md-row flex-row mt-2">
          <Nav className="sb-3-tab flex-column flex-md-row" role="tablist">
            {createTab(
              "Guest",
              PersonRole.Guest,
              () => {
                props.onChange(PersonRole.Guest);
              },
              props.defaultValue as PersonRole
            )}
            {createTab(
              "Person",
              PersonRole.Person,
              () => {
                props.onChange(PersonRole.Person);
              },
              props.defaultValue as PersonRole
            )}
            {createTab(
              "Admin",
              PersonRole.Admin,
              () => {
                props.onChange(PersonRole.Admin);
              },
              props.defaultValue as PersonRole
            )}
          </Nav>
        </div>
      );
    };

    return (
      <tr>
        <td>{props.title}</td>
        <td className="d-flex justify-content-end pr-4">
          <Tabs />
        </td>
      </tr>
    );
  }
);

interface MemberPermissionRowProps {
  title: string;
  defaultValue: MemberRole;
  onChange: (newRole: MemberRole) => void;
}

/** A row that provides a view into a selected MemberRole */
const MemberPermissionRow: React.FC<MemberPermissionRowProps> = observer(
  (props: MemberPermissionRowProps) => {
    const createTab = (
      title: string,
      value: MemberRole,
      onClick: () => void,
      currentMode: MemberRole
    ) => {
      const className = classnames(
        { active: value === currentMode },
        "sb-tab-btn"
      );

      return (
        <NavItem>
          <NavLink
            role="tab"
            aria-selected={value === currentMode}
            className={className}
            onClick={onClick}
          >
            {title}
          </NavLink>
        </NavItem>
      );
    };

    const Tabs: React.FC = () => {
      return (
        <div className="d-flex flex-md-row flex-row mt-2">
          <Nav className="sb-3-tab flex-column flex-md-row" role="tablist">
            {createTab(
              "Member",
              MemberRole.Member,
              () => {
                props.onChange(MemberRole.Member);
              },
              props.defaultValue
            )}
            {createTab(
              "Owner",
              MemberRole.Owner,
              () => {
                props.onChange(MemberRole.Owner);
              },
              props.defaultValue
            )}
          </Nav>
        </div>
      );
    };

    return (
      <tr>
        <td>{props.title}</td>
        <td className="d-flex justify-content-end pr-4">
          <Tabs />
        </td>
      </tr>
    );
  }
);

/** Configuration card for org permissions */
export const PermissionsCard: React.FC = observer(() => {
  // Params
  let { orgRoute } = useParams<{ orgRoute: string }>();

  const [settings, setSettings] = useState<OrgPermissions | undefined>(
    undefined
  );
  const [isChanged, setIsChanged] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    const load = async () => {
      const initialSettings = await OrganizationsService.readPermissionsAsync(
        orgRoute
      );
      setSettings(initialSettings);
      setIsChanged(false);
    };
    load();
  }, [orgRoute]);

  const onSave = async () => {
    // Must have settings; otherwise, just ignore
    if (settings == null) {
      return;
    }

    setIsSaving(true);
    try {
      const newSettings = await OrganizationsService.updatePermissionsAsync(
        orgRoute,
        settings
      );
      setSettings(newSettings);
      setIsChanged(false);
    } catch (err) {
      ErrorDlg.show(err, "Error Saving Notification Settings");
    }
    setIsSaving(false);
  };

  const onChangeCreateSb = (newRole: PersonRole) => {
    if (settings == null) {
      return;
    }
    setSettings({ ...settings, minRoleToCreateSession: newRole });
    setIsChanged(true);
  };

  const onChangeAudience = (newRole: PersonRole) => {
    if (settings == null) {
      return;
    }
    setSettings({ ...settings, minRoleForAudience: newRole });
    setIsChanged(true);
  };

  const onChangeOrgInvite = (newRole: PersonRole) => {
    if (settings == null) {
      return;
    }
    setSettings({ ...settings, minRoleForOrgInvite: newRole });
    setIsChanged(true);
  };

  const onChangeCreatePublicSb = (newRole: PersonRole) => {
    if (settings == null) {
      return;
    }
    setSettings({ ...settings, minRoleToCreatePublic: newRole });
    setIsChanged(true);
  };

  const onChangeCreateTeam = (newRole: PersonRole) => {
    if (settings == null) {
      return;
    }
    setSettings({ ...settings, minRoleToCreateTeam: newRole });
    setIsChanged(true);
  };

  const onChangeTeamInvite = (newRole: MemberRole) => {
    if (settings == null) {
      return;
    }
    setSettings({ ...settings, minRoleForTeamInvite: newRole });
    setIsChanged(true);
  };

  const onChangeCreateSbTeam = (newRole: MemberRole) => {
    if (settings == null) {
      return;
    }
    setSettings({ ...settings, minRoleToCreateTeamSession: newRole });
    setIsChanged(true);
  };

  const onChangeCreatePublicSbTeam = (newRole: MemberRole) => {
    if (settings == null) {
      return;
    }
    setSettings({ ...settings, minRoleToCreateTeamPublic: newRole });
    setIsChanged(true);
  };

  return (
    <Card>
      <Loader
        isLoadedWhen={!!WidgetStore.organizations.currentOrg && !!settings}
        style={{ minHeight: "12rem" }}
      >
        <CardHeader className="border-0">
          <Row className="align-items-center">
            <Col md="11">
              <h1 className="mb-0">
                <i className="fas fa-lock d-none d-sm-inline"></i>
                {WidgetStore.organizations.currentOrg?.details.name} Permissions
              </h1>
              <div className="mt-0 text-muted">
                Configure what level of access is required to perform key
                actions
              </div>
            </Col>
            <Col md="1" className="text-right"></Col>
          </Row>
        </CardHeader>
        <div className="table-responsive">
          <table className="table align-items-center sb-max-width-table sb-dual-column-table">
            <thead className="thead-light">
              <tr>
                <th scope="col" className="sort" data-sort="name">
                  Permission Type
                </th>
                <th scope="col" className="sort" data-sort="action"></th>
              </tr>
            </thead>
            <PersonPermissionRow
              title="Create Session for Organization"
              defaultValue={
                settings?.minRoleToCreateSession ?? PersonRole.Person
              }
              onChange={onChangeCreateSb}
            />
            <PersonPermissionRow
              title="Create Public Session for Organization"
              defaultValue={
                settings?.minRoleToCreatePublic ?? PersonRole.Person
              }
              onChange={onChangeCreatePublicSb}
            />
            <PersonPermissionRow
              title="Invite to Organization"
              defaultValue={settings?.minRoleForOrgInvite ?? PersonRole.Person}
              onChange={onChangeOrgInvite}
            />
            <PersonPermissionRow
              title="Show Audience for Sessions"
              defaultValue={settings?.minRoleForAudience ?? PersonRole.Person}
              onChange={onChangeAudience}
            />
            <PersonPermissionRow
              title="Create Group"
              defaultValue={settings?.minRoleToCreateTeam ?? PersonRole.Person}
              onChange={onChangeCreateTeam}
            />
            <MemberPermissionRow
              title="Invite to Group"
              defaultValue={settings?.minRoleForTeamInvite ?? MemberRole.Member}
              onChange={onChangeTeamInvite}
            />
            <MemberPermissionRow
              title="Create Session for Group"
              defaultValue={
                settings?.minRoleToCreateTeamSession ?? MemberRole.Member
              }
              onChange={onChangeCreateSbTeam}
            />
            <MemberPermissionRow
              title="Create Public Session for Group"
              defaultValue={
                settings?.minRoleToCreateTeamPublic ?? MemberRole.Owner
              }
              onChange={onChangeCreatePublicSbTeam}
            />
          </table>
        </div>
        <CardFooter className="d-flex justify-content-end">
          <Button
            color="primary"
            className="btn-icon btn-3"
            onClick={onSave}
            disabled={!isChanged && !isSaving}
            title="Save"
          >
            <span className="btn-inner--icon">
              <i className="fas fa-save"></i>
            </span>
            <span className="btn-inner--text">
              <Fragment>&nbsp;</Fragment>Save
            </span>
          </Button>
        </CardFooter>
      </Loader>
    </Card>
  );
});
