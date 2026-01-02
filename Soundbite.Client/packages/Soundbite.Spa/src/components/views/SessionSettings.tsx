import React from "react";
import { observer } from "mobx-react-lite";

import { Private } from "./Private";
import { SessionSettingsCard } from "../cards/SessionSettingsCard";

export const SessionSettings: React.FC = observer(() => {
  return (
    <Private>
      <SessionSettingsCard />
    </Private>
  );
});
