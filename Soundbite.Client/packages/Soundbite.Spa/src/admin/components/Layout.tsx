import React, { useEffect, useState } from "react";
import { observer } from "mobx-react-lite";
import { Container, Navbar, Nav, NavItem, Collapse } from "reactstrap";
import { NavLink as RouterNavLink } from "react-router-dom";

import { GlobalTheme } from "@soundbite/widgets-react";

import { Footer } from "../../components/nav/Footer";
import { TopNav } from "../../components/nav/TopNav";
import { Header } from "../../components/nav/Header";
import Background, {
  BackgroundMode,
} from "../../components/controls/Background";
import UserMenu from "../../components/controls/UserMenu";

//////////[ Component Properties ]//////////////////////////////////////////////////////////////////
interface IProps {
  children?: React.ReactNode;
}

//////////[ UI Methods ]////////////////////////////////////////////////////////////////////////////

function SideNav() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  const toggleCollapse = () => {
    setIsMenuOpen(!isMenuOpen);
  };
  const closeMenu = () => {
    setIsMenuOpen(false);
  };

  return (
    <Navbar
      className="navbar-vertical fixed-left navbar-light"
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
        <RouterNavLink className="navbar-brand" to="/">
          <img
            alt="Soundbite"
            className="navbar-brand-img"
            src={GlobalTheme.current.images.logo}
          />
          <img
            alt="Soundbite"
            className="navbar-brand-img-mini"
            src={GlobalTheme.current.images.mark}
          />
        </RouterNavLink>
        <Nav className="align-items-center d-md-none">
          <UserMenu />
        </Nav>
        <Collapse navbar isOpen={isMenuOpen}>
          <h6 className="navbar-heading p-0 text-muted">
            <span className="docs-normal">Tenant Administration</span>
          </h6>
          <Nav className="mb-md-3" navbar>
            <NavItem className="sb-team-link">
              <RouterNavLink
                activeClassName="active"
                className="nav-link"
                to={`/admin`}
                onClick={closeMenu}
                exact={true}
                title="Admin Home"
              >
                <i className="fas fa-home"></i>
                <span className="nav-link-text">Home</span>
              </RouterNavLink>
            </NavItem>
            <NavItem className="sb-team-link">
              <RouterNavLink
                activeClassName="active"
                className="nav-link"
                to={`/admin/orgs`}
                onClick={closeMenu}
                title="Organizations"
              >
                <i className="fas fa-sitemap"></i>
                <span className="nav-link-text">Organizations</span>
              </RouterNavLink>
            </NavItem>
          </Nav>
        </Collapse>
      </Container>
    </Navbar>
  );
}

//////////[ Component Definition ]//////////////////////////////////////////////////////////////////

const Component: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  async function initialize() {}

  // Calling useEffect with [] as the second parameter ensures one-time execution
  useEffect(() => {
    initialize();
  }, []);

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  Background.apply(BackgroundMode.Default);
  return (
    <div className="sb-private-view">
      <SideNav />
      <div className="main-content">
        <TopNav />
        <Header />
        <Container className="sb-navigation-container">
          {props.children}
          <Footer />
        </Container>
      </div>
    </div>
  );
});

export default Component;
