import { observer } from "mobx-react-lite";
import React, { useEffect, useState } from "react";

import {
  OrgSyncResult,
  OrgSyncState,
  SyncService,
  Utils,
} from "@soundbite/api";
import { Button, Col, Row } from "reactstrap";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCalendarCheck, faSyncAlt } from "@fortawesome/free-solid-svg-icons";
import { Loader, ShowWhen, WidgetStore } from "@soundbite/widgets-react";

interface IProps {
  orgRoute?: string;
}

function valWithDelta(result: number, deltaUp: number, deltaDown: number) {
  let deltaTxt = "";
  if (deltaUp > 0) {
    deltaTxt += `+${deltaUp}`;
  }

  if (deltaDown > 0) {
    if (deltaUp > 0) {
      deltaTxt += ", ";
    }

    deltaTxt += `-${deltaDown}`;
  }

  if (deltaTxt.length > 0) {
    deltaTxt = ` (${deltaTxt})`;
  }

  return `${result}${deltaTxt}`;
}

function statusForResult(result: OrgSyncResult) {
  if (result.isFailed) {
    return <span className="text-danger">Failed</span>;
  }

  if (result.state === OrgSyncState.Pending) {
    return <span className="text-muted">Pending</span>;
  }

  if (result.state === OrgSyncState.Cancelled) {
    return <span className="text-warning">Cancelled</span>;
  }

  if (result.state === OrgSyncState.Processing) {
    return <span className="text-muted">Processing</span>;
  }

  return <span className="text-success">Success</span>;
}

export const SyncHistory: React.FC<IProps> = observer((props: IProps) => {
  const [orgName, setOrgName] = useState("");
  const [results, setResults] = useState<OrgSyncResult[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const load = async () => {
    if (props.orgRoute == null) {
      setResults([]);
      return;
    }

    setIsLoading(true);
    const newResults = await SyncService.readResults(props.orgRoute, {
      take: 10,
    });
    setResults(newResults.result);
    setIsLoading(false);
  };

  useEffect(() => {
    if (props.orgRoute == null) {
      return;
    }

    // We want the first items plus the metadata for the org
    const onFirstLoad = async () => {
      if (props.orgRoute == null) {
        return;
      }

      setIsLoading(true);
      const orgPromise = WidgetStore.organizations.readOrgAsync(props.orgRoute);
      const resultsPromise = SyncService.readResults(props.orgRoute, {
        take: 10,
      });

      const [newOrg, newResults] = await Promise.all([
        orgPromise,
        resultsPromise,
      ]);
      setOrgName(newOrg.details.name);
      setResults(newResults.result);
      setIsLoading(false);
    };
    onFirstLoad();
  }, [props.orgRoute]);

  if (props.orgRoute == null) {
    return <div />;
  }

  return (
    <div className="sb-sync-results-table">
      <Row>
        <Col md={9}>
          <h2>
            <FontAwesomeIcon
              icon={faCalendarCheck}
              className="d-none d-sm-inline mr-1"
            />
            {orgName} Sync History
          </h2>
        </Col>
        <Col md={3} className="text-right">
          <ShowWhen is={!isLoading}>
            <Button
              color="secondary"
              outline={true}
              onClick={() => {
                load();
              }}
              size="sm"
              title="Refresh Feed"
            >
              <span className="btn-inner--icon">
                <FontAwesomeIcon icon={faSyncAlt} />
              </span>
            </Button>
          </ShowWhen>
        </Col>
      </Row>
      <Loader isLoadedWhen={!isLoading}>
        <table className="mb-3 table">
          <thead className="thead-light">
            <tr>
              <th>Date</th>
              <th>Status</th>
              <th>Duration</th>
              <th>Users</th>
              <th>Groups</th>
            </tr>
          </thead>
          <tbody>
            {results.map((result: OrgSyncResult) => {
              return (
                <tr key={result.universalId}>
                  <td>{Utils.formatDateTime(result.startTime)}</td>
                  <td>{statusForResult(result)}</td>
                  {result.deltaTime === 0 ? (
                    <td></td>
                  ) : (
                    <td>{Utils.secondsToString(result.deltaTime)}</td>
                  )}
                  <td>
                    {valWithDelta(result.usersUpdated, result.usersAdded, 0)}
                  </td>
                  <td>
                    {valWithDelta(
                      result.groupsUpdated,
                      result.groupsAdded,
                      result.groupsRemoved
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </Loader>
    </div>
  );
});
