import React, { Component } from "react";
import { Row, Col, FormGroup, InputGroup } from "reactstrap";
import moment, { Moment } from "moment";

import { Session, Recurrence, Series } from "@soundbite/api";

import { DateTimePicker } from "./DateTimePicker";
import { FaPrepend, LabelInfo, ShowWhen } from "../controls";

interface IProps {
  session: Session & Series;
  onSessionChange?: (session: Session & Series) => void;
  disabled?: boolean;
  allowRecurrence?: boolean;
}

interface IState {
  session: Session & Series;
}

/** A combined picker for datetime and recurrence */
export class SchedulePicker extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);
    this.state = { session: props.session };

    this.onRecurrenceChange = this.onRecurrenceChange.bind(this);
    this.onLimitChange = this.onLimitChange.bind(this);
    this.onReminderChange = this.onReminderChange.bind(this);
    this.onDeadlineChange = this.onDeadlineChange.bind(this);
    this.isReminderValid = this.isReminderValid.bind(this);
  }

  onRecurrenceChange(event: React.FormEvent<HTMLSelectElement>) {
    var session = this.state.session;
    session.recurrence = parseInt(event.currentTarget.value);
    this.setState({ session: session });
    if (this.props.onSessionChange) this.props.onSessionChange(session);
  }

  recurrence(allowRecurrence: boolean) {
    const date =
      this.state.session.publish === undefined
        ? new Date()
        : new Date(this.state.session.publish);

    const monthAndDayNumber = Intl.DateTimeFormat("en-US", {
      month: "short",
      day: "numeric",
    }).format(date);

    const dayOfWeek = Intl.DateTimeFormat("en-US", { weekday: "short" }).format(
      date
    );
    const dayOfMonth = date.getDate();

    const daily = `Every day starting ${monthAndDayNumber}`;
    const weekday = `Every Mon-Fri starting ${monthAndDayNumber}`;
    const weekly = `Every ${dayOfWeek} starting ${monthAndDayNumber}`;
    const monthly = `Every month on day ${dayOfMonth} starting ${monthAndDayNumber}`;

    return (
      <Row>
        <Col>
          <FormGroup>
            <LabelInfo
              label="Recurrence"
              info="How often this session will repeat"
            />
            <InputGroup aria-disabled={this.props.disabled}>
              <FaPrepend icon="sync-alt" />
              <select
                className="form-control sb-input-group-height-fix"
                value={
                  allowRecurrence
                    ? this.state.session.recurrence
                    : Recurrence.NoRepeat
                }
                onChange={this.onRecurrenceChange}
                disabled={allowRecurrence ? this.props.disabled : true}
                title="Session Recurrence Type"
              >
                <option value={Recurrence.NoRepeat}>Does Not Repeat</option>
                <option value={Recurrence.Daily}>Daily - {daily}</option>
                <option value={Recurrence.Weekday}>
                  Every Weekday - {weekday}
                </option>
                <option value={Recurrence.Weekly}>Weekly - {weekly}</option>
                <option value={Recurrence.Monthly}>Monthly - {monthly}</option>
              </select>
            </InputGroup>
            <ShowWhen
              is={this.state.session.recurrence !== Recurrence.NoRepeat}
            >
              <span className="text-muted">
                Reminder will be sent out to the host 24 hours ahead of time
              </span>
            </ShowWhen>
          </FormGroup>
        </Col>
      </Row>
    );
  }

  protected onLimitChange(event: React.ChangeEvent<HTMLInputElement>) {
    // TODO: Validation
    let session = this.state.session;
    session.limit = Math.round(parseInt(event.target.value) * 60);
    this.setState({ session: session });
    if (this.props.onSessionChange) this.props.onSessionChange(session);
  }

  onReminderChange(dateIsoString?: string) {
    const session = this.state.session;
    session.reminder = dateIsoString;
    this.setState({ session: session });
    if (this.props.onSessionChange) this.props.onSessionChange(session);
  }

  onDeadlineChange(dateIsoString?: string) {
    const session = this.state.session;
    session.publish = dateIsoString;
    this.setState({ session: session });
    if (this.props.onSessionChange) this.props.onSessionChange(session);
  }

  isReminderValid(current: Moment, selected: Moment) {
    const deadline = this.state.session.publish
      ? moment(this.state.session.publish)
      : moment();
    return current.isBefore(deadline);
  }

  scheduler() {
    return (
      <ShowWhen is={!this.state.session.publishSent}>
        <Row>
          <Col>
            <LabelInfo label="Schedule" info="When this session will publish" />
            <DateTimePicker
              togglable={this.state.session.recurrence === Recurrence.NoRepeat}
              noneTitle="Publish Now"
              onDateChange={this.onDeadlineChange}
              title="Publish"
              defaultValueDateIsoString={this.state.session.publish}
              disabled={this.props.disabled}
            />
          </Col>
        </Row>
      </ShowWhen>
    );
  }

  timeLimit() {
    return (
      <ShowWhen is={this.state.session.limit > 0}>
        <Row>
          <Col>
            <LabelInfo
              label="Time Limit"
              info="Number of minutes speaker(s) can record"
            />
            <div className="form-group">
              <InputGroup aria-disabled={this.props.disabled}>
                <FaPrepend icon="hourglass-half" />
                <input
                  placeholder="Time Limit"
                  type="text"
                  className="form-control"
                  aria-label="Minutes"
                  defaultValue={(this.state.session.limit / 60).toString()}
                  onChange={this.onLimitChange}
                  disabled={this.props.disabled}
                  title="Time Limit in Minutes"
                />
                <div className="input-group-append">
                  <span className="input-group-text">Minutes</span>
                </div>
              </InputGroup>
            </div>
          </Col>
        </Row>
      </ShowWhen>
    );
  }

  render() {
    return (
      <React.Fragment>
        {this.timeLimit()}
        {this.scheduler()}
        {this.recurrence(this.props.allowRecurrence === false ? false : true)}
      </React.Fragment>
    );
  }
}
