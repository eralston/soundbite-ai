/***************************************************************************************************
 * DialogManagerUi is responsible for the actual rendering of the current dialog component defined
 * by the DialogManager.  DialogManager and DialogManagerUI communicate via the setCurrentDialogRef
 * method on the DialogManagerLink class.
 **************************************************************************************************/

import * as React from "react";

import { DialogManagerLink } from "./DialogManagerLink";

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const DialogManagerUi: React.FC = () => {
  //////////[ Define ]//////////////////////////////////////////////////////////////////////////////

  let [currentDialog, setCurrentDialog] = React.useState<React.ReactNode>(null);
  DialogManagerLink.setCurrentDialogRef = setCurrentDialog;

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  if (currentDialog) {
    return <React.Fragment>{currentDialog}</React.Fragment>;
  } else {
    return null;
  }
};
