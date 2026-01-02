import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";
import { useParams } from "react-router-dom";
import {
  checkDialogHash,
  SeriesWidget,
  WidgetStore,
} from "@soundbite/widgets-react";

import { Private } from "./Private";

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const Calendar: React.FC = observer(() => {
  //////////[ Variables ]///////////////////////////////////////////////////////////////////////////

  let { orgRoute, groupRoute } = useParams<{
    orgRoute: string;
    groupRoute: string;
  }>();

  // When the widget first loads
  useEffect(() => {
    checkDialogHash(orgRoute, groupRoute);
    // eslint-disable-next-line
  }, []);

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <Private>
      <SeriesWidget
        orgRoute={orgRoute}
        groupRoute={groupRoute}
        currentUser={WidgetStore.organizations.currentOrg?.details.me}
      />
    </Private>
  );
});
