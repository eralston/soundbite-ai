import React, { useEffect } from "react";
import { SoundbiteApiConfig } from "@soundbite/api";
import { AppStore } from "../services/AppStore";
import { observer } from "mobx-react-lite";
import { InitTeamsWidget } from "@soundbite/widgets-api";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const TeamsView: React.FC<IProps> = observer((props: IProps) => {
  let widgetData: InitTeamsWidget | null = null;

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  async function initialize() {
    widgetData = AppStore.CurrentWidgetData as InitTeamsWidget;
    SoundbiteApiConfig.getToken = () =>
      Promise.resolve(widgetData?.authToken || null);
  }

  useEffect(() => {
    initialize();
    // eslint-disable-next-line
  }, []); // Calling useEffect with [] as the second parameter ensures one-time execution

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return <div>Not Yet Supported</div>;
});
