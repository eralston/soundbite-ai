import React, { Component } from "react";
import ReactDatetime from "react-datetime";
import { FormGroup, InputGroup, InputGroupAddon } from "reactstrap";
import moment, { Moment } from "moment";

import { FaPrepend, ShowWhen, Toggle } from "../controls";
import { TimePicker } from "./TimePicker";

interface IProps {
  defaultValueDateIsoString?: string;
  onDateChange: (dateIsoString?: string) => void;
  title: string;
  noneTitle?: string;
  togglable?: boolean;
  isValidDate?: (current: Moment, selected: Moment) => boolean;
  disabled?: boolean;
}

interface IState {
  selectedDateIsoString?: string;
  isEnabled: boolean;
}

/**
 * Allows users to pick the date and hour of their choosing
 * This receives and sends the date/hour as an ISO String
 * TODO: Consider limiting the amount of string-to-moment transformations in the code
 * */
export class DateTimePicker extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);

    let start = props.defaultValueDateIsoString;
    if (start === undefined) start = DateTimePicker.nextHalfHourIsoString();
    this.state = {
      selectedDateIsoString: start,
      isEnabled: props.togglable
        ? props.defaultValueDateIsoString !== undefined
        : true,
    };

    this.onDateChange = this.onDateChange.bind(this);
    this.onTimeChange = this.onTimeChange.bind(this);
    this.onToggle = this.onToggle.bind(this);
    this.isValidDate = this.isValidDate.bind(this);
  }

  public static nextHalfHourIsoString(): string {
    const start = moment();
    if (start.minute() >= 30) {
      start.add(1, "h");
      start.minute(0);
    } else {
      start.minute(30);
    }
    const ret: string = start.toISOString();
    return ret;
  }

  public get dateMoment(): Moment {
    return moment.utc(this.state.selectedDateIsoString).local();
  }

  public set dateMoment(val: Moment) {
    const selectedDateIsoString = val.utc().toISOString();
    this.setState({ selectedDateIsoString });
    this.props.onDateChange(
      this.state.isEnabled || !this.props.togglable
        ? selectedDateIsoString
        : undefined
    );
  }

  protected onDateChange(arg: Moment | string) {
    const argMoment = arg as Moment;
    let newMoment = this.dateMoment;
    newMoment = newMoment
      .year(argMoment.year())
      .month(argMoment.month())
      .date(argMoment.date());
    this.dateMoment = newMoment;
  }

  protected onTimeChange(argMoment: Moment) {
    let newMoment = this.dateMoment;
    newMoment = newMoment.hour(argMoment.hour()).minute(argMoment.minute());
    this.dateMoment = newMoment;
  }

  protected onToggle(isEnabled: boolean) {
    this.setState({ isEnabled });
    this.props.onDateChange(
      isEnabled ? this.state.selectedDateIsoString : undefined
    );
  }

  isValidDate(currentDate: any, selectedDate: any): boolean {
    if (!!this.props.isValidDate)
      return this.props.isValidDate(currentDate, selectedDate);
    else return true;
  }

  renderControls() {
    const selectedMoment = moment(this.state.selectedDateIsoString);
    const selectedDate = selectedMoment.format("L");
    const selectedHour = selectedMoment.format("h:mmA");

    return (
      <React.Fragment>
        <ReactDatetime
          timeFormat={false}
          defaultValue={selectedDate}
          onChange={this.onDateChange}
          className="sb-date-picker"
          isValidDate={this.isValidDate}
          inputProps={{ disabled: this.props.disabled }}
        />
        <TimePicker
          defaultValue={selectedHour}
          onChange={this.onTimeChange}
          disabled={this.props.disabled}
        />
      </React.Fragment>
    );
  }

  renderBody() {
    if (this.state.isEnabled || !this.props.togglable) {
      return this.renderControls();
    } else {
      return (
        <label className="sb-datepicker-none-label text-muted">
          {this.props.noneTitle}
        </label>
      );
    }
  }

  render() {
    return (
      <FormGroup className="sb-datepicker">
        <InputGroup aria-disabled={this.props.disabled}>
          <FaPrepend icon="calendar-alt" />
          <InputGroupAddon addonType="prepend" className="sb-title-input-addon">
            {this.props.title}
          </InputGroupAddon>
          <ShowWhen is={this.props.togglable}>
            <Toggle
              onToggle={this.onToggle}
              defaultValue={this.state.isEnabled}
              disabled={this.props.disabled}
              title="Session Schedule"
            />
          </ShowWhen>
          {this.renderBody()}
        </InputGroup>
      </FormGroup>
    );
  }
}
