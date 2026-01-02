import React from "react";
import { observer } from "mobx-react-lite";
import { useParams } from "react-router-dom";

import { GroupsCard } from "../cards/GroupsCard";
import { Private } from "./Private";

export const Groups: React.FC = observer(() => {
  let { orgRoute } = useParams<{ orgRoute: string }>();
  return (
    <Private>
      <GroupsCard orgRoute={orgRoute} />
    </Private>
  );
});
