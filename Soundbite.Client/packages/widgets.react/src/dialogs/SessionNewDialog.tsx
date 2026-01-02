import React from "react";

import { Utils } from "@soundbite/api";

import { DialogHash } from "../modules/HashHelper";
import { WidgetStore } from "../store/WidgetStore";
import { SessionEditorWidget } from "../widgets/SessionEditorWidget";
import { DialogAction } from "./DialogAction";
import { DialogHelper } from "./DialogHelper";
import { DialogStyleType } from "./DialogStyleType";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  close: () => void;
  isExpanded?: boolean;
  orgRoute: string;
  groupRoute?: string;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const SessionNewDialog: React.FC<IProps> = (props: IProps) => {
  /////[ State ]//////////////////////////////////////////////////////////////////////////////////

  let [isActionRunning, setIsActionRunning] = React.useState(false);
  let [dialogStyle, setDialogStyle] = React.useState<DialogStyleType>(
    DialogStyleType.SessionEditor
  );

  /////[ Methods ]//////////////////////////////////////////////////////////////////////////////////

  function getBody(): JSX.Element {
    if (dialogStyle === DialogStyleType.SessionEditorBig) {
      Utils.setHash(DialogHash.CreateSessionExpanded);
    } else {
      Utils.setHash(DialogHash.CreateSession);
    }
    if (
      WidgetStore.organizations.currentOrg?.details.route === props.orgRoute
    ) {
      if (WidgetStore.organizations.currentOrg.details.me) {
        return (
          <SessionEditorWidget
            orgRoute={props.orgRoute}
            groupRoute={props.groupRoute}
            onSaved={onSave}
            onClose={onClose}
            onExpand={onExpand}
            startExpanded={props.isExpanded}
          />
        );
      } else {
        return <div>Current organization member is not set.</div>;
      }
    } else {
      return <div>Current organization is not set.</div>;
    }
  }

  function getActions(): DialogAction[] {
    return [];
  }

  /////[ Event Handlers ]///////////////////////////////////////////////////////////////////////////

  function onSave(): void {
    props.close();
  }

  function onClose(): void {
    Utils.setHash();
    props.close();
  }

  function onExpand(): void {
    setDialogStyle(DialogStyleType.SessionEditorBig);
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return DialogHelper.RenderDialog(
    dialogStyle,
    "Create Soundbite Session",
    getActions(),
    getBody(),
    isActionRunning,
    setIsActionRunning,
    props.close
  );
};
