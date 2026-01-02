import { ShowWhen, WidgetStore } from "@soundbite/widgets-react";
import { observer } from "mobx-react-lite";
import React from "react";
import { NavLink as RouterLink, useParams } from "react-router-dom";
import { Alert } from "reactstrap";

export const SyncResults: React.FC = observer(() => {
  let { orgRoute } = useParams<{ orgRoute: string }>();
  const result = WidgetStore.sync.result;
  return (
    <div>
      <h3>Sync Results</h3>
      <ShowWhen is={WidgetStore.sync.result?.isFailed}>
        <Alert color="danger">
          Directory sync failed. Please try again. For support, please contact{" "}
          <a
            className="alert-link"
            href="mailto:Support@Soundbite.Freshdesk.Com"
          >
            Soundbite Support
          </a>
          .
          <ShowWhen is={(result?.errorMessages?.length ?? 0) > 0}>
            <ul className="mb-0">
              {result?.errorMessages.map((msg, i) => {
                return <li key={i}>{msg}</li>;
              })}
            </ul>
          </ShowWhen>
        </Alert>
      </ShowWhen>
      <ShowWhen is={!result?.isFailed}>
        <Alert color="success">Directory sync successful.</Alert>
      </ShowWhen>
      <table className="mb-3 table">
        <thead className="thead-light">
          <tr>
            <th>Dimension</th>
            <th>Value</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>
              <strong>Directory Type</strong>
            </td>
            <td>{result?.syncType}</td>
          </tr>
          <tr>
            <td>
              <strong>Duration</strong>
            </td>
            <td>{Math.ceil(result?.deltaTime ?? 0)} Second(s)</td>
          </tr>
          <tr>
            <td>
              <strong>Memory</strong>
            </td>
            <td>{Math.ceil((result?.deltaBytes ?? 0) / 1000000)} MB(s)</td>
          </tr>
          <tr>
            <td>
              <strong>User API Requests</strong>
            </td>
            <td>{result?.userNetworkRequests}</td>
          </tr>
          <tr>
            <td>
              <strong>Users Added</strong>
            </td>
            <td>{result?.usersAdded}</td>
          </tr>
          <tr>
            <td>
              <strong>Users Updated</strong>
            </td>
            <td>{result?.usersUpdated}</td>
          </tr>
          <tr>
            <td>
              <strong>Group API Requests</strong>
            </td>
            <td>{result?.groupNetworkRequests}</td>
          </tr>
          <tr>
            <td>
              <strong>Groups Added</strong>
            </td>
            <td>{result?.groupsAdded}</td>
          </tr>
          <tr>
            <td>
              <strong>Groups Updated</strong>
            </td>
            <td>{result?.groupsUpdated}</td>
          </tr>
          <tr>
            <td>
              <strong>Groups Removed</strong>
            </td>
            <td>{result?.groupsRemoved}</td>
          </tr>
          <tr>
            <td>
              <strong>Members Added</strong>
            </td>
            <td>{result?.membersAdded}</td>
          </tr>
          <tr>
            <td>
              <strong>Members Removed</strong>
            </td>
            <td>{result?.membersRemoved}</td>
          </tr>
        </tbody>
      </table>
      <ShowWhen is={!result?.isFailed}>
        <p className="text-muted">
          To see changes, check your{" "}
          <RouterLink
            activeClassName="active"
            to={`/Organizations/${orgRoute}/directory/users`}
          >
            Users
          </RouterLink>{" "}
          and{" "}
          <RouterLink to={`/Organizations/${orgRoute}/directory/groups`}>
            Groups
          </RouterLink>
          .
        </p>
      </ShowWhen>
    </div>
  );
});
