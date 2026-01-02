import React, { useState, useEffect } from "react";
import { observer } from "mobx-react-lite";
import {
  Alert,
  Button,
  Card,
  CardHeader,
  Col,
  FormGroup,
  InputGroup,
  InputGroupAddon,
  Row,
} from "reactstrap";
import moment, { Moment } from "moment";
import ReactDatetime from "react-datetime";

import {
  ActivityReport,
  OrgContentReport,
  ReportService,
} from "@soundbite/api";

import { FaPrepend, ShowWhen } from "../controls";
import { WidgetStore } from "../../store";

import { ActivityCards } from "./ActivityCards";
import { ContentCards } from "./ContentCards";
import { ErrorDlg } from "../ErrorDlg";
import { ActivityGraphs } from "./ActivityGraphs";

interface IProps {
  orgRoute: string;
}

const utcNow = moment().utc();

/** Shows an org-level set of reports for the given org */
export const ReportView: React.FC<IProps> = observer((props: IProps) => {
  const [startDate, setStartDate] = useState<string | undefined>(
    moment().utc().add({ months: -1 }).format("L")
  );
  const [endDate, setEndDate] = useState<string | undefined>(
    moment().utc().format("L")
  );
  const [activityReport, setActivityReport] = useState<
    ActivityReport | undefined
  >(undefined);
  const [contentReport, setContentReport] = useState<
    OrgContentReport | undefined
  >(undefined);

  function applyDateToNow(argMoment: Moment) {
    let newMoment = moment().utc();
    newMoment = newMoment
      .year(argMoment.year())
      .month(argMoment.month())
      .day(argMoment.day());
    return newMoment;
  }

  const load = async () => {
    try {
      setActivityReport(undefined);
      setContentReport(undefined);

      const orgRoute = props.orgRoute;
      if (!orgRoute) {
        throw new Error("Must have initialized organization to load session");
      }

      const startDateArg = applyDateToNow(moment(startDate)).toISOString();
      const endDateArg = applyDateToNow(moment(endDate)).toISOString();
      const activityPromise = ReportService.activityReportAsync(
        orgRoute,
        startDateArg,
        endDateArg
      );
      const contentPromise = ReportService.contentReportAsync(
        orgRoute,
        startDateArg,
        endDateArg
      );

      const [activity, content] = await Promise.all([
        activityPromise,
        contentPromise,
      ]);

      if (activity?.startUtc != null && startDate == null) {
        setStartDate(activity.startUtc);
      }

      if (activity?.endUtc != null && endDate == null) {
        setEndDate(activity.endUtc);
      }

      setActivityReport(activity);
      setContentReport(content);
    } catch (err: any) {
      ErrorDlg.show(
        err,
        "Error Loading Activity Report",
        "Unable to reach reporting service"
      );
    }
  };

  function onStartDateChange(arg: Moment | string) {
    const argMoment = arg as Moment;
    let newMoment = moment().utc();
    newMoment = newMoment
      .year(argMoment.year())
      .month(argMoment.month())
      .day(argMoment.day());
    setStartDate(newMoment.toISOString());
  }

  function onEndDateChange(arg: Moment | string) {
    const argMoment = arg as Moment;
    let newMoment = moment().utc();
    newMoment = newMoment
      .year(argMoment.year())
      .month(argMoment.month())
      .day(argMoment.day());
    setEndDate(newMoment.toISOString());
  }

  function isValidStartDate(
    currentDate: Moment,
    selectedDate: Moment
  ): boolean {
    if (currentDate.isAfter(utcNow)) {
      return false;
    }

    if (currentDate.isAfter(moment(endDate))) {
      return false;
    }

    return true;
  }

  function isValidEndDate(currentDate: Moment, selectedDate: Moment): boolean {
    if (currentDate.isAfter(utcNow)) {
      return false;
    }

    return true;
  }

  const onRefresh = async () => {
    await load();
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line
  }, [startDate, endDate]);

  const currentDateDisplay = moment().format("LLL");
  const isLoading = !activityReport || !contentReport;
  return (
    <div className="sb-reports">
      <Alert color="light">
        Reporting is Currently in Preview. For more info please contact{" "}
        <a className="alert-link" href="mailto:Support@Soundbite.Freshdesk.Com">
          Support@Soundbite.Freshdesk.Com
        </a>
      </Alert>
      <Card>
        <CardHeader className="border-0">
          <Row>
            <Col>
              <h3 className="mb-0">
                <i className="fas fa-chart-area d-none d-sm-inline"></i>
                {WidgetStore.organizations.currentOrg?.details.name} Soundbite
                Reports
              </h3>
            </Col>
          </Row>
          <Row className="pt-2">
            <Col md="12" lg="6">
              <FormGroup className="sb-datepicker">
                <InputGroup className="pr-2">
                  <FaPrepend icon="calendar-alt" />
                  <ReactDatetime
                    timeFormat={false}
                    defaultValue={startDate}
                    onChange={onStartDateChange}
                    className="sb-date-picker"
                    isValidDate={isValidStartDate}
                  />
                  <ReactDatetime
                    timeFormat={false}
                    defaultValue={endDate}
                    onChange={onEndDateChange}
                    className="sb-date-picker"
                    isValidDate={isValidEndDate}
                  />
                </InputGroup>
              </FormGroup>
            </Col>
            <Col sm="12" md="12" lg="6" className="text-right">
              <span className="text-sm text-muted pr-2">
                {currentDateDisplay}
              </span>
              <Button
                outline={true}
                color="secondary"
                className="btn-icon btn-sm"
                onClick={onRefresh}
                disabled={isLoading}
                title="Refresh Report"
              >
                <span className="btn-inner--icon">
                  <i
                    className={`fas fa-sync-alt ${isLoading ? "fa-spin" : ""}`}
                  ></i>
                </span>
              </Button>
            </Col>
          </Row>
        </CardHeader>
      </Card>
      <ShowWhen is={!!activityReport && !!contentReport}>
        <ActivityCards activityReport={activityReport} />
        <ActivityGraphs contentReport={contentReport} />
        <ContentCards orgRoute={props.orgRoute} contentReport={contentReport} />
      </ShowWhen>
    </div>
  );
});
