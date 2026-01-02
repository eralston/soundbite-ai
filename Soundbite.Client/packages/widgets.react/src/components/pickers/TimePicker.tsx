import React, { Component, ChangeEvent } from "react";
// TODO: Swap with Luxon the moment we don't already have a dependency on moment
import moment, { Moment } from "moment";

interface IProps {
  defaultValue?: string | number | string[];
  onChange: (moment: Moment) => void;
  disabled?: boolean;
}

/** Time of day picker */
export class TimePicker extends Component<IProps> {
  // TODO: Timezones

  constructor(props: IProps) {
    super(props);
    this.onChange = this.onChange.bind(this);
  }

  onChange(event: ChangeEvent<HTMLSelectElement>) {
    const selectedText = event.target.selectedOptions[0].text;
    const selectedTime = moment(selectedText, "h:mmA");
    this.props.onChange(selectedTime);
  }

  render() {
    return (
      <select
        className="form-control sb-input-group-height-fix sb-time-picker"
        defaultValue={
          typeof this.props.defaultValue === "number"
            ? this.props.defaultValue.toString()
            : this.props.defaultValue
        }
        onChange={this.onChange}
        disabled={this.props.disabled}
        title="Time of Day"
      >
        <option value="12:00AM">12:00AM</option>
        <option value="12:30AM">12:30AM</option>
        <option value="1:00AM">1:00AM</option>
        <option value="1:30AM">1:30AM</option>
        <option value="2:00AM">2:00AM</option>
        <option value="2:30AM">2:30AM</option>
        <option value="3:00AM">3:00AM</option>
        <option value="3:30AM">3:30AM</option>
        <option value="4:00AM">4:00AM</option>
        <option value="4:30AM">4:30AM</option>
        <option value="5:00AM">5:00AM</option>
        <option value="5:30AM">5:30AM</option>
        <option value="6:00AM">6:00AM</option>
        <option value="6:30AM">6:30AM</option>
        <option value="7:00AM">7:00AM</option>
        <option value="7:30AM">7:30AM</option>
        <option value="8:00AM">8:00AM</option>
        <option value="8:30AM">8:30AM</option>
        <option value="9:00AM">9:00AM</option>
        <option value="9:30AM">9:30AM</option>
        <option value="10:00AM">10:00AM</option>
        <option value="10:30AM">10:30AM</option>
        <option value="11:00AM">11:00AM</option>
        <option value="11:30AM">11:30AM</option>
        <option value="12:00PM">12:00PM</option>
        <option value="12:30PM">12:30PM</option>
        <option value="1:00PM">1:00PM</option>
        <option value="1:30PM">1:30PM</option>
        <option value="2:00PM">2:00PM</option>
        <option value="2:30PM">2:30PM</option>
        <option value="3:00PM">3:00PM</option>
        <option value="3:30PM">3:30PM</option>
        <option value="4:00PM">4:00PM</option>
        <option value="4:30PM">4:30PM</option>
        <option value="5:00PM">5:00PM</option>
        <option value="5:30PM">5:30PM</option>
        <option value="6:00PM">6:00PM</option>
        <option value="6:30PM">6:30PM</option>
        <option value="7:00PM">7:00PM</option>
        <option value="7:30PM">7:30PM</option>
        <option value="8:00PM">8:00PM</option>
        <option value="8:30PM">8:30PM</option>
        <option value="9:00PM">9:00PM</option>
        <option value="9:30PM">9:30PM</option>
        <option value="10:00PM">10:00PM</option>
        <option value="10:30PM">10:30PM</option>
        <option value="11:00PM">11:00PM</option>
        <option value="11:30PM">11:30PM</option>
      </select>
    );
  }
}
