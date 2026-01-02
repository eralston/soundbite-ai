import React, { Component } from "react";
import { Alert } from "reactstrap";
import { PublicError } from "@soundbite/api";

import { SupportLink } from "..";

interface IProps {
  error?: PublicError;
}

interface IState {
  isShowDetails: boolean;
}

/**
 * React component for managing a singleton error dialog
 * If the error is an instance of PublicError, it will show it to the user in the dialog
 * Otherwise, it will console.error with a helpful header on the message
 * */
export class ErrorCtrl extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);
    this.state = { isShowDetails: false };
  }

  showDetails() {
    this.setState({ isShowDetails: true });
  }

  render() {
    if (!this.props.error) return <React.Fragment />;

    return (
      <div className="sb-error">
        <Alert color="danger" className="bg-gradient-danger">
          <div className="alert-danger-contents">
            <h3>Apologies</h3>
            <p>{this.props.error.message}</p>
            <p>
              Please try again. If that doesn't work,{" "}
              <SupportLink error={this.props.error}>
                Contact Support
              </SupportLink>
              .
            </p>
          </div>
        </Alert>
      </div>
    );
  }
}
