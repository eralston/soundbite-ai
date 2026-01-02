import React, { Component } from "react";
import { Container, Row, Col } from "reactstrap";

import { ContactBtn } from "../controls/ContactBtn";
import Background, { BackgroundMode } from "../controls/Background";
import { GlobalTheme } from "@soundbite/widgets-react";
import SpaStore from "../../store/Spa.Store";

interface IPublicProps {
  cardWidthLg?: string; // This could be safer by making it a column props
  cardWidthMd?: string; // This could be safer by making it a column props
}

export default class Public extends Component<IPublicProps> {
  constructor(props: IPublicProps) {
    super(props);
    this.onLogout = this.onLogout.bind(this);
  }

  async onLogout() {
    await SpaStore.logout();
  }

  render() {
    Background.apply(BackgroundMode.Dark);
    const cardMd = this.props.cardWidthMd || "8";
    const cardLg = this.props.cardWidthLg || "6";

    return (
      <div className="sb-default-background">
        <div className="main-content">
          <div className="header pt-0 pb-5 sb-progress-background sb-progress-background-go">
            <Container>
              <div className="header-body text-center mb-7">
                <Row className="justify-content-center">
                  <Col md="8" lg="6">
                    <img
                      className="sb-header-logo"
                      src={GlobalTheme.current.images.logoAlt}
                      alt="Soundbite"
                    />
                    <p className="text-lead text-light">
                      A Better Way to Communicate
                    </p>
                  </Col>
                </Row>
              </div>
            </Container>
            <div className="separator separator-bottom separator-skew zindex-100">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                preserveAspectRatio="none"
                version="1.1"
                viewBox="0 0 2560 100"
                x="0"
                y="0"
              >
                <polygon
                  className="fill-default"
                  points="2560 0 2560 100 0 100"
                ></polygon>
              </svg>
            </div>
          </div>
          <Container className="mt--8 pb-5">
            <Row className="justify-content-center">
              <Col md={cardMd} lg={cardLg}>
                {this.props.children}
                <Row className="mt-3">
                  <Col xs="6">
                    <a
                      href="https://soundbite.ai/"
                      className="text-dark"
                      target="_blank"
                      rel="noopener noreferrer"
                    >
                      <small>soundbite.ai</small>
                    </a>
                    <small>
                      <ContactBtn textOnly={true} className="text-dark ml-2" />
                    </small>
                  </Col>
                  <Col xs="6" className="text-right">
                    <small>
                      <a
                        onClick={this.onLogout}
                        href="#logout"
                        className="text-dark"
                      >
                        Logout
                      </a>
                    </small>
                  </Col>
                </Row>
              </Col>
            </Row>
          </Container>
        </div>
      </div>
    );
  }
}
