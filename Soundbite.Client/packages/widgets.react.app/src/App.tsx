import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";
import { SoundbiteApiConfig } from "@soundbite/api";
import { setupAxiosAdapter } from "@soundbite/api-axios";
import {
  SbBootstrapStyles,
  SbDashboardStyles,
  SbStyles,
  DialogManagerUi,
} from "@soundbite/widgets-react";

import { AppStore } from "./services/AppStore";
import { WidgetService } from "./services/WidgetService";
import "./App.css";
//import "./Soundbite.css";

import { FeedView } from "./views/FeedView";
import { PlayerView } from "./views/PlayerView";
import { SessionView } from "./views/SessionView";
import { TeamsView } from "./views/TeamsView";

const App: React.FC = observer(() => {
  setupAxiosAdapter();

  // The window.SbApiPrefixUrl value is defined in the public\index.html page
  SoundbiteApiConfig.ApiPrefixUrl = (window as any).SbApiPrefixUrl;

  function ShowSelectedWidget() {
    switch (AppStore.CurrentWiget) {
      case "Feed":
        return <FeedView />;
      case "Player":
        return <PlayerView />;
      case "Session":
        return <SessionView />;
      case "Teams":
        return <TeamsView />;
      default:
        return <div>Waiting for Widget Request...</div>;
    }
  }

  // The following starts a window listener
  useEffect(() => {
    const handler = (event: MessageEvent) => {
      WidgetService.onWindowMessage(event);
    };

    // Attach a handler for the window message event
    window.addEventListener("message", handler);

    // Remove the handler for the window message event
    return () => window.removeEventListener("message", handler);
  }, []); // empty array => run only once

  return (
    <div>
      <SbBootstrapStyles />
      <SbDashboardStyles />
      <SbStyles />
      {ShowSelectedWidget()}
      <DialogManagerUi />
    </div>
  );
});

export default App;
