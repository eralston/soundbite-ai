import React from "react";
import {
  WidgetManager,
  InitFeedWidget,
  InitPlayerWidget,
  InitSessionWidget,
  InitTeamsWidget,
} from "@soundbite/widgets-api";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  children?: React.ReactNode;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
const Component: React.FC<IProps> = (props: IProps) => {
  // ERIK VARIABLES
  const erikAuthKey =
    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1dWlkIjoiYjJiZTU2ZmItMGI4Ni00NGM2LWIwYWItOGEwMGU5MGM2MWJlIiwib3VpZCI6IjIiLCJuYmYiOjE2MTcwNjUxODIsImV4cCI6MTY0ODYwMTE4MiwiaWF0IjoxNjE3MDY1MTgyfQ.DWEFywRPjkymO9zCcCTtSTuXNzqhbVsW4JDKF0thhKQ";
  const erikUserRoute = "b2be56fb-0b86-44c6-b0ab-8a00e90c61be";
  const erikOrgRoute = "hk8hPNOW";
  const erikSessionRoute = "EpfIC9od";
  const authKey = erikAuthKey;
  const sessionRoute = erikSessionRoute;
  const orgRoute = erikOrgRoute;
  const userRoute = erikUserRoute;

  // DAMON VARIABLES
  //const authKeyDamonSb = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1dWlkIjoiZGE2ZWZmMmEtNGMzNi00ZmI0LWI3MGMtYjY2M2U3YjA2OGE5Iiwib3VpZCI6IjEiLCJuYmYiOjE2MTM4Njk5MTAsImV4cCI6MTYyMTY0NTkxMCwiaWF0IjoxNjEzODY5OTEwfQ.xRJCYKXnB5VwVCFN9WzOLglBLcfkCgl7VJy2hXvAnIQ";
  //const authKeyDamonGoT =
  //"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1dWlkIjoiZGE2ZWZmMmEtNGMzNi00ZmI0LWI3MGMtYjY2M2U3YjA2OGE5Iiwib3VpZCI6IjIiLCJuYmYiOjE2MTc0NzM4MTIsImV4cCI6MTY0OTAwOTgxMiwiaWF0IjoxNjE3NDczODEyfQ.RcRkze6mwfkIjXyEVm0bK0DCSc0c6dj592wz1clsK_E";
  //const damonGoTOrgSessionRoute = "q9VCAXjM";
  //const damonGoTOrgRoute = "2"
  //const damonGoTUserRoute = "da6eff2a-4c36-4fb4-b70c-b663e7b068a9"
  //const authKey = authKeyDamonGoT;
  //const sessionRoute = damonGoTOrgSessionRoute;
  //const orgRoute = damonGoTOrgRoute;
  //const userRoute = damonGoTUserRoute

  WidgetManager.widgetUrl = "http://localhost:4000/";

  function showFeedWidget(): void {
    WidgetManager.showFeedWidget(
      "widgetHost",
      new InitFeedWidget(authKey, orgRoute, "")
    ).then(() => {});

    //WidgetManager.onFeedTestButtonClicked = (clickCount) => {
    //  alert(`[On Client Page] - Clicked ${clickCount} times`);
    //};
  }

  function showPlayerWidget(): void {
    WidgetManager.showPlayerWidget(
      "widgetHost",
      new InitPlayerWidget(
        authKey,
        sessionRoute,
        orgRoute,
        userRoute,
        true,
        true
      )
    ).then(() => {});
  }

  function showSessionWidget(): void {
    WidgetManager.showSessionWidget(
      "widgetHost",
      new InitSessionWidget(authKey)
    ).then(() => {});
  }

  function showTeamsWidget(): void {
    WidgetManager.showTeamsWidget(
      "widgetHost",
      new InitTeamsWidget(authKey)
    ).then(() => {});
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  let iFrameStyle = { width: "98%", height: "450px" };

  return (
    <div className="frame">
      <h1>Contoso Dashboard</h1>
      <h2 className="info-text">Soundbite.AI Widgets in an iFrame</h2>
      <iframe
        src="http://localhost:4000/"
        id="widgetHost"
        style={iFrameStyle}
        title="Soundbite Widgets in an iFrame"
      />
      <br />
      <div>
        <button onClick={showFeedWidget}>Feed Widget</button>
        <button onClick={showPlayerWidget}>Player Widget</button>
        <button onClick={showSessionWidget}>Soundbite Widget</button>
        <button onClick={showTeamsWidget}>Teams Widget</button>
      </div>
    </div>
  );
};

export const IFrameTestView = Component;
