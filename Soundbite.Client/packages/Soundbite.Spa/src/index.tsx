import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter } from "react-router-dom";
import App from "./App";
import {
  SbBootstrapStyles,
  SbDashboardStyles,
  SbStyles,
  VideoEditorContext,
} from "@soundbite/widgets-react";

const rootElement = document.getElementById("root");
VideoEditorContext.loadDevices();

ReactDOM.render(
  <React.Fragment>
    <SbBootstrapStyles />
    <SbDashboardStyles />
    <SbStyles />
    <BrowserRouter>
      <App />
    </BrowserRouter>
  </React.Fragment>,
  rootElement
);
