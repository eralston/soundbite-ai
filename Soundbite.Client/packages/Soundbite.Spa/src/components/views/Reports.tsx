import React from "react";
import { observer } from "mobx-react-lite";
import { useParams } from "react-router-dom";

import { ReportView } from "@soundbite/widgets-react";

import { Private } from "./Private";

export const Reports: React.FC = observer(() => {
  let { orgRoute } = useParams<{ orgRoute: string }>();
  return (
    <Private>
      <ReportView orgRoute={orgRoute} />
    </Private>
  );
});
