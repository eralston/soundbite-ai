import React, { useState } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import { observer } from "mobx-react-lite";
import { Group } from "@soundbite/api";
import { WidgetStore, GroupPicker } from "@soundbite/widgets-react";
import { TeamsAppStore } from "../services/TeamsAppStore";

/***************************************************************************************************
 *  FeedTabConfig Component - this is responsible for displaying the configuration dialog that
 *    Microsoft Teams displays when setting up a tab within a team.  This allows users to select
 *    which teams's feed to display in the tab.
 **************************************************************************************************/
export const FeedTabConfig: React.FC = observer(() => {
  //////////[ State ]///////////////////////////////////////////////////////////////////////////////

  let [title, setTitle] = useState<string>("");
  let [selectedGroup, setSelectedGroup] = useState<Group>();

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Executes when the save button is clicked on the Teams tab configuration screen.
   */
  microsoftTeams.settings.registerOnSaveHandler(
    (saveEvent: microsoftTeams.settings.SaveEvent) => {
      if (selectedGroup) {
        microsoftTeams.settings.setSettings({
          websiteUrl: TeamsAppStore.settings.teamsWebUrl,
          contentUrl: `${TeamsAppStore.settings.teamsWebUrl}FeedTab?teamRoute=${
            selectedGroup.route
          }&title=${encodeURIComponent(title)}`,
          entityId: "sbTab-" + new Date().getTime().toString(),
          suggestedDisplayName: "Soundbites",
        });
        saveEvent.notifySuccess();
      } else {
        saveEvent.notifyFailure("No Team Selected");
      }
    }
  );

  function onGroupSelected(item: any) {
    setSelectedGroup(item);
    if (item) {
      setTitle(`${item.name} Feed`);
      microsoftTeams.settings.setValidityState(true);
    } else {
      microsoftTeams.settings.setValidityState(false);
    }
  }

  function onTitleChanged(e: React.ChangeEvent<HTMLInputElement>) {
    setTitle(e.target.value);
    checkValidity(selectedGroup, e.target.value);
  }

  function checkValidity(group: Group | undefined, title: string): boolean {
    const isValid = !!group && !!title;
    microsoftTeams.settings.setValidityState(isValid);
    return isValid;
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <div className="tabConfig">
      <div>
        Welcome to the Soundbite Teams Tab Configuration Page. This
        configuration screen allows you to choose Soundbite Team feed to display
        on this tab. Please use the Team Picker below to select a team and then
        click Save to create a new Soundbite tab in Microsoft Teams.
      </div>
      <br />
      <div className="configLabel">Team Feed to Display</div>
      <GroupPicker
        isMulti={false}
        onItemSelected={onGroupSelected}
        orgRoute={WidgetStore.organizations.currentOrg?.details.route ?? ""}
      />
      <br />
      <div className="configLabel">Title to Display</div>
      <input type="text" value={title} onChange={onTitleChanged} />
    </div>
  );
});
