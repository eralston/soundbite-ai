import React, { Component } from "react";
import moment from "moment";

import { SeriesPreview, SeriesDetails, Recurrence } from "@soundbite/api";

interface IRecurrenceSummaryProps {
  series: SeriesPreview | SeriesDetails;
}

/** Shows the recurrence behavior of the given series */
export class RecurrenceSummary extends Component<IRecurrenceSummaryProps> {
  render() {
    const dateIsoString = this.props.series.template.publish;
    if (dateIsoString === undefined) return <React.Fragment />;

    const date = moment(dateIsoString).local();

    const monthAndDayNumber = date.format("MMM D");
    const dayOfWeek = date.format("dddd");
    const dayOfMonth = date.date();

    switch (this.props.series.recurrence) {
      case Recurrence.Daily:
        return (
          <span>
            Every Day{" "}
            <span className="text-muted">starting {monthAndDayNumber}</span>
          </span>
        );
      case Recurrence.Weekday:
        return (
          <span>
            Every Weekday{" "}
            <span className="text-muted">starting {monthAndDayNumber}</span>
          </span>
        );
      case Recurrence.Weekly:
        return (
          <span>
            Every {dayOfWeek}{" "}
            <span className="text-muted">starting {monthAndDayNumber}</span>
          </span>
        );
      case Recurrence.Monthly:
        return (
          <span>
            Every Month{" "}
            <span className="text-muted">
              on day {dayOfMonth} starting {monthAndDayNumber}
            </span>
          </span>
        );
    }
    return <React.Fragment />;
  }
}
