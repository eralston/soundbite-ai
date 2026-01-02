import React, { Component } from "react";
import { Modal, ModalHeader, ModalBody, ModalFooter } from "reactstrap";
import { PublicError } from "@soundbite/api";
import { ErrorCtrl, EmailPicker, Loader } from "@soundbite/widgets-react";

interface IProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmitAsync: (emails: string[]) => Promise<void>;
}

interface IState {
  isLoading: boolean;
  emails: string[];
  error?: PublicError;
}

export class InviteDlg extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);
    this.close = this.close.bind(this);
    this.submit = this.submit.bind(this);
    this.onEmailsChange = this.onEmailsChange.bind(this);

    this.state = this.defaultState;
  }

  get defaultState() {
    return { emails: [], isLoading: false, error: undefined };
  }

  get isLoading() {
    return this.state.isLoading;
  }

  componentDidMount() {
    this.setState(this.defaultState);
  }

  onEmailsChange(emails: string[]) {
    this.setState({ emails: emails });
  }

  close() {
    this.props.onClose();
  }

  async submit() {
    try {
      this.setState({ isLoading: true, error: undefined });
      await this.props.onSubmitAsync(this.state.emails);
      this.close();
    } catch (error: any) {
      let err: PublicError = error;
      if (!(err instanceof PublicError)) err = new PublicError(error);
      this.setState({ error: err });
    } finally {
      this.setState({ isLoading: false });
    }
  }

  get invalid() {
    return this.state.emails.length === 0;
  }

  render() {
    return (
      <Modal isOpen={this.props.isOpen} toggle={this.close} backdrop="static">
        <ModalHeader
          toggle={this.close.bind(this)}
          className="bg-gradient-primary"
        >
          Share Soundbite With a Friend
        </ModalHeader>
        <ModalBody>
          <p className="text-muted">
            Send an early invite to one or more people via e-mail to help them
            get started with Soundbite
          </p>
          <EmailPicker
            onChange={this.onEmailsChange}
            disabled={this.isLoading}
          />
          <small className="text-muted">
            They will <strong>not</strong> be invited to your organization
          </small>
        </ModalBody>
        <ModalFooter>
          <ErrorCtrl error={this.state.error} />
          <Loader isLoadedWhen={!this.isLoading}>
            <button
              className="btn btn-secondary"
              onClick={this.close}
              disabled={this.isLoading}
            >
              Close
            </button>
            <button
              className="btn btn-primary"
              onClick={this.submit}
              disabled={this.invalid || this.isLoading}
            >
              Send
            </button>
          </Loader>
        </ModalFooter>
      </Modal>
    );
  }
}

interface IReferralBtnProps {
  onSubmitAsync: (emails: string[]) => Promise<void>;
  className?: string;
}

interface IReferralBtnState {
  isDialogOpen: boolean;
}

export default class InviteBtn extends Component<
  IReferralBtnProps,
  IReferralBtnState
> {
  constructor(props: IReferralBtnProps) {
    super(props);
    this.close = this.close.bind(this);
    this.open = this.open.bind(this);
    this.onSubmitAsync = this.onSubmitAsync.bind(this);

    this.state = {
      isDialogOpen: false,
    };
  }

  async onSubmitAsync(emails: string[]) {
    await this.props.onSubmitAsync(emails);
  }

  open() {
    this.setState({ isDialogOpen: true });
  }

  close() {
    this.setState({ isDialogOpen: false });
  }

  render() {
    return (
      <React.Fragment>
        <span
          className={this.props.className}
          onClick={this.open}
          title="Share"
        >
          {this.props.children}
        </span>
        <InviteDlg
          isOpen={this.state.isDialogOpen}
          onClose={this.close}
          onSubmitAsync={this.onSubmitAsync}
        />
      </React.Fragment>
    );
  }
}
