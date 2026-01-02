import React, { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { ITeamsBaseComponentProps } from "msteams-react-base-component";
import { SessionEditorWidget, WidgetStore } from "@soundbite/widgets-react";
import { SessionDetails } from "@soundbite/api";
import { observer } from "mobx-react-lite";

/***************************************************************************************************
 *  Component Props
 **************************************************************************************************/
export interface ICreateSoundbiteViewProps extends ITeamsBaseComponentProps {}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const CreateSoundbiteView: React.FC = observer(() => {
  function onClose(isSaved: boolean) {
    if (!isSaved) {
      // Submit a null task which effectively cancels the action
      microsoftTeams.tasks.submitTask();
    }
  }

  function onSaved(session: SessionDetails) {
    microsoftTeams.tasks.submitTask({
      cmd: "sessionCreated",
      route: session.route,
      title: session.name,
      orgRoute: WidgetStore.organizations.currentOrg?.details.route,
    });
    alert(session.route);
  }

  /**
   * Responsible for setting up the initial state of the component.
   */
  async function initialize(): Promise<void> {}

  useEffect(() => {
    initialize();
  }, []);

  return (
    <div>
      <SessionEditorWidget onClose={onClose} onSaved={onSaved} />
    </div>
  );
});
