/** @jsx jsx */
import { jsx } from "@emotion/react";
import { Component } from "react";
import { Modal, ModalBody, ModalHeader } from "reactstrap";

import { SessionDetails, Utils } from "@soundbite/api";

import { DialogHash } from "../modules/HashHelper";
import { WidgetStore } from "../store/WidgetStore";
import { PlayerWidget } from "../widgets/PlayerWidget";
import { ErrorDlg } from "./ErrorDlg";
import { Loader } from "./controls";
import { IMediaPlayerContext } from "./video/context/IMediaPlayerContext";

export interface IPlayDlgProps {
  orgRoute?: string;
  currentUserRoute?: string;
  sessionRoute?: string;
  openImmediately?: boolean;
  onClose?: (session?: SessionDetails) => void;
}

export interface IPlayDlgState {
  session?: SessionDetails;
  isOpen: boolean;
  hasBeenClosed: boolean;
  isVideo: boolean;
}

export class PlayDlg extends Component<IPlayDlgProps, IPlayDlgState> {
  constructor(props: IPlayDlgProps) {
    super(props);

    this.state = {
      session: undefined,
      isOpen: false,
      hasBeenClosed: false,
      isVideo: false,
    };

    this.open = this.open.bind(this);
    this.onClose = this.onClose.bind(this);
    this.onContext = this.onContext.bind(this);
  }

  componentDidMount() {
    if (
      this.props.openImmediately &&
      this.props.sessionRoute !== undefined &&
      !this.state.hasBeenClosed
    ) {
      this.open();
    }
  }

  async open() {
    try {
      // Validate
      if (!this.props.sessionRoute) {
        this.setState({ session: undefined, isOpen: false });
        return;
      }

      Utils.setHash(`${DialogHash.Play}=${this.props.sessionRoute}`);

      const orgRoute = this.props.orgRoute;
      if (!orgRoute)
        throw new Error("Must have initialized organization to load session");

      // Load
      this.setState({ session: undefined, isOpen: true });

      const session: SessionDetails =
        await WidgetStore.sessions.readSessionDetailsAsync(
          orgRoute,
          this.props.sessionRoute
        );

      // Set returned value
      this.setState({ session });
    } catch (err: any) {
      this.setState({ isOpen: false });
      ErrorDlg.show(
        err,
        "Error Loading Soundbite",
        "Likely could not make contact with server"
      );
    }
  }

  onClose() {
    Utils.setHash();
    this.setState({ isOpen: false, hasBeenClosed: true });
    if (this.props.onClose) this.props.onClose(this.state.session);
  }

  getTitle() {
    if (this.state.session === undefined) return "Loading Soundbite Session...";

    return this.state.session.name;
  }

  getSubtitle() {
    if (this.state.session === undefined) return <span> </span>;

    return (
      <span className="sb-modal-header-subtitle">
        {Utils.formatRelativeDate(this.state.session.publish)}
      </span>
    );
  }

  onContext(context: IMediaPlayerContext) {
    this.setState({ isVideo: context.isVideo });
  }

  render() {
    let isOpen = this.state.isOpen;
    if (this.props.openImmediately && this.state.hasBeenClosed) isOpen = false;

    const className = this.state.isVideo ? "sb-cover-dialog" : "";
    return (
      <Modal
        isOpen={isOpen}
        toggle={this.onClose}
        backdrop="static"
        className={className}
      >
        <ModalHeader toggle={this.onClose} className="bg-gradient-primary">
          {this.getTitle()}
          {this.getSubtitle()}
        </ModalHeader>
        <ModalBody>
          <Loader
            isLoadedWhen={!!this.props.sessionRoute && !!this.state.session}
          >
            <PlayerWidget
              orgRoute={this.props.orgRoute}
              sessionRoute={this.props.sessionRoute}
              session={this.state.session}
              currentUserRoute={this.props.currentUserRoute}
              onClose={this.onClose}
              showButtons={true}
              autoPlay={true}
              onContext={this.onContext}
            />
          </Loader>
        </ModalBody>
      </Modal>
    );
  }
}
