import React, { Component } from "react";
import { Modal, ModalHeader, ModalBody, ModalFooter, Button } from "reactstrap";

interface ISimpleDlgProps {
  isOpen?: boolean;
  onClose: () => void;
  title?: string;
}

/**
 * React component for managing a singleton error dialog
 * If the error is an instance of PublicError, it will show it to the user in the dialog
 * Otherwise, it will console.error with a helpful header on the message
 * */
export default class SimpleDlg extends Component<ISimpleDlgProps> {
  constructor(props: ISimpleDlgProps) {
    super(props);
    this.close = this.close.bind(this);
  }

  close() {
    this.props.onClose();
  }

  render() {
    return (
      <Modal isOpen={this.props.isOpen} toggle={this.close} backdrop="static">
        <ModalHeader
          toggle={this.close.bind(this)}
          className="bg-gradient-primary"
        >
          {this.props.title}
        </ModalHeader>
        <ModalBody>{this.props.children}</ModalBody>
        <ModalFooter>
          <Button
            className="btn btn-secondary"
            onClick={this.close}
            title="Close"
          >
            Close
          </Button>
        </ModalFooter>
      </Modal>
    );
  }
}
