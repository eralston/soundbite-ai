import React from "react";
import { observer } from "mobx-react-lite";
import { SessionEditorWidget } from "@soundbite/widgets-react";
import { SessionDetails } from "../../../../api/dist";

/***************************************************************************************************
 *  Component
 **************************************************************************************************/
export const IFrameSessionEditor: React.FC = observer(() => {
  // Determine whether automatic play/record has been specified in the hash portion of the URL

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  function onClose() {
    var eventInfo = { sbevent: "sessionEditorOnClosed" };
    window.parent.postMessage(JSON.stringify(eventInfo), "*");
  }

  function onSaved(info: SessionDetails) {
    var eventInfo = { sbevent: "sessionEditorOnSaved", data: info };
    window.parent.postMessage(JSON.stringify(eventInfo), "*");
  }

  function onExpand() {
    var eventInfo = { sbevent: "sessionEditorOnExpand" };
    window.parent.postMessage(JSON.stringify(eventInfo), "*");
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <SessionEditorWidget
      onClose={onClose}
      onSaved={onSaved}
      onExpand={onExpand}
    />
  );
});
