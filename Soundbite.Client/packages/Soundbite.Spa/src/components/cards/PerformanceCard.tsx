import {
  OrganizationsService,
  ReportService,
  SessionSecurityType,
  SpaService,
} from "@soundbite/api";
import { Loader, ShowWhen, WidgetStore } from "@soundbite/widgets-react";
import { observer } from "mobx-react-lite";
import React, { useState } from "react";
import { useEffect } from "react";
import { useParams } from "react-router-dom";
import {
  Card,
  CardHeader,
  CardBody,
  Button,
  Row,
  Col,
  Alert,
} from "reactstrap";

enum PerfRating {
  Indifferent = "",
  Success = "success",
  Warning = "warning",
  Danger = "danger",
}

function ratingForTime(
  time: number,
  successTime: number = 1,
  warningTime: number = 2
): PerfRating {
  if (time < successTime) {
    return PerfRating.Success;
  } else if (time < warningTime) {
    return PerfRating.Warning;
  } else {
    return PerfRating.Danger;
  }
}

interface PerformanceMetric {
  method: string;
  name: string;
  measure: string;
  rating?: PerfRating;
}

interface MetricCardProps {
  metric: PerformanceMetric;
}

export const MetricCard: React.FC<MetricCardProps> = observer(
  (props: MetricCardProps) => {
    const rating = props.metric?.rating ?? PerfRating.Indifferent;
    return (
      <Card className="mb-4 mb-xl-0 shadow" title={props.metric.method}>
        <CardBody>
          <h4 className="mb-0">{props.metric.name}</h4>
          <span className={`text-${rating} font-weight-bold`}>
            {props.metric.measure}
          </span>
        </CardBody>
      </Card>
    );
  }
);

