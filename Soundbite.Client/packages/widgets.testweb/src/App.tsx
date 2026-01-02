import React from "react";
import { Route, Switch } from "react-router-dom";
import { IFrameTestView } from "./views/IFrameTest";

import "./App.css";

function App() {
  return (
    <Switch>
      <Route path="/">
        <IFrameTestView />
      </Route>
    </Switch>
  );
}

export default App;
