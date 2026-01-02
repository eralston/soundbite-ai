import React from "react";
import { Container, Navbar, Nav } from "reactstrap";

import { GroupTabs } from "../controls/GroupTabs";
import OrgTabs from "../controls/OrgTabs";
import { Route, Switch } from "react-router";
import { DirectoryTabs } from "../controls/DirectoryTabs";
import UserMenu from "../controls/UserMenu";

export const TopNav: React.FC = () => {
  return (
    <Navbar className="navbar-top navbar-dark bg-gradient-brand" expand="md">
      <Container>
        <Nav className="navbar-nav align-items-center">
          <Switch>
            <Route path="/organizations/:orgRoute/groups/:groupRoute">
              <GroupTabs />
            </Route>
            <Route path="/organizations/:orgRoute/directory">
              <DirectoryTabs />
            </Route>
            <Route path="/organizations/:orgRoute/notifications"></Route>
            <Route path="/organizations/:orgRoute/permissions"></Route>
            <Route path="/organizations/:orgRoute/theme"></Route>
            <Route path="/organizations/:orgRoute/reports"></Route>
            <Route path="/organizations/:orgRoute/performance">
              <div />
            </Route>
            <Route path="/organizations/:orgRoute/">
              <OrgTabs />
            </Route>
          </Switch>
        </Nav>
        <ul className="navbar-nav align-items-center ml-auto d-none d-md-flex">
          <UserMenu />
        </ul>
      </Container>
    </Navbar>
  );
};
