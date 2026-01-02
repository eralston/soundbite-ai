import { observer } from "mobx-react-lite";
import React from "react";
import { Button } from "reactstrap";

interface IProps {
  onGrant: () => void;
}

/** React component depicting the AAD Sync-specific sync settings */
export const AadGrant: React.FC<IProps> = observer((props: IProps) => {
  return (
    <div>
      <h3>Getting Started</h3>
      <p>
        To get started, an Azure Active Directory (AAD or AD) administrator must
        first grant Soundbite the{" "}
        <a
          target="_blank"
          rel="noreferrer"
          href="https://docs.microsoft.com/en-us/graph/permissions-reference#application-permissions-21"
        >
          Directory.Read.All Application permission
        </a>
        , which allows for read-only access to AAD without user interaction.
      </p>
      <p>
        Administrators can read and configure{" "}
        <a href="https://portal.azure.com/#blade/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/Overview">
          Azure Active Directory settings in the Azure Portal
        </a>
        . If you need any assistance for security review or understanding the
        best directory sync approach for your organization, please reach out to{" "}
        <a href="mailto:Support@Soundbite.Freshdesk.Com">
          Support@Soundbite.Freshdesk.Com
        </a>
        .
      </p>
      <p>
        <Button
          color="primary"
          className="btn-icon btn-3"
          onClick={props.onGrant}
        >
          <span className="btn-inner--icon">
            <i className="fas fa-unlock"></i>&nbsp;
          </span>
          <span className="btn-inner--text">Grant Admin Access</span>
        </Button>
      </p>
    </div>
  );
});
