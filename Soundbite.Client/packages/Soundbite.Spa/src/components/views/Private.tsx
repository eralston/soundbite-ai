import React, { Component } from "react";
import { Container } from "reactstrap";

import { SideNav } from "../nav/SideNav";
import { TopNav } from "../nav/TopNav";
import { Header } from "../nav/Header";
import { Footer } from "../nav/Footer";
import { EnvironmentSettings, Platform } from "../../sdk/EnvironmentSettings";
import Background, { BackgroundMode } from "../controls/Background";
import { ThemeBanner } from "@soundbite/widgets-react";

export class Private extends Component {
  render() {
    Background.apply(BackgroundMode.Default);
    return (
      <React.Fragment>
        <ThemeBanner />
        <div className="sb-private-view">
          {EnvironmentSettings.instance.platform === Platform.Web && (
            <SideNav />
          )}
          <div className="main-content">
            <TopNav />
            <Header />
            <Container className="sb-navigation-container">
              {this.props.children}
              <Footer />
            </Container>
          </div>
        </div>
      </React.Fragment>
    );
  }
}
