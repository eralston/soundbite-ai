import React, { Component } from "react";
import { Modal, ModalHeader, ModalBody, ModalFooter } from "reactstrap";

interface IState<IData> {
  isOpen: boolean;
  data?: IData;
  onSubmit?: (data?: IData) => void;
  onClose?: (data?: IData) => void;
}

/** Abstract base class for a dialog that is always on the page and able to dynamically accept input to simplify its display */
export abstract class SingletonDialogBase<
  IData = any,
  IProps = any
> extends Component<IProps, IState<IData>> {
  public readonly state: IState<IData> = {
    isOpen: false,
    data: undefined,
  };

  isBig: boolean = false;
  headerClassname: string = "bg-gradient-primary";

  public close(): void {
    this.setState({ isOpen: false });
    this.state.onClose && this.state.onClose(this.state.data);
  }

  public submit(): void {
    this.setState({ isOpen: false });
    this.state.onSubmit && this.state.onSubmit(this.state.data);
  }

  public open(
    data?: IData,
    onSubmit?: (data?: IData) => void,
    onClose?: (data?: IData) => void
  ): void {
    this.setState({
      isOpen: true,
      data: data,
      onSubmit: onSubmit,
      onClose: onClose,
    });
  }

  protected abstract modalHeader(): JSX.Element;
  protected abstract modalBody(): JSX.Element;

  protected modalFooter(): JSX.Element | undefined {
    return (
      <React.Fragment>
        <button
          type="button"
          className="btn btn-outline-secondary"
          onClick={this.close.bind(this)}
          title="Close"
        >
          Close
        </button>{" "}
        <button
          type="button"
          className="btn btn-primary"
          onClick={this.submit.bind(this)}
          title="Add"
        >
          Add
        </button>
      </React.Fragment>
    );
  }

  protected renderFooter() {
    const childFooter = this.modalFooter();
    if (childFooter) {
      return <ModalFooter>{childFooter}</ModalFooter>;
    } else return <React.Fragment />;
  }

  render() {
    let modalClassName: string = this.isBig ? "sb-big-dialog" : "";

    return (
      <Modal
        isOpen={this.state.isOpen}
        toggle={this.close.bind(this)}
        className={modalClassName}
        backdrop="static"
      >
        <ModalHeader
          toggle={this.close.bind(this)}
          className={this.headerClassname}
        >
          {this.modalHeader()}
        </ModalHeader>
        <ModalBody>{this.modalBody()}</ModalBody>
        {this.renderFooter()}
      </Modal>
    );
  }
}
