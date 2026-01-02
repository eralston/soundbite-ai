/** @jsx jsx */
import { jsx } from "@emotion/react";
import React, { Component } from "react";
import CreatableSelect from "react-select/creatable";
import makeAnimated from "react-select/animated";

import { Utils } from "@soundbite/api";

interface IProps {
  onChange: (emails: string[]) => void;
  disabled?: boolean;
}

interface IState {
  options: any;
  value: any;
}

/**
 * Enables the user to generate multiple e-mail addresses
 * */
export class EmailPicker extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);
    this.onChange = this.onChange.bind(this);
    this.onCreateOption = this.onCreateOption.bind(this);
    this.onFormatCreateLabel = this.onFormatCreateLabel.bind(this);
    this.state = {
      options: [],
      value: undefined,
    };
  }

  firePropsOnChange(newValue: any) {
    const emails = new Array<string>();
    const listOfOptions =
      newValue == null
        ? []
        : (newValue as {
            label: string;
            value: string;
          }[]);
    for (const option of listOfOptions) {
      emails.push(option.value);
    }
    this.props.onChange(emails);
  }

  onChange(newValue: any) {
    this.setState({ value: newValue });
    this.firePropsOnChange(newValue);
  }

  onCreateOption(inputValue: string) {
    const email = Utils.validateEmail(inputValue);
    if (!email) return;
    const newOption = {
      label: email,
      value: email,
    };
    let newValue = [...(this.state.value || []), newOption];
    this.setState({
      options: [...this.state.options, newOption],
      value: newValue,
    });
    this.firePropsOnChange(newValue);
  }

  onFormatCreateLabel(inputValue: string) {
    return `Send Invite to ${inputValue}...`;
  }

  render() {
    const animatedComponents = makeAnimated();
    const { options, value } = this.state;
    return (
      <CreatableSelect
        className="sb-picker"
        classNamePrefix="sb-picker"
        components={animatedComponents}
        placeholder="Enter Work E-Mail..."
        isMulti
        onChange={this.onChange}
        onCreateOption={this.onCreateOption}
        options={options}
        value={value}
        isDisabled={this.props.disabled}
        formatCreateLabel={this.onFormatCreateLabel}
      />
    );
  }
}
