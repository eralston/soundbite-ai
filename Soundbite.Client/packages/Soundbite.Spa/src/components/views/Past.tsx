import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";

import { checkDialogHash, PastSessionsWidget } from "@soundbite/widgets-react";

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
export const Past: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  // When the widget first loads
  useEffect(() => {
    checkDialogHash(props.orgRoute, props.groupRoute);
    // eslint-disable-next-line
  }, []);

  return (
    <Private>
      <PastSessionsWidget
        orgRoute={props.orgRoute}
        groupRoute={props.groupRoute}
      />
    </Private>
  );
});
