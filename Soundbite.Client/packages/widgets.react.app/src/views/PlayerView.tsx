import React, { useEffect, useState } from "react";
import {
  SessionDetails,
  SessionsService,
  SoundbiteApiConfig,
  UsersService,
} from "@soundbite/api";
import { InitPlayerWidget } from "@soundbite/widgets-api";
import { AppStore } from "../services/AppStore";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const PlayerView: React.FC<IProps> = (props: IProps) => {
  let [session, setSession] = useState<SessionDetails | undefined>();
  let [userRoute, setUserRoute] = useState<string>("");

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  useEffect(() => {
    (function initialize() {
      const widgetData = AppStore.CurrentWidgetData as InitPlayerWidget;
      SoundbiteApiConfig.getToken = () =>
        Promise.resolve(widgetData?.authToken || null);
      SessionsService.readAsync(
        widgetData.orgRouteIsUid
          ? `${widgetData.orgRoute}`
          : widgetData.orgRoute,
        widgetData.sessionRoute
      ).then((data) => setSession(data));
      setSession(session);
      if (widgetData.userRouteIsUid) {
        UsersService.getRouteFromUniversalId(widgetData.userRoute).then(
          (data) => setUserRoute(data)
        );
      } else {
        setUserRoute(widgetData.userRoute);
      }
    })();
    // eslint-disable-next-line
  }, []); // Calling useEffect with [] as the second parameter ensures one-time execution

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  if (userRoute !== "" && session != null) {
    return <div>PLAYER WIDGET</div>;
  } else {
    return <div>Loading ...</div>;
  }
};
