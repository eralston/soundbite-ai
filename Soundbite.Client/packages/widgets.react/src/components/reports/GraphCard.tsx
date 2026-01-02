import React from "react";
import { CardBody, Card } from "reactstrap";
import { Line } from "react-chartjs-2";

import { ActivityRange, OrgContentReport, Utils } from "@soundbite/api";
import { Loader, ShowWhen } from "../controls";
import {
  PointElement,
  Chart,
  ChartOptions,
  CategoryScale,
  LinearScale,
  LineElement,
  ChartData,
} from "chart.js";
import { GlobalTheme } from "../../styles";

Chart.register(CategoryScale);
Chart.register(LinearScale);
Chart.register(PointElement);
Chart.register(LineElement);

interface IProps {
  contentReport?: OrgContentReport;
  className?: string;
  data?: ActivityRange;
  fillDateRange?: boolean;
}

const font = {
  family: GlobalTheme.current.fonts.fontFamily,
  size: 14,
};

export const GraphCard: React.FC<IProps> = (props: IProps) => {
  if (props?.data != null && props.fillDateRange === true) {
    props.data.items = Utils.fillMissingDates(props.data.items);
  }

  const graphData: ChartData<"line", number[], string> = {
    labels: props.data?.items.map((t) => Utils.formatDate(t.label)),
    datasets: [
      {
        label: props.data?.title,
        data: props.data?.items.map((t) => t.value) ?? [],
      },
    ],
  };

  const graphOptions: ChartOptions<"line"> = {
    responsive: true,
    plugins: {
      legend: {
        position: "bottom",
        labels: { font: font },
      },
      title: {
        display: true,
        text: "Example Chart",
      },
    },
    font: font,
    hover: {},
    scales: {
      y: { beginAtZero: true },
    },
  };

  return (
    <Loader isLoadedWhen={props.data != null}>
      <Card className={`sb-kpi-card mb-4 mb-xl-0 shadow ${props.className}`}>
        <CardBody>
          <span>{props.data?.title}</span>
          <ShowWhen is={graphData.labels?.length === 0}>
            No Data for Graph
          </ShowWhen>
          <ShowWhen
            is={graphData.labels != null && graphData.labels.length > 0}
          >
            <Line data={graphData} options={graphOptions} />
          </ShowWhen>
        </CardBody>
      </Card>
    </Loader>
  );
};
