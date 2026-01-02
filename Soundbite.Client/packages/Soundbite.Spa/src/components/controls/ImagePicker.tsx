import { SoundbiteApiConfig } from "@soundbite/api";
import React, { Component } from "react";
import { Row, Col } from "reactstrap";

interface IProps {
  title: string;
  initialAvatarUrl?: string;
}

interface IState {
  avatarUrl: string;
}

export class ImagePicker extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);

    let initialAvatarUrl = props.initialAvatarUrl;
    if (!initialAvatarUrl) initialAvatarUrl = SoundbiteApiConfig.imgAvatarUrl;

    this.state = { avatarUrl: initialAvatarUrl };
  }

  render() {
    return (
      <div className="sb-imagepicker">
        <label className="form-control-label">{this.props.title}</label>
        <Row>
          <Col>
            <Row className="mb-2">
              <Col>
                <button
                  className="btn btn-icon btn-primary w-100"
                  type="button"
                >
                  <span className="btn-inner--icon">
                    <i className="fas fa-file-upload"></i>
                  </span>
                  <span className="btn-inner--text">Upload</span>
                </button>
              </Col>
            </Row>
            <Row>
              <Col>
                <button
                  className="btn btn-icon btn-primary w-100"
                  type="button"
                >
                  <span className="btn-inner--icon">
                    <i className="fas fa-trash"></i>
                  </span>
                  <span className="btn-inner--text">Remove</span>
                </button>
              </Col>
            </Row>
          </Col>
          <Col className="col">
            <img
              src={this.state.avatarUrl}
              className="rounded-circle"
              alt="Avatar"
            />
          </Col>
        </Row>
      </div>
    );
  }
}
