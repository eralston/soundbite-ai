import {
  faCheck,
  faHeadphonesAlt,
  faPercentage,
  faPlay,
  faUsers,
} from "@fortawesome/free-solid-svg-icons";
import {
  ActivityRangeItem,
  NotificationChannel,
  NotificationStatus,
  Participant,
  ParticipantGroup,
  ParticipantRole,
  SessionContentDetailsReport,
  SessionContentNotificiation,
  SessionContentPersonEvent,
  SessionNotificationType,
  SessionType,
} from "@soundbite/api";
import { Utils } from "@soundbite/api";
import {
  PersonRole,
  ReportService,
  Session,
  SessionContentReport,
  SessionSecurityType,
} from "@soundbite/api";
import moment from "moment-timezone";
import React, { useState, useEffect } from "react";
import { Card, CardBody, Col, Row } from "reactstrap";

import { KpiCard, Loader, ShowWhen } from "../components";
import { GraphCard } from "../components/reports/GraphCard";
import { SessionReactions } from "../components/SessionReactions";
import { WidgetStore } from "../store/WidgetStore";
import { DialogAction } from "./DialogAction";
import { DialogButtonStyleType } from "./DialogButtonStyleType";
import { DialogHelper } from "./DialogHelper";
import { DialogStyleType } from "./DialogStyleType";

interface IProps {
  orgRoute: string;
  session: Session;
  onClose: () => void;
}