export const PerformanceCard: React.FC = observer(() => {
  // Params
  let { orgRoute } = useParams<{ orgRoute: string }>();

  // State
  const [isRunning, setIsLoading] = useState(false);
  const [metrics, setMetrics] = useState<PerformanceMetric[] | undefined>(
    undefined
  );
  const [needsGroup, setNeedsGroup] = useState(false);
  const [needsSession, setNeedsSession] = useState(false);

  const measureSeconds = async (func: () => Promise<void>): Promise<number> => {
    const startTime = new Date().getTime();
    await func();
    const endTime = new Date().getTime();
    const seconds = ((endTime - startTime) / 1000).toFixed(2);
    return Number(seconds);
  };

  const init = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await SpaService.load(orgRoute);
    });
    return {
      method: "SpaService.load",
      name: "First-time app load",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const myOrgs = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.organizations.readMyOrgsAsync(true);
    });
    return {
      method: "WidgetStore.organizations.getMyOrgsAsync",
      name: "All current user organizations",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const orgFeed = async (): Promise<PerformanceMetric> => {
    // Use the default session security type when acquiring metrics.
    const securityType =
      WidgetStore.organizations.currentOrg?.settings.sessions
        .defaultSessionSecurity ?? SessionSecurityType.Protected;
    const seconds = await measureSeconds(async () => {
      await WidgetStore.sessions.readOrgFeedAsync(
        orgRoute,
        securityType,
        false
      );
    });
    return {
      method: "WidgetStore.sessions.readOrgFeedAsync",
      name: "Active sessions for an organization",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const orgSeries = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.sessions.readOrgSeriesAsync(orgRoute, false);
    });
    return {
      method: "WidgetStore.sessions.readOrgSeriesAsync",
      name: "Active series for an organization",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const people = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.people.readAllAsync(orgRoute);
    });
    return {
      method: "WidgetStore.people.readAllAsync",
      name: "List of all users, plus people picker",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const groups = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.organizations.readGroupsAsync(orgRoute, false);
    });
    return {
      method: "WidgetStore.organizations.readGroupsAsync",
      name: "List of all groups in the org",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const myGroups = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.organizations.readMyGroupsAsync(orgRoute, false);
    });
    return {
      method: "WidgetStore.organizations.readMyGroupsAsync",
      name: "Groups for current user in current org",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const refreshWidgetStore = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.refresh();
    });
    return {
      method: "WidgetStore.refresh",
      name: "Store Cache Refresh",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const sessionDetails = async (
    sessionRoute: string
  ): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.sessions.readSessionDetailsAsync(
        orgRoute,
        sessionRoute,
        false
      );
    });
    return {
      method: "WidgetStore.sessions.readSessionDetailsAsync",
      name: "Loaded in the record and play widgets",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const sessionActivityReport = async (
    sessionRoute: string
  ): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await ReportService.sessionReportAsync(orgRoute, sessionRoute);
    });
    return {
      method: "ReportService.sessionReportAsync",
      name: "Per Session Reporting",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const activityReport = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await ReportService.activityReportAsync(orgRoute);
    });
    return {
      method: "ReportService.activityReportAsync",
      name: "Activity Reporting",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const contentReport = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await ReportService.contentReportAsync(orgRoute);
    });
    return {
      method: "ReportService.contentReportAsync",
      name: "Content Reporting",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const readNotificationSettings = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await OrganizationsService.readNotificationSettingsAsync(orgRoute);
    });
    return {
      method: "OrganizationsService.readNotificationSettingsAsync",
      name: "Read Notification Settings",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const readPermissions = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await OrganizationsService.readPermissionsAsync(orgRoute);
    });
    return {
      method: "OrganizationsService.readPermissionsAsync",
      name: "Read Permissions",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const readAzureSettings = async (): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await OrganizationsService.readAzureSettingsAsync(orgRoute);
    });
    return {
      method: "OrganizationsService.readAzureSettingsAsync",
      name: "Read Azure Settings",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const groupDetails = async (
    groupRoute: string
  ): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.groups.readGroupAsync(orgRoute, groupRoute, false);
    });
    return {
      method: "WidgetStore.groups.readGroupAsync",
      name: "Details for a group",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const groupMembers = async (
    groupRoute: string
  ): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.members.readMembersAsync(orgRoute, groupRoute);
    });
    return {
      method: "WidgetStore.groups.readMembersAsync",
      name: "List of members for a given group",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const groupFeed = async (groupRoute: string): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.sessions.readGroupFeedAsync(
        orgRoute,
        groupRoute,
        WidgetStore.organizations.currentOrg?.settings.sessions
          .defaultSessionSecurity ?? SessionSecurityType.Protected,
        false
      );
    });
    return {
      method: "WidgetStore.sessions.readGroupFeedAsync",
      name: "Active Sessions for a group",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  const groupSeries = async (
    groupRoute: string
  ): Promise<PerformanceMetric> => {
    const seconds = await measureSeconds(async () => {
      await WidgetStore.sessions.readGroupSeriesAsync(
        orgRoute,
        groupRoute,
        false
      );
    });
    return {
      method: "WidgetStore.sessions.readGroupSeriesAsync",
      name: "Active Series for a group",
      measure: `${seconds} seconds`,
      rating: ratingForTime(seconds),
    };
  };

  /** Fire all perf tests */
  const run = async () => {
    setIsLoading(true);

    const sessionRoute = (
      await WidgetStore.sessions.readOrgFeedAsync(
        orgRoute,
        WidgetStore.organizations.currentOrg?.settings.sessions
          .defaultSessionSecurity ?? SessionSecurityType.Protected
      )
    )[0]?.route;
    const groupRoute = (
      await WidgetStore.organizations.readMyGroupsAsync(orgRoute)
    )[0]?.route;
    // Set current group to the one we just found
    if (groupRoute != null) {
      await WidgetStore.groups.readGroupAsync(orgRoute, groupRoute);
    }

    const tests = [
      myOrgs(),
      init(),
      orgFeed(),
      orgSeries(),
      groups(),
      myGroups(),
      refreshWidgetStore(),
      people(),
      activityReport(),
      contentReport(),
      readNotificationSettings(),
      readPermissions(),
      readAzureSettings(),
    ];

    if (sessionRoute != null) {
      setNeedsSession(false);
      tests.push(sessionDetails(sessionRoute));
      tests.push(sessionActivityReport(sessionRoute));
    } else {
      setNeedsSession(true);
    }

    if (groupRoute != null) {
      setNeedsGroup(false);
      tests.push(
        groupDetails(groupRoute),
        groupMembers(groupRoute),
        groupFeed(groupRoute),
        groupSeries(groupRoute)
      );
    } else {
      setNeedsGroup(true);
    }

    const results = await Promise.all(tests);
    setMetrics(results);
    setIsLoading(false);
  };

  // Run on first load
  useEffect(() => {
    run();
    // eslint-disable-next-line
  }, []);

  const spinClass = isRunning ? "fa-spin" : "";

  return (
    <Card>
      <Loader
        isLoadedWhen={!!WidgetStore.organizations.currentOrg}
        style={{ minHeight: "12rem" }}
      >
        <CardHeader className="border-0">
          <Row className="align-items-center">
            <Col md="11">
              <h1 className="mb-0">
                <i className="fas fa-flag-checkered d-none d-sm-inline"></i>
                {WidgetStore.organizations.currentOrg?.details.name} Performance
              </h1>
              <div className="mt-0 text-muted">
                Check performance given the size and complexity of your
                organization
              </div>
            </Col>
            <Col md="1" className="text-right">
              <Button
                outline={true}
                color="secondary"
                className="btn-icon btn-sm"
                onClick={run}
                title="Refresh"
              >
                <span className="btn-inner--icon">
                  <i className={`fas fa-sync-alt ${spinClass}`}></i>
                </span>
              </Button>
            </Col>
          </Row>
        </CardHeader>
        <CardBody>
          <ShowWhen is={needsGroup}>
            <Alert color="light">
              Create at least one group to run group-related performance tests
            </Alert>
          </ShowWhen>
          <ShowWhen is={needsSession}>
            <Alert color="light">
              Create at least one session to run session-specific performance
              tests
            </Alert>
          </ShowWhen>
          <Loader
            isLoadedWhen={!isRunning}
            message="Running Performance Tests..."
          >
            <ShowWhen is={metrics != null}>
              <Row className="mb-2" md="1" lg="2">
                {metrics?.map((metric: PerformanceMetric, index: number) => {
                  return (
                    <Col key={metric.method} lg={6} xl={3} className="mt-1">
                      <MetricCard metric={metric} />
                    </Col>
                  );
                })}
              </Row>
            </ShowWhen>
          </Loader>
        </CardBody>
      </Loader>
    </Card>
  );
});
