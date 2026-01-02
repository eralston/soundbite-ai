import { observer } from "mobx-react-lite";
import React from "react";

export const SyncDisabled: React.FC = observer(() => {
  return (
    <div>
      <p>
        Directory sync connects your organization's users and groups from your
        directory data every day at midnight{" "}
        <a href="https://en.wikipedia.org/wiki/Coordinated_Universal_Time">
          UTC
        </a>
        .
      </p>
      <p>
        The configuration allows directory sync to fetch information using a
        configured mix of four strategies:
      </p>
      <ul>
        <li>
          <strong>Selected Groups (Recommended)</strong> - Groups you select and
          their members only.
        </li>
        <li>
          <strong>Selected Users</strong> - Users on a selected list, regardless
          of group membership.
        </li>
        <li>
          <strong>All Users</strong> - All licensed users in your organization,
          including potential guest users, which allows maximum use of
          Soundbite.
        </li>
        <li>
          <strong>All Groups</strong> - Loading all groups into the system.
          Convenient for small organizations, but usually too much for large
          ones.
        </li>
      </ul>
      <p>
        Unlike when inviting individuals manually, the synchronization process
        will
        <em>never</em> send welcome messages. Profile data is read from the
        directory and individuals are immediately able to log in to Soundbite
        and see content in the context of their groups. No other action is taken
        by the platform.
      </p>
      <p>
        Most organizations will want start small then scale audience as part of
        their change management process. We recommended starting with a handful
        of selected users or a few groups. Generally the final step for a "full
        deployment" is enabling All Users in addition to curated groups,
        maximizing the potential for Soundbite to support communication across
        the organization.
      </p>
      <p>
        If you have any questions or need any advice on your directory sync
        configuration, please reach out to us at{" "}
        <a href="mailto:Support@Soundbite.Freshdesk.Com">Soundbite Support</a>.
      </p>
    </div>
  );
});
