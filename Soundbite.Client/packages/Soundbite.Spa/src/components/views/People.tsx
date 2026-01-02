import React from "react";
import { observer } from "mobx-react-lite";
import { useParams } from "react-router-dom";

import { PeopleWidget } from "@soundbite/widgets-react";

import { Private } from "./Private";

export const People: React.FC = observer(() => {
  let { orgRoute } = useParams<{ orgRoute: string }>();
  return (
    <Private>
      <PeopleWidget orgRoute={orgRoute} />
    </Private>
  );
});
