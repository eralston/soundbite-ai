import { faPlay } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import React from "react";

import { WidgetStore } from "../..";
import { SbButton, SbButtonSize, SbButtonType } from "../SbButton";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute: string;
  size?: SbButtonSize;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const PlayBtn: React.FC<IProps> = (props: IProps) => {
  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onOpen(): void {
    if (props.orgRoute && props.sessionRoute) {
      WidgetStore.showPlayer(props.orgRoute, props.sessionRoute);
    }
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  const size = props.size ?? SbButtonSize.Small;

  return (
    <React.Fragment>
      <SbButton
        type={SbButtonType.Primary}
        size={size}
        btnClassName="sb-play-btn"
        title="Play Session"
        text="Play"
        onClick={onOpen}
        icon={faPlay}
      />
    </React.Fragment>
  );
};
