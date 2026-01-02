import React, { useState, useEffect, Fragment } from "react";
import { observer } from "mobx-react-lite";
import { Collapse, Navbar, NavLink, NavItem, Nav, Container } from "reactstrap";
import { NavLink as RouterNavLink, useParams } from "react-router-dom";

import {
  Group,
  OrganizationWithPermissions,
  PersonRole,
  UserRole,
} from "@soundbite/api";
import {
  GlobalTheme,
  Loader,
  ShowWhen,
  WidgetStore,
} from "@soundbite/widgets-react";

import { NewGroupDialog } from "../dialogs/NewGroupDialog";
import UserMenu from "../controls/UserMenu";
import SpaStore from "../../store/Spa.Store";
import { MiniFooter } from "./Footer";

export const SideNav: React.FC = observer(() => {
  const { orgRoute } = useParams<{ orgRoute: string }>();
  const [myGroups, setMyGroups] = useState<Group[] | undefined>();
  const [org, setOrg] = useState<OrganizationWithPermissions | undefined>();

  const flags = SpaStore.config.featureFlags;

  const isAdminOrMore = WidgetStore.isPersonRole(PersonRole.Admin);
  const isGlobalAdmin = WidgetStore.isUserRole(UserRole.God);

  const load = async (allowCache = true) => {
    if (!allowCache) {
      setMyGroups(undefined);
      setOrg(undefined);
    }
    const groupsPromise = WidgetStore.organizations.readMyGroupsAsync(
      orgRoute,
      allowCache
    );

    const newOrg = await WidgetStore.organizations.readOrgAsync(
      orgRoute,
      allowCache
    );
    setOrg(newOrg);
    const newGroups = await groupsPromise;
    setMyGroups(newGroups);
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line
  }, [orgRoute]);

  // Mobile menu
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const toggleCollapse = () => {
    setIsMenuOpen(!isMenuOpen);
  };
  const closeMenu = () => {
    setIsMenuOpen(false);
  };

  // New Team Dialog
  const [createTeamIsOpen, setCreateTeamOpen] = useState(false);

  const onCloseCreateTeam = () => {
    setCreateTeamOpen(false);
  };

  const onOpenCreateTeam = () => {
    closeMenu();
    setCreateTeamOpen(true);
  };

  const Discovery: React.FC = () => {
    return (
      <ShowWhen is={org != null}>
        <h1 className="navbar-heading p-0 text-muted">
          <span className="docs-normal">{org?.details.name}</span>
        </h1>
        <Nav className="mb-md-3" navbar>
          <SbNavItem
            className="sb-team-link"
            title="Feed"
            to={`/organizations/${orgRoute}/feed`}
            icon="fas fa-podcast"
          />
          <SbNavItem
            className="sb-team-link"
            title="Calendar"
            to={`/organizations/${orgRoute}/calendar`}
            icon="fas fa-calendar"
          />
          <SbNavItem
            className="sb-team-link"
            title="Scheduled"
            to={`/organizations/${orgRoute}/scheduled`}
            icon="fas fa-calendar-day"
          />
          <SbNavItem
            className="sb-team-link"
            title="History"
            to={`/organizations/${orgRoute}/history`}
            icon="fas fa-calendar-check"
          />
        </Nav>
      </ShowWhen>
    );
  };

  const GroupLink: React.FC<{ team: Group }> = (props: { team: Group }) => {
    return (
      <SbNavLink
        title={props.team.name}
        to={`/organizations/${orgRoute}/groups/${props.team.route}`}
        icon="fas fa-users"
      />
    );
  };

  const MyGroups: React.FC = () => {
    return (
      <ShowWhen is={(myGroups?.length ?? 0) > 0}>
        <h6 className="navbar-heading p-0 text-muted">
          <span className="docs-normal">Groups</span>
        </h6>
        <Loader isLoadedWhen={!!myGroups}>
          <Nav className="mb-md-3" navbar>
            {myGroups?.map((group: Group) => {
              return (
                <NavItem key={group.route} className="nav-item sb-team-link">
                  <GroupLink team={group} />
                </NavItem>
              );
            })}
          </Nav>
        </Loader>
      </ShowWhen>
    );
  };

  const Admin: React.FC = () => {
    return (
      <ShowWhen is={isAdminOrMore}>
        <h1 className="navbar-heading p-0 text-muted">
          <span className="docs-normal">Admin</span>
        </h1>
        <Nav className="mb-md-3" navbar>
          <NavItem>
            <SbNavLink
              title="Users & Groups"
              to={`/organizations/${orgRoute}/directory`}
              icon="fas fa-user-cog"
            />
            <SbNavLink
              title="Notifications"
              to={`/organizations/${orgRoute}/notifications`}
              icon="fas fa-bell"
            />
            <SbNavLink
              title="Permissions"
              to={`/organizations/${orgRoute}/permissions`}
              icon="fas fa-lock"
            />
            <SbNavLink
              title="Sessions"
              to={`/organizations/${orgRoute}/sessions/settings`}
              icon="fas fa-podcast"
            />
            <SbNavLink
              title="Reports"
              to={`/organizations/${orgRoute}/reports`}
              icon="fas fa-chart-area"
            />
            <SbNavLink
              title="Theme"
              to={`/organizations/${orgRoute}/theme`}
              icon="fas fa-palette"
            />
            <ShowWhen is={isAdminOrMore}>
              <SbNavLink
                title="Performance"
                to={`/organizations/${orgRoute}/performance`}
                icon="fas fa-flag-checkered"
              />
            </ShowWhen>
          </NavItem>
        </Nav>
      </ShowWhen>
    );
  };

  function SbNavItem(props: {
    className?: string;
    to: string;
    title: string;
    icon?: string;
  }): JSX.Element {
    return (
      <NavItem className={props.className}>
        <SbNavLink {...props} />
      </NavItem>
    );
  }

  function SbNavLink(props: {
    to: string;
    title: string;
    icon?: string;
  }): JSX.Element {
    return (
      <RouterNavLink
        activeClassName="active"
        className="nav-link"
        to={props.to}
        onClick={closeMenu}
        title={props.title}
      >
        <i className={props.icon}></i>
        <span>{props.title}</span>
      </RouterNavLink>
    );
  }

  const GlobalAdmin: React.FC = () => {
    return (
      <ShowWhen is={isGlobalAdmin}>
        <h1 className="navbar-heading p-0 text-muted">
          <span className="docs-normal">Global Admin</span>
        </h1>
        <Nav className="mb-md-3" navbar>
          <NavItem>
            <SbNavLink
              title="Azure Settings"
              to={`/organizations/${orgRoute}/azureSettings`}
              icon="fas fa-user-cog"
            />
            <SbNavLink
              title="Security Token"
              to={`/organizations/${orgRoute}/orgTokenSettings`}
              icon="fas fa-lock"
            />
          </NavItem>
        </Nav>
      </ShowWhen>
    );
  };

  const isCreateTeamAllowed = WidgetStore.isPersonRole(
    WidgetStore.organizations.currentOrg?.permissions?.minRoleToCreateTeam ??
      PersonRole.Person
  );

  const CreateGroup: React.FC = () => {
    return (
      <ShowWhen is={flags.teamCreationEnabled && isCreateTeamAllowed}>
        <Nav navbar>
          <NavItem className="sb-bottom-nav-item">
            <NavLink>
              <button
                className={
                  "btn btn-icon btn-primary w-100" +
                  ((myGroups?.length ?? 0) > 0 ? " btn-sm" : "")
                }
                onClick={onOpenCreateTeam}
                title="Create Group"
              >
                <span className="btn-inner--icon">
                  <i className="fas fa-users"></i>
                </span>
                <span className="btn-inner--text">
                  <Fragment>&nbsp;</Fragment>Create Group
                </span>
              </button>
              <NewGroupDialog
                isOpen={createTeamIsOpen}
                onClose={onCloseCreateTeam}
                orgRoute={orgRoute}
              />
            </NavLink>
          </NavItem>
        </Nav>
      </ShowWhen>
    );
  };

  return (
    <Navbar
      className="navbar-vertical fixed-left navbar-light d-print-none"
      expand="md"
      id="sidenav-main"
    >
      <Container fluid>
        <button
          className="navbar-toggler"
          type="button"
          onClick={toggleCollapse}
          title="Collapse"
        >
          <span className="navbar-toggler-icon" />
        </button>
        <RouterNavLink className="navbar-brand" to="/" title="Home">
          <img
            alt="Soundbite"
            className="navbar-brand-img"
            src={
              WidgetStore.organizations.currentOrg?.details.imageSrc ??
              GlobalTheme.current.images.logo
            }
          />
          <img
            alt="Soundbite"
            className="navbar-brand-img-mini"
            src={
              WidgetStore.organizations.currentOrg?.details.imageSrc ??
              GlobalTheme.current.images.mark
            }
          />
        </RouterNavLink>
        <Nav className="align-items-center d-md-none">
          <UserMenu />
        </Nav>
        <Collapse navbar isOpen={isMenuOpen}>
          <Discovery />
          <Loader isLoadedWhen={!!org}>
            <MyGroups />
            <Admin />
            <GlobalAdmin />
            <CreateGroup />
            <MiniFooter />
          </Loader>
        </Collapse>
      </Container>
    </Navbar>
  );
});
