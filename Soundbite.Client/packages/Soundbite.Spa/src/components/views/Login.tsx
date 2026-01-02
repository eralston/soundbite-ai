import React, { Component } from "react";
import { Card, CardHeader } from "reactstrap";

import Public from "./Public";

interface ILoginProps {
  prompt?: string;
  isLoaded: boolean;
}

export default class Login extends Component<ILoginProps> {
  constructor(props: ILoginProps) {
    super(props);
    this.state = { isLoading: false };
  }

  render() {
    return (
      <Public>
        <Card>
          <CardHeader>
            <div className="text-muted text-center mt-2 mb-3">
              <small>{this.props.prompt || "Single Sign-On (SSO)"} </small>
            </div>
            <div className="btn-wrapper text-center py-1">
              {this.props.children}
            </div>
          </CardHeader>
        </Card>
      </Public>
    );
  }
}
