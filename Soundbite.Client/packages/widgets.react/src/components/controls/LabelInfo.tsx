import React, { Component } from "react";

interface ILabelInfoProps {
  label: string;
  info: string;
}

/**
 * A label and info disclosure in one component
 * */
export class LabelInfo extends Component<ILabelInfoProps> {
  render() {
    return (
      <React.Fragment>
        <label className="form-control-label">{this.props.label}</label>{" "}
        <span className="small text-muted">{this.props.info}</span>
      </React.Fragment>
    );
  }
}
