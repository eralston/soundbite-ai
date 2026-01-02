import React from "react";
import { NavItem } from "reactstrap";
import {
  faCalendar,
  faCalendarCheck,
  faCalendarDay,
  faPodcast,
  faPeopleGroup,
} from "@fortawesome/free-solid-svg-icons";
import { NavLink, useParams } from "react-router-dom";
import { observer } from "mobx-react-lite";

import { ShowWhen, WidgetStore, IconLink } from "@soundbite/widgets-react";

/***************************************************************************************************
 *  Component
 **************************************************************************************************/

const OrgTabs: React.FC = observer(() => {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  let { orgRoute } = useParams<{ orgRoute: string }>();

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <ul className="sb-tabs">
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/organizations/${orgRoute}/feed`}
          title="Feed"
        >
          <IconLink title="Feed" icon={faPodcast} />
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/organizations/${orgRoute}/calendar`}
          title="Calendar"
        >
          <IconLink title="Calendar" icon={faCalendar} />
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/organizations/${orgRoute}/scheduled`}
          title="Scheduled"
        >
          <IconLink title="Scheduled" icon={faCalendarDay} />
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/organizations/${orgRoute}/history`}
          title="History"
        >
          <IconLink title="History" icon={faCalendarCheck} />
        </NavLink>
      </NavItem>
      <ShowWhen
        is={
          WidgetStore.organizations.currentOrg?.hasPublicNotifications === true
        }
      >
        <NavItem>
          <NavLink
            activeClassName="active"
            to={`/organizations/${orgRoute}/public`}
            title="Public"
          >
            <IconLink title="Public" icon={faPeopleGroup} />
          </NavLink>
        </NavItem>
      </ShowWhen>
    </ul>
  );
});

export default OrgTabs;
