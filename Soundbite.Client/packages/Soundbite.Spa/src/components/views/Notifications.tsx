import React from "react";
import { observer } from "mobx-react-lite";

import { Private } from "./Private";
import { NotificationsCard } from "../cards/NotificationsCard";
import { TeamsNotificationsCard } from "../cards/TeamsNotificationsCard";
import { AcsCard } from "../cards/AcsCard";

export const Notifications: React.FC = observer(() => {
  return (
    <Private>
      <NotificationsCard />
      <TeamsNotificationsCard className="mt-4" />
      <AcsCard className="mt-4" />
    </Private>
  );
});
