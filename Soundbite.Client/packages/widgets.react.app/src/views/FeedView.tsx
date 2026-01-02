import React, { useEffect, useState } from "react";
import { Person, SessionPreview, SessionSecurityType } from "@soundbite/api";
import { InitFeedWidget } from "@soundbite/widgets-api";
import { FeedWidget, WidgetStore } from "@soundbite/widgets-react";

import { observer } from "mobx-react-lite";
import { AppStore } from "../services/AppStore";
import { LoadState } from "../models/Enums";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const FeedView: React.FC<IProps> = observer((props: IProps) => {
  let [sessions, setSessions] = useState<SessionPreview[]>([]);
  let [orgRoute, setOrgRoute] = useState("");
  let [groupRoute, setGroupRoute] = useState("");
  let [currentUser, setCurrentUser] = useState<Person>();

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  // Calling useEffect with [] as the second parameter ensures one-time execution
  useEffect(() => {
    (async function initialize() {
      const widgetData = AppStore.CurrentWidgetData as InitFeedWidget;
      await AppStore.initializeAsync(widgetData.authToken, widgetData.orgRoute);

      // Populate data from the initialized application store
      setOrgRoute(AppStore.MyOrgRoute);
      setCurrentUser(AppStore.MyUser);

      // Determine whether team-specific feed data needs to be loaded
      if (widgetData.groupRoute) {
        setGroupRoute(widgetData.groupRoute);
        await refreshTeamData(widgetData.orgRoute, widgetData.groupRoute);
      } else {
        await refreshOrgData(widgetData.orgRoute);
      }
    })();
  }, []);

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  async function refreshTeamData(orgRoute: string, groupRoute: string) {
    setSessions([]);
    //TODO: Figure out how to suppport protected/public feeds
    const teamFeed = await WidgetStore.sessions.readGroupFeedAsync(
      orgRoute,
      groupRoute,
      SessionSecurityType.Protected,
      false
    );
    setSessions(teamFeed);
  }

  async function refreshOrgData(orgRoute: string) {
    setSessions([]);
    //TODO: Figure out how to suppport protected/public feeds
    const feed = await WidgetStore.sessions.readOrgFeedAsync(
      orgRoute,
      SessionSecurityType.Protected,
      false
    );
    setSessions(feed);
  }

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function getContent() {
    switch (AppStore.AppStoreState) {
      case LoadState.Loading:
        return <div>Loading ...</div>;
      case LoadState.Failed:
        return <div>{AppStore.AppStoreErrorMsg}</div>;
      case LoadState.Loaded:
        return (
          <FeedWidget
            title="Soundbite Feed"
            orgRoute={orgRoute}
            groupRoute={groupRoute}
            sessions={sessions}
            currentUser={currentUser}
          />
        );
      default:
        return <div>Feed Widget...</div>;
    }
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return <div>{getContent()}</div>;
});
