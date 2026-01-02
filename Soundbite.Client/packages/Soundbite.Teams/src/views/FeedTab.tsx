import React from "react";
import { TeamsAppStore } from "../services/TeamsAppStore";
import {
  DialogManagerUi,
  MegaWidget,
  SbBootstrapStyles,
  SbDashboardStyles,
  SbStyles,
  WidgetStore,
} from "@soundbite/widgets-react";
import { observer } from "mobx-react-lite";
import { useLocation } from "react-router-dom";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  children?: React.ReactNode;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const FeedTab: React.FC<IProps> = observer((props: IProps) => {
  //////////[ Variables ]///////////////////////////////////////////////////////////////////////////

  let { search } = useLocation();
  const qsParams = new URLSearchParams(search);
  const groupRoute: string | undefined = qsParams.get("teamRoute") ?? undefined;
  const title = qsParams.get("title") ?? undefined;
  const orgRoute = WidgetStore.organizations.currentOrg?.details?.route ?? "";

  //////////[ Initiaize ]///////////////////////////////////////////////////////////////////////////

  React.useEffect(() => {
    async function initialize() {
      if (TeamsAppStore.initComplete) {
        await TeamsAppStore.initialize();
      }
    }
    initialize();
    // eslint-disable-next-line
  }, [WidgetStore.organizations.currentOrg, groupRoute]);

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <React.Fragment>
      <SbBootstrapStyles />
      <SbDashboardStyles />
      <SbStyles />
      <MegaWidget
        title={title}
        showTitle={true}
        orgRoute={orgRoute}
        groupRoute={groupRoute}
      />
      <DialogManagerUi />
    </React.Fragment>
  );
});
