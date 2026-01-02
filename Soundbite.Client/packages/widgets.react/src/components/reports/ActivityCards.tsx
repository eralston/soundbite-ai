import React from "react";
import { observer } from "mobx-react-lite";
import { Row, Col } from "reactstrap";
import {
  faHeadphonesAlt,
  faHeadset,
  faHourglassEnd,
  faHourglassStart,
} from "@fortawesome/free-solid-svg-icons";

import { ActivityHighlight, ActivityReport } from "@soundbite/api";

import { KpiCard } from "./KpiCard";
import { ShowWhen } from "../controls";

interface IProps {
  activityReport?: ActivityReport;
}

/** Shows the people and time metrics for the given ActivityReport object */
export const ActivityCards: React.FC<IProps> = observer((props: IProps) => {
  const timeSummary = (minutes?: number): string => {
    if (minutes == null) {
      return "";
    }
    const hrs = Math.floor(minutes / 60);
    const min = (minutes - hrs * 60) % 60;
    let ret = "";
    if (hrs > 0) ret += `${hrs}h `;
    ret += `${min}m`;
    return ret;
  };

  const highlight = (highlight: ActivityHighlight) => {
    return (
      <Col lg={6} xl={3} className="mt-4">
        <KpiCard title={highlight.title} stat={highlight.value} />
      </Col>
    );
  };

  return (
    <ShowWhen is={!!props.activityReport}>
      <div className="ml-2">
        <h3 className="mt-3">
          <i className="fas fa-chart-area"></i>
          Activity
        </h3>
      </div>
      <Row className="">
        <Col lg={6} xl={3} className="mt-1">
          <KpiCard
            title="Listeners"
            tooltip="Number of unique people who have listened to at least one Soundbite Session"
            stat={props.activityReport?.consumerCount}
            statIcon={faHeadphonesAlt}
          />
        </Col>
        <Col lg={6} xl={3} className="mt-1">
          <KpiCard
            title="Listening Time"
            tooltip="Total session time listened by all people - remember it's possible to listen to a Soundbite Session more than once per person"
            stat={timeSummary(props.activityReport?.unitsConsumed)}
            statIcon={faHourglassEnd}
          />
        </Col>
        <Col lg={6} xl={3} className="mt-1">
          <KpiCard
            title="Speakers"
            tooltip="Number of unique people who have created one or more Soundbite Session"
            stat={props.activityReport?.producerCount}
            statIcon={faHeadset}
          />
        </Col>
        <Col lg={6} xl={3} className="mt-1">
          <KpiCard
            title="Recording Time"
            tooltip="Total Soundbite Session time recorded by all people"
            stat={timeSummary(props.activityReport?.unitsProduced)}
            statIcon={faHourglassStart}
          />
        </Col>
      </Row>
      <ShowWhen is={(props.activityReport?.highlights.length ?? 0) > 0}>
        <Row>
          {props.activityReport?.highlights.slice(0, 4).map((item) => {
            return highlight(item);
          })}
        </Row>
      </ShowWhen>
    </ShowWhen>
  );
});
