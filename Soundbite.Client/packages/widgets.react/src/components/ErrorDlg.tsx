import { PublicError } from "@soundbite/api";
import React, { Component } from "react";
import { Modal, ModalBody, ModalFooter, ModalHeader } from "reactstrap";

import { SupportLink } from "../components/controls";

interface IState {
  isOpen: boolean;
  error?: PublicError;
}

/**
 * React component for managing a singleton error dialog
 * If the error is an instance of PublicError, it will show it to the user in the dialog
 * Otherwise, it will console.error with a helpful header on the message
 * */
export class ErrorDlg extends Component<{}, IState> {
  // Static

  static instance: ErrorDlg;

  static show(error: unknown, name?: string, message?: string) {
    if (!(error instanceof PublicError))
      error = new PublicError(error, name, message);

    this.instance.setState({
      isOpen: true,
      error: error as unknown as PublicError,
    });
  }

  // Instance

  onClose?: (error?: Error) => void;

  constructor(props: any) {
    super(props);

    if (ErrorDlg.instance) console.error("More than one instance of ErrorDlg");

    ErrorDlg.instance = this;
  }

  close() {
    if (this.onClose) this.onClose(this.state.error);

    this.setState({ isOpen: false, error: undefined });
  }

  render() {
    if (!this.state || !this.state.error) return <React.Fragment />;
    return (
      <Modal
        isOpen={this.state.isOpen}
        toggle={this.close.bind(this)}
        backdrop="static"
        modalClassName="sb-error-dlg"
      >
        <ModalHeader
          toggle={this.close.bind(this)}
          className="bg-gradient-danger"
        >
          {this.state.error.name}
        </ModalHeader>
        <ModalBody>
          <h1>Apologies</h1>
          <p>{this.state.error.message}</p>
          <p>Please try again. If that doesn't work, contact support.</p>
        </ModalBody>
        <ModalFooter>
          <SupportLink error={this.state.error}>Contact Support</SupportLink>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={this.close.bind(this)}
          >
            Close
          </button>
        </ModalFooter>
      </Modal>
    );
  }
}
