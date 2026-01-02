import React from "react";
import { observer } from "mobx-react-lite";

import { SyncCard } from "../cards/SyncCard";
import { Private } from "./Private";

export const Sync: React.FC = observer(() => {
  return (
    <Private>
      <SyncCard />
    </Private>
  );
});
