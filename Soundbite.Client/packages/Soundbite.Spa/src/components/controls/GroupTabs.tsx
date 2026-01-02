import React from "react";
import { NavItem } from "reactstrap";
import { NavLink, useParams } from "react-router-dom";
import {
  faCalendar,
  faCalendarCheck,
  faCalendarDay,
  faPodcast,
  faUsers,
} from "@fortawesome/free-solid-svg-icons";

import { IconLink } from "@soundbite/widgets-react";

export const GroupTabs = () => {
  let { orgRoute, groupRoute } = useParams<{
    orgRoute: string;
    groupRoute: string;
  }>();

  return (
    <ul className="sb-tabs">
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/organizations/${orgRoute}/groups/${groupRoute}/feed`}
          title="Feed"
        >
          <IconLink title="Feed" icon={faPodcast} />
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/organizations/${orgRoute}/groups/${groupRoute}/calendar`}
          title="Calendar"
        >
          <IconLink title="Calendar" icon={faCalendar} />
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/organizations/${orgRoute}/groups/${groupRoute}/scheduled`}
          title="Scheduled"
        >
          <IconLink title="Scheduled" icon={faCalendarDay} />
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/organizations/${orgRoute}/groups/${groupRoute}/history`}
          title="History"
        >
          <IconLink title="History" icon={faCalendarCheck} />
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/organizations/${orgRoute}/groups/${groupRoute}/members`}
          title="Members"
        >
          <IconLink title="Members" icon={faUsers} />
        </NavLink>
      </NavItem>
    </ul>
  );
};
