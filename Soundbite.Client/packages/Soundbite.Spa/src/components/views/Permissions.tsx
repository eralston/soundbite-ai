import React from "react";
import { observer } from "mobx-react-lite";

import { Private } from "./Private";
import { PermissionsCard } from "../cards/PermissionsCard";

export const Permissions: React.FC = observer(() => {
  return (
    <Private>
      <PermissionsCard />
    </Private>
  );
});
