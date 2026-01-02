import React from "react";
import { observer } from "mobx-react-lite";
import { Row, Col } from "reactstrap";
import { IconProp } from "@fortawesome/fontawesome-svg-core";
import {
  faCheck,
  faMicrophone,
  faPlay,
  faRedo,
} from "@fortawesome/free-solid-svg-icons";

import { ActivityHighlight, OrgContentReport } from "@soundbite/api";

import { KpiCard } from "./KpiCard";
import { ShowWhen } from "../controls";
import { FeedWidget } from "../../widgets";

interface IProps {
  contentReport?: OrgContentReport;
  orgRoute?: string;
}

/** Visualizes the metrics for the OrgContentReport object */
export const ContentCards: React.FC<IProps> = observer((props: IProps) => {
  const highlight = (
    highlight?: ActivityHighlight,
    icon?: IconProp,
    tooltip?: string
  ) => {
    if (highlight == null) {
      return <React.Fragment />;
    }

    return (
      <Col lg={6} xl={3} className="mt-1">
        <KpiCard
          title={highlight.title}
          tooltip={tooltip}
          stat={highlight.value}
          statIcon={icon}
        />
      </Col>
    );
  };

  return (
    <ShowWhen is={!!props.contentReport}>
      <div className="ml-1">
        <h3 className="mt-3">
          <i className="fas fa-chart-area"></i>
          Content
        </h3>
      </div>
      <Row className="">
        {highlight(
          props.contentReport?.sessionsCount,
          faMicrophone,
          "Number of sessions in the organization for the given time period"
        )}
        {highlight(
          props.contentReport?.seriesCount,
          faRedo,
          "Number of active recurring sessions in the organization for the given time period"
        )}
        {highlight(
          props.contentReport?.consumeCount,
          faPlay,
          "Number of times anyone played a session - people may have listened to a session more than once"
        )}
        {highlight(
          props.contentReport?.acknowledgeCount,
          faCheck,
          "Number of times a session was confirmed by someone - this can only happen once per person per session"
        )}
      </Row>
      <ShowWhen is={(props.contentReport?.highlights.length ?? 0) > 0}>
        <Row>
          {props.contentReport?.highlights.slice(0, 4).map((item) => {
            return highlight(item);
          })}
        </Row>
      </ShowWhen>
      <ShowWhen
        is={
          props.orgRoute != null && props.contentReport?.recentSessions != null
        }
      >
        <Row>
          <Col className="mt-4">
            <FeedWidget
              title="Most Recent Soundbite Sessions"
              isReadOnly={true}
              orgRoute={props.orgRoute}
              sessions={props.contentReport?.recentSessions}
            />
          </Col>
        </Row>
      </ShowWhen>
    </ShowWhen>
  );
});
