import React, { useEffect } from "react";
import { observer } from "mobx-react-lite";
import {
  checkDialogHash,
  FeedMode,
  FeedWidget,
} from "@soundbite/widgets-react";
import { Private } from "./Private";
import { SessionSecurityType } from "../../../../api/dist";
import { IconProp } from "@fortawesome/fontawesome-svg-core";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute: string;
  groupRoute: string;
  securityType: SessionSecurityType;
  icon?: IconProp;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const Feed: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  // When the widget first loads
  useEffect(() => {
    checkDialogHash(props.orgRoute, props.groupRoute);
    // eslint-disable-next-line
  }, []);

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <Private>
      <FeedWidget
        groupRoute={props.groupRoute}
        icon={props.icon}
        orgRoute={props.orgRoute}
        securityType={props.securityType}
        mode={FeedMode.Card}
      />
    </Private>
  );
});
