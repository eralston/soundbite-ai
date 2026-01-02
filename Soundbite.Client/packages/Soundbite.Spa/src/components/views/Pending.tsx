import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";

import {
  checkDialogHash,
  PendingSessionsWidget,
} from "@soundbite/widgets-react";

import { Private } from "./Private";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute: string;
  groupRoute?: string;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const Pending: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  // When the widget first loads
  useEffect(() => {
    checkDialogHash(props.orgRoute, props.groupRoute);
    // eslint-disable-next-line
  }, []);

  return (
    <Private>
      <PendingSessionsWidget
        orgRoute={props.orgRoute}
        groupRoute={props.groupRoute}
      />
    </Private>
  );
});
