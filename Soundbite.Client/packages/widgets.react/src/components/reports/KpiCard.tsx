import React from "react";
import { observer } from "mobx-react-lite";
import { Card, CardBody, Row, Col } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { IconProp } from "@fortawesome/fontawesome-svg-core";

import { ShowWhen } from "../controls";
import { faChartArea } from "@fortawesome/free-solid-svg-icons";

interface IProps {
  title: string;
  subTitle?: string;
  tooltip?: string;
  stat?: string | number;
  statIcon?: IconProp;
  statIconBg?: string;
  delta?: string;
  deltaTitle?: string;
  deltaIcon?: string;
}

/** A simple card for displaying Key Performance Indicators, such as highlights around the activity in the system */
export const KpiCard: React.FC<IProps> = observer((props: IProps) => {
  const deltaIconClass = `fa ${props.deltaIcon ?? "fa-arrow-up"} `;
  const statIconBackClass = `icon icon-shape ${
    props.statIconBg ?? "bg-info"
  } text-white rounded-circle shadow opacity-2`;

  return (
    <Card className="sb-kpi-card mb-4 mb-xl-0 shadow">
      <ShowWhen is={!!props.statIcon}>
        <div className={statIconBackClass}>
          <FontAwesomeIcon icon={props.statIcon ?? faChartArea} />
        </div>
      </ShowWhen>
      <CardBody title={props.tooltip}>
        <Row>
          <Col>
            <h5 className="mb-0 card-title">
              <span className="text-uppercase">{props.title}</span>
              <span className="text-muted font-weight-light ml-1 text-sm">
                {props.subTitle}
              </span>
            </h5>
            <span className="h2 font-weight-bold mb-0">{props.stat}</span>
          </Col>
        </Row>
        <ShowWhen is={!!props.delta && !!props.stat}>
          <p className="mt-3 mb-0 text-muted text-sm">
            <span className="text-success mr-2">
              <i className={deltaIconClass}></i>&nbsp;
              {props.delta}
            </span>
            <ShowWhen is={!!props.deltaTitle}>
              <span className="text-nowrap">{props.deltaTitle}</span>
            </ShowWhen>
          </p>
        </ShowWhen>
      </CardBody>
    </Card>
  );
});
