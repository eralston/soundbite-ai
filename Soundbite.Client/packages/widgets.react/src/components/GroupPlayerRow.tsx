import React, { Component } from "react";
import { Row, Col } from "reactstrap";

import { ClipWithContributor, SoundbiteApiConfig, Utils } from "@soundbite/api";

interface GroupPlayerRowProps {
  clip: ClipWithContributor;
}

/** Wraps a set of children with a row describing the given clip */
export class GroupPlayerRow extends Component<GroupPlayerRowProps> {
  render() {
    const avatarlUrl =
      this.props.clip.contributor.imageSrc || SoundbiteApiConfig.imgAvatarUrl;
    const displayName = Utils.userDisplay(this.props.clip.contributor);
    return (
      <li className="list-group-item px-0">
        <Row className="align-items-center">
          <Col xs="2" className="d-none d-sm-block">
            <span className="avatar rounded-circle">
              <img alt="Avatar" src={avatarlUrl} />
            </span>
          </Col>
          <Col xs="4" className="ml--2">
            <h4 className="mb-0">{displayName}</h4>
          </Col>
          <Col xs="6">{this.props.children}</Col>
        </Row>
      </li>
    );
  }
}
