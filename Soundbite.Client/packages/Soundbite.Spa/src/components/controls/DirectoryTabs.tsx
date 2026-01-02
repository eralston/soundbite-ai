import React from "react";
import { NavItem } from "reactstrap";
import { NavLink, useParams } from "react-router-dom";
import {
  faUserPlus,
  faUserCog,
  faUsersCog,
} from "@fortawesome/free-solid-svg-icons";

import { IconLink, ShowWhen, WidgetStore } from "@soundbite/widgets-react";
import { PersonRole } from "@soundbite/api";

export const DirectoryTabs = () => {
  let { orgRoute } = useParams<{ orgRoute: string }>();
  const isAdminOrMore = WidgetStore.isPersonRole(PersonRole.Admin);

  return (
    <ul className="sb-tabs">
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/Organizations/${orgRoute}/directory/users`}
          title="Users"
        >
          <IconLink title="Users" icon={faUserCog} />
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink
          activeClassName="active"
          to={`/Organizations/${orgRoute}/directory/groups`}
          title="Groups"
        >
          <IconLink title="Groups" icon={faUsersCog} />
        </NavLink>
      </NavItem>
      <ShowWhen is={isAdminOrMore}>
        <NavItem>
          <NavLink
            activeClassName="active"
            to={`/Organizations/${orgRoute}/directory/sync`}
            title="Sync"
          >
            <IconLink title="Sync" icon={faUserPlus} />
          </NavLink>
        </NavItem>
      </ShowWhen>
    </ul>
  );
};
