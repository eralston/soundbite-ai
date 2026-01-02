import React from "react";
import { observer } from "mobx-react-lite";

import { Private } from "./Private";
import { PerformanceCard } from "../cards/PerformanceCard";

export const Performance: React.FC = observer(() => {
  return (
    <Private>
      <PerformanceCard />
    </Private>
  );
});
