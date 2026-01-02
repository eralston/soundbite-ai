import React from "react";
import { observer } from "mobx-react-lite";
import { Row, Col } from "reactstrap";

import { OrgContentReport, Utils } from "@soundbite/api";
import { ShowWhen } from "../controls";
import { GraphCard } from "./GraphCard";

interface IProps {
  contentReport?: OrgContentReport;
  orgRoute?: string;
  className?: string;
}

/** Visualizes the metrics for the OrgContentReport object */
export const ActivityGraphs: React.FC<IProps> = observer((props: IProps) => {
  return (
    <ShowWhen is={!!props.contentReport}>
      <Row>
        <Col sm="12" lg="6">
          <GraphCard
            className="mt-4"
            data={props.contentReport?.sessionsCountOverTime}
            fillDateRange={true}
          ></GraphCard>
        </Col>
        <Col sm="12" lg="6">
          <GraphCard
            className="mt-4"
            data={props.contentReport?.consumeCountOverTime}
            fillDateRange={true}
          ></GraphCard>
        </Col>
      </Row>
    </ShowWhen>
  );
});
