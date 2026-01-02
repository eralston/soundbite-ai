import React, { Component } from "react";
import moment from "moment";

interface IProps {
  dateIsoString?: string;
  mutePast?: boolean;
  muteFuture?: boolean;
}

export default class RelativeDate extends Component<IProps> {
  render() {
    const date = this.props.dateIsoString;
    if (date === undefined || date === "") return <React.Fragment />;

    const dateMoment = moment.utc(date);

    if (!dateMoment.isValid()) return <span className="text-muted">None</span>;

    const isPast = dateMoment.isBefore();
    let muted = false;
    if (this.props.mutePast && isPast) muted = true;

    if (this.props.muteFuture && !isPast) muted = true;

    const displayClass = muted ? "text-muted" : "";

    const dateString = dateMoment.local().calendar();

    return <span className={displayClass}>{dateString}</span>;
  }
}
