import { observer } from "mobx-react-lite";
import React from "react";

/** React component for user introduction to the feature */
export const AadIntro: React.FC = observer(() => {
  return (
    <div>
      <h3>Getting Started With Azure Active Directory</h3>
      <p>
        Thank you for granting permission to the Soundbite platform. We are
        ready to connect to your Azure Active Directory (Microsoft 365/Office
        365) tenant. Once activated, you will be able to sync:
      </p>
      <ul>
        <li>
          <strong>
            <a
              rel="noreferrer"
              target="_blank"
              href="https://portal.azure.com/#blade/Microsoft_AAD_IAM/UsersManagementMenuBlade/MsGraphUsers"
            >
              Users
            </a>
          </strong>
          , including guests, who are members of your organization.{" "}
        </li>
        <li>
          <strong>
            <a
              rel="noreferrer"
              target="_blank"
              href="https://portal.azure.com/#blade/Microsoft_AAD_IAM/GroupsManagementMenuBlade/AllGroups"
            >
              Groups
            </a>
          </strong>{" "}
          in your organization, including M365 and security groups, plus
          distribution lists.
        </li>
      </ul>
      <p>
        Save to activate Active Directory Sync and start configuring your
        organization.
      </p>
    </div>
  );
});
