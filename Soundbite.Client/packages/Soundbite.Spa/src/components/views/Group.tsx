import React from "react";
import { observer } from "mobx-react-lite";
import { useParams } from "react-router-dom";

import { MemberCard } from "../cards/MemberCard";
import { Private } from "./Private";

export const Group: React.FC = observer(() => {
  let { orgRoute, groupRoute } = useParams<{
    orgRoute: string;
    groupRoute: string;
  }>();

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <Private>
      <MemberCard orgRoute={orgRoute} groupRoute={groupRoute} />
    </Private>
  );
});
