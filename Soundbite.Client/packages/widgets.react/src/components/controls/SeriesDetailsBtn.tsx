import { faCalendar } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Modal, ModalHeader, ModalBody, ModalFooter, Button } from "reactstrap";
import React, { Component, createRef } from "react";
import sanitizeHtml from "sanitize-html";

import {
  PersonRole,
  SeriesDetails,
  SeriesPreview,
  SessionPreview,
  Utils,
} from "@soundbite/api";

import { LabelInfo } from "./LabelInfo";
import { Loader } from "./Loader";
import { ParticipantSummary } from "./ParticipantSummary";
import { ShowWhen } from "./ShowWhen";
import { WidgetStore } from "../../store";
import { ErrorDlg } from "../ErrorDlg";

interface IProps {
  series: SeriesPreview;
}

interface IState {
  seriesDetails?: SeriesDetails;
  isOpen: boolean;
  isExpanded: boolean;
}

/**
 * Shows the details for a given series object
 * */
export class SeriesDetailsDlg extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);

    this.state = {
      seriesDetails: undefined,
      isOpen: false,
      isExpanded: false,
    };

    this.open = this.open.bind(this);
    this.close = this.close.bind(this);
    this.renderScheduleRow = this.renderScheduleRow.bind(this);
    this.expandSchedule = this.expandSchedule.bind(this);
  }

  async open() {
    try {
      // Validate
      if (!this.props.series) {
        this.setState({ seriesDetails: undefined, isOpen: false });
        return;
      }
      const orgRoute = WidgetStore.organizations.currentOrg?.details.route;
      if (!orgRoute) {
        throw new Error("Must have initialized organization to load session");
      }

      // Load
      this.setState({
        seriesDetails: undefined,
        isOpen: true,
        isExpanded: false,
      });
      const seriesDetails = await WidgetStore.series.readSeriesAsync(
        orgRoute,
        this.props.series.route
      );

      // Set returned value
      this.setState({ seriesDetails });
    } catch (err: any) {
      this.setState({ isOpen: false });
      ErrorDlg.show(
        err,
        "Error Loading Soundbite Session",
        "Likely could not make contact with server"
      );
    }
  }

  close() {
    this.setState({ isOpen: false });
  }

  getTitle() {
    if (this.state.seriesDetails === undefined)
      return "Loading Soundbite Series...";

    return this.state.seriesDetails.name;
  }

  getSubtitle() {
    if (this.state.seriesDetails === undefined) return <span> </span>;

    const details: SeriesDetails = this.state.seriesDetails;
    return (
      <span className="sb-modal-header-subtitle">
        {Utils.recurrenceDescription(
          details.template.publish,
          details.recurrence
        )}
      </span>
    );
  }

  renderScheduleRow(session: SessionPreview, key: any) {
    return (
      <tr key={session.route}>
        <td>{Utils.formatRelativeDate(session.reminder)}</td>
        <td>{Utils.formatRelativeDate(session.publish)}</td>
      </tr>
    );
  }

  expandSchedule() {
    this.setState({ isExpanded: true });
  }

  schedule() {
    const previewCount = 3;
    const sessions = this.state.isExpanded
      ? this.state.seriesDetails?.sessions
      : this.state.seriesDetails?.sessions.slice(0, previewCount);
    const showExpandBtn =
      (this.state.seriesDetails?.sessions.length ?? 0) > previewCount &&
      !this.state.isExpanded;
    return (
      <div className="mt-3">
        <LabelInfo
          label="Schedule"
          info="Future Soundbite Sessions for this series"
        />
        <table className="table align-items-center">
          <thead className="thead-light">
            <tr>
              <th scope="col" className="sort">
                Reminder
              </th>
              <th scope="col" className="sort">
                Publish
              </th>
            </tr>
          </thead>
          <tbody className="list">
            {sessions?.map(this.renderScheduleRow)}
          </tbody>
        </table>
        <ShowWhen is={showExpandBtn}>
          <Button
            onClick={this.expandSchedule}
            color="info"
            size="sm"
            type="button"
            title="Show More Scheduled Sessions"
          >
            Show More
          </Button>
        </ShowWhen>
      </div>
    );
  }

  render() {
    let isOpen = this.state.isOpen;

    const series = this.state.seriesDetails;

    const cleanPromptContext = sanitizeHtml(
      series?.template?.prompts[0].text || ""
    );

    const isAudienceEnabled = WidgetStore.isPersonRole(
      WidgetStore.organizations.currentOrg?.permissions?.minRoleForAudience ??
        PersonRole.Unknown
    );

    return (
      <Modal isOpen={isOpen} toggle={this.close} backdrop="static">
        <ModalHeader toggle={this.close} className="bg-gradient-primary">
          {this.getTitle()}
          {this.getSubtitle()}
        </ModalHeader>
        <Loader
          isLoadedWhen={this.state.seriesDetails !== undefined}
          style={{ minHeight: "12rem" }}
        >
          <ModalBody>
            <ShowWhen is={cleanPromptContext.trim().length > 0}>
              <LabelInfo
                label="Call to Action"
                info="Text to prime the conversation"
              />
              <p dangerouslySetInnerHTML={{ __html: cleanPromptContext }} />
            </ShowWhen>
            <ShowWhen is={isAudienceEnabled}>
              <ParticipantSummary session={series?.template} />
            </ShowWhen>
            {this.schedule()}
          </ModalBody>
          <ModalFooter>
            <button
              type="button"
              className="btn btn-primary"
              onClick={this.close}
              title="Close"
            >
              Close
            </button>
          </ModalFooter>
        </Loader>
      </Modal>
    );
  }
}

export interface ISeriesDetailsBtnProps {
  series: SeriesPreview;
}

export default class SeriesDetailsBtn extends Component<ISeriesDetailsBtnProps> {
  private dialog = createRef<SeriesDetailsDlg>();

  constructor(props: ISeriesDetailsBtnProps) {
    super(props);

    this.state = {
      isOpen: false,
    };

    this.open = this.open.bind(this);
  }

  async open() {
    this.dialog.current?.open();
  }

  render() {
    return (
      <React.Fragment>
        <button
          className="btn btn-outline-primary btn-sm"
          type="button"
          onClick={this.open}
          title="Show Series Schedule"
        >
          <span className="btn-inner--icon">
            <FontAwesomeIcon icon={faCalendar} />
            <span className="btn-inner--text">Schedule</span>
          </span>
        </button>
        <SeriesDetailsDlg ref={this.dialog} series={this.props.series} />
      </React.Fragment>
    );
  }
}