export const SessionReportDialog: React.FC<IProps> = (props: IProps) => {
  let [isActionRunning, setIsActionRunning] = useState(false);
  let [sessionReport, setSessionReport] = useState<
    SessionContentReport | undefined
  >(undefined);
  const isAdmin = WidgetStore.isPersonRole(PersonRole.Admin);

  // NOTE: audience count is 100% accurate, but listens could count repeats
  // TODO: Make this fully-based on reading the database
  const audienceCount = Number(sessionReport?.audienceSize.value ?? 0);
  const listenersCount = Number(sessionReport?.consumerCount.value ?? 0);

  let percent = 0;
  if (audienceCount > 0) {
    percent = Math.floor((listenersCount / audienceCount) * 100);
  }

  if (percent > 100) {
    percent = 100;
  }

  const load = async () => {
    const newReport = await ReportService.sessionReportAsync(
      props.orgRoute,
      props.session.route
    );
    setSessionReport(newReport);
  };

  const downloadCsv = (data: string, filename: string) => {
    const blob = new Blob([data], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.setAttribute("download", filename);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  const csvRow = (...data: string[]) => {
    // Convert data into comma-delimited string ready for CSV
    return data.map((item) => `"${item}"`).join(",");
  };

  const createCsv = async (): Promise<string> => {
    const reportData: SessionContentDetailsReport =
      await ReportService.sessionDetailsReportAsync(
        props.orgRoute,
        props.session.route
      );

    if (reportData == null) {
      window.alert("Unable Generate CSV Report");
      return "";
    }

    const tz = moment.tz(moment.tz.guess()).format("z");
    const csv: string[] = [];
    // Header with a row per field
    sessionSummary(
      csv,
      csvRow,
      props,
      tz,
      percent,
      reportData,
      listenersCount,
      audienceCount
    );

    // List of Audience
    audience(reportData, csv, csvRow, tz);

    // List of Groups in the Audience
    if (
      reportData.audienceGroups != null &&
      reportData.audienceGroups.length > 0
    ) {
      audienceGroups(reportData, csv, csvRow, tz);
    }

    // List of Listeners
    if (reportData.plays != null && reportData.plays.length > 0) {
      playEvents(reportData, csv, csvRow, tz);
    }

    // Listens over time
    if (
      reportData.consumeCountOverTime != null &&
      reportData.consumeCountOverTime.items.length > 0
    ) {
      consumeCountOverTime(reportData, csv, csvRow, tz);
    }

    // List of Acknowledgers
    if (
      reportData.acknowledgers != null &&
      reportData.acknowledgers.length > 0
    ) {
      acknowledgers(reportData, csv, csvRow, tz);
    }

    sessionNotifications(reportData, csv, csvRow, tz);

    return csv.join("\r\n");
  };

  const makeSafeFilename = (filename: string) => {
    // Removing any invalid character.
    return filename.replace(/[<>:"\/\\|?*]+/g, "_");
  };

  const download = async () => {
    const currentDateInISOFormat = moment().format("YYYY-MM-DD");
    const csv = await createCsv();
    if (csv.length > 0) {
      downloadCsv(
        csv,
        `Soundbite-Session-Report-${currentDateInISOFormat}-${makeSafeFilename(
          props.session.name
        )}.csv`
      );
    }
  };

  useEffect(() => {
    load();
  }, []);

  const getBody = (): JSX.Element => {
    return (
      <React.Fragment>
        <Loader isLoadedWhen={sessionReport != null}>
          <ShowWhen
            is={
              props.session.sessionSecurity === SessionSecurityType.Protected &&
              percent > 0
            }
          >
            <Row className="mt-2">
              <Col>
                <KpiCard
                  title="Engagement"
                  tooltip="Percentage of audience that has played to the Soundbite Session"
                  stat={`${percent}%`}
                  statIcon={faPercentage}
                />
              </Col>
            </Row>
          </ShowWhen>
          <Row className="mt-2">
            <Col>
              <Card className="sb-kpi-card mb-4 mb-xl-0 shadow">
                <CardBody title="Session Reactions">
                  <h5 className="mb-0 card-title">
                    <span className="text-uppercase">Reactions</span>
                  </h5>
                  <SessionReactions
                    orgRoute={props.orgRoute}
                    sessionRoute={props.session.route}
                    readOnly={true}
                  />
                </CardBody>
              </Card>
            </Col>
          </Row>
          <Row className="mt-2">
            <Col>
              <KpiCard
                title="Plays"
                tooltip="Distinct number of times the session was played; may count repeat plays from the same person"
                stat={sessionReport?.consumeCount.value}
                statIcon={faPlay}
              />
            </Col>
          </Row>
          <ShowWhen
            is={props.session.sessionSecurity === SessionSecurityType.Protected}
          >
            <Row className="mt-2">
              <Col>
                <KpiCard
                  title="Consumers"
                  tooltip="Number of individuals who played the Soundbite Session from any channel"
                  stat={sessionReport?.consumerCount.value}
                  statIcon={faHeadphonesAlt}
                />
              </Col>
            </Row>
            <Row className="mt-2">
              <Col>
                <KpiCard
                  title="Audience"
                  tooltip="Number of individuals who received the Soundbite Session when it was published"
                  stat={sessionReport?.audienceSize.value}
                  statIcon={faUsers}
                />
              </Col>
            </Row>
            <Row className="mt-2">
              <Col>
                <KpiCard
                  title="Acknowledgements"
                  tooltip="Number of individuals who confirmed playing to remove it from their feed"
                  stat={sessionReport?.acknowledgeCount.value}
                  statIcon={faCheck}
                />
              </Col>
            </Row>
          </ShowWhen>
          <Row className="mt-2">
            <Col>
              <GraphCard
                className="mt-4"
                data={sessionReport?.consumeCountOverTime}
                fillDateRange={true}
              />
            </Col>
          </Row>
        </Loader>
      </React.Fragment>
    );
  };

  const getActions = (): DialogAction[] => {
    const ret = [];
    if (isAdmin) {
      ret.push({
        name: "Download",
        isClose: false,
        style: DialogButtonStyleType.Secondary,
        onSelected: download,
      });
    }

    ret.push({
      name: "Close",
      isClose: true,
      style: DialogButtonStyleType.Primary,
    });
    return ret;
  };

  return DialogHelper.RenderDialog(
    DialogStyleType.Normal,
    `${props.session.name} Engagement Report`,
    getActions(),
    getBody(),
    isActionRunning,
    setIsActionRunning,
    props.onClose
  );
};

function sessionSummary(
  csv: string[],
  csvRow: (...data: string[]) => string,
  props: IProps,
  tz: string,
  percent: number,
  reportData: SessionContentDetailsReport,
  listenersCount: number,
  audienceCount: number
) {
  csv.push(csvRow("Session Name", props.session.name));
  csv.push(csvRow(`Date Created (${tz})`, props.session.createdUtc));
  csv.push(
    csvRow(
      "Session Security",
      props.session.sessionSecurity === SessionSecurityType.Protected
        ? "Private"
        : "Public"
    )
  );
  // TODO: Add other types as they arrive
  csv.push(
    csvRow(
      "Session Type",
      props.session.sessionType === SessionType.Announcement
        ? "Announcement"
        : "Other"
    )
  );
  if (props.session.reminderSent != null) {
    csv.push(
      csvRow(
        `Date Reminded (${tz})`,
        Utils.formatDateTime(props.session.reminderSent)
      )
    );
  }
  if (props.session.publishSent != null) {
    csv.push(
      csvRow(
        `Date Published (${tz})`,
        Utils.formatDateTime(props.session.publishSent)
      )
    );
  } else {
    csv.push(csvRow(`Date Published (${tz})`, "Unpublished"));
  }

  csv.push(csvRow(""));

  // Summary numbers
  csv.push(csvRow("Engagement Summary"));
  csv.push(csvRow("Engagement", "" + percent + "%"));
  csv.push(csvRow("Views", "" + reportData.consumerCount.value));
  csv.push(csvRow("Viewer Count", "" + listenersCount));
  csv.push(csvRow("Audience", "" + audienceCount));
  csv.push(
    csvRow("Acknowledgement Count", "" + reportData.acknowledgeCount.value)
  );
  csv.push(csvRow(""));
}

function acknowledgers(
  reportData: SessionContentDetailsReport,
  csv: string[],
  csvRow: (...data: string[]) => string,
  tz: string
) {
  const acknowledgers = reportData.acknowledgers;
  csv.push(csvRow("Acknowledgements"));
  csv.push(csvRow("One entry per participant indicated as having consumed"));
  csv.push(
    csvRow(
      "Family Name",
      "Given Name",
      "Job Title",
      "Email",
      `Date Listened (${tz})`
    )
  );
  acknowledgers.forEach((acknowleder: SessionContentPersonEvent) => {
    csv.push(
      csvRow(
        acknowleder.familyName,
        acknowleder.givenName,
        acknowleder.title,
        acknowleder.email,
        Utils.formatDateTime(acknowleder.dateTimeUtc)
      )
    );
  });
  csv.push(csvRow(""));
}

function notificationStatusName(
  notification: SessionContentNotificiation
): string {
  switch (notification.status) {
    case NotificationStatus.Success:
      return "Success";
    case NotificationStatus.Failed:
      return "Failure";
    default:
      return "Unknown";
  }
}

function notificationTypeName(
  notification: SessionContentNotificiation
): string {
  switch (notification.notificationType) {
    case SessionNotificationType.HostPublish:
      return "Host Publish";
    case SessionNotificationType.Publish:
      return "Audience Publish";
    case SessionNotificationType.Reminder:
      return "Host Reminder";
    default:
      return "Unknown";
  }
}

function notificationChannelName(
  notification: SessionContentNotificiation
): string {
  switch (notification.channel) {
    case NotificationChannel.Email:
      return "E-Mail";
    case NotificationChannel.SMS:
      return "SMS";
    case NotificationChannel.Teams:
      return "MS Teams";
    default:
      return "Unknown";
  }
}

function sessionNotifications(
  reportData: SessionContentDetailsReport,
  csv: string[],
  csvRow: (...data: string[]) => string,
  tz: string
) {
  const notifications = reportData.notifications;
  console.log("Example data", JSON.stringify(notifications));
  csv.push(csvRow("Notifications"));
  csv.push(
    csvRow(
      `Date Sent (${tz})`,
      "Status",
      "Channel",
      "Type",
      "Details",
      "User Family Name",
      "User Given Name",
      "User Job Title",
      "User Email"
    )
  );
  notifications.forEach((notification: SessionContentNotificiation) => {
    csv.push(
      csvRow(
        Utils.formatDateTime(notification.dateTimeUtc),
        notificationStatusName(notification),
        notificationChannelName(notification),
        notificationTypeName(notification),
        notification.details ?? "",
        notification.familyName,
        notification.givenName,
        notification.title,
        notification.email
      )
    );
  });
  csv.push(csvRow(""));
}

function audienceGroups(
  reportData: SessionContentDetailsReport,
  csv: string[],
  csvRow: (...data: string[]) => string,
  tz: string
) {
  const audienceGroups = reportData.audienceGroups;
  csv.push(csvRow("Participant Groups"));
  csv.push(
    csvRow(
      "Groups assigned to the session; if the session is published, then the individuals above reflect the members of these groups at the time of session publish"
    )
  );
  csv.push(
    csvRow("Name", "Description", "Session Role", `Last Updated (${tz})`)
  );
  audienceGroups.forEach((partGrp: ParticipantGroup) => {
    csv.push(
      csvRow(
        partGrp.group.name,
        partGrp.group.description,
        Utils.participantRoleString(partGrp.participantRole),
        Utils.formatDateTime(partGrp.updatedUtc)
      )
    );
  });
  csv.push(csvRow(""));
}

function audience(
  reportData: SessionContentDetailsReport,
  csv: string[],
  csvRow: (...data: string[]) => string,
  tz: string
) {
  const audience = reportData.audience;
  csv.push(csvRow("Participants"));
  csv.push(csvRow("Individuals who were assigned to the session"));
  csv.push(
    csvRow(
      "Family Name",
      "Given Name",
      "Job Title",
      "Email",
      "Session Role",
      "Session Reaction",
      `Last Updated (${tz})`
    )
  );
  audience.forEach((part: Participant) => {
    csv.push(
      csvRow(
        part.person.user.familyName,
        part.person.user.givenName,
        part.person.user.title,
        part.person.user.email,
        Utils.participantRoleString(part.participantRole),
        Utils.participantReactionString(part.reactionType),
        Utils.formatDateTime(part.updatedUtc)
      )
    );
  });
  csv.push(csvRow(""));
}

function playEvents(
  reportData: SessionContentDetailsReport,
  csv: string[],
  csvRow: (...data: string[]) => string,
  tz: string
) {
  const playEvents = reportData.plays;
  csv.push(csvRow("Play Events"));
  csv.push(
    csvRow(
      "One entry per clip play, the same person may have played multiple times"
    )
  );
  csv.push(
    csvRow(
      "Family Name",
      "Given Name",
      "Job Title",
      "Email",
      `Date Played (${tz})`
    )
  );
  playEvents.forEach((play: SessionContentPersonEvent) => {
    csv.push(
      csvRow(
        play.familyName,
        play.givenName,
        play.title,
        play.email,
        Utils.formatDateTime(play.dateTimeUtc)
      )
    );
  });
  csv.push(csvRow(""));
}

function consumeCountOverTime(
  reportData: SessionContentDetailsReport,
  csv: string[],
  csvRow: (...data: string[]) => string,
  tz: string
) {
  let timeSeries = reportData.consumeCountOverTime.items;
  timeSeries = Utils.fillMissingDates(timeSeries);
  csv.push(csvRow("Play Events Over Time"));
  csv.push(
    csvRow("One entry per date the clip was consumed by at least one person")
  );
  csv.push(csvRow(`Date (${tz})`, "Play Count"));
  timeSeries.forEach((day: ActivityRangeItem) => {
    csv.push(csvRow(Utils.formatDate(day.label), day.value + ""));
  });
  csv.push(csvRow(""));
}
