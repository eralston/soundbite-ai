import React, { Fragment, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faMicrophone } from "@fortawesome/free-solid-svg-icons";

import { ParticipantRole } from "@soundbite/api";

import { RecordDlg } from "../components/RecordDlg";
import { ErrorDlg } from "../components/ErrorDlg";
import { SbButton, SbButtonSize, SbButtonType } from "./SbButton";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute: string;
  participantRole: ParticipantRole;
  onSave?: () => void;
}

/**
 * Opens the record dialog for the given org and session
 * @param props
 */
export const RecordBtn: React.FC<IProps> = (props: IProps) => {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  let [isOpen, setIsOpen] = useState(false);

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onError(err: any): void {
    ErrorDlg.show(
      err,
      "Error Recording Session",
      "Likley could not make contact with server"
    );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <React.Fragment>
      <SbButton
        type={SbButtonType.Primary}
        size={SbButtonSize.Small}
        title="Record Session"
        text="Record"
        btnClassName="sb-record-btn"
        icon={faMicrophone}
        onClick={() => setIsOpen(true)}
      />
      <RecordDlg
        orgRoute={props.orgRoute}
        sessionRoute={props.sessionRoute}
        participantRole={props.participantRole}
        onError={onError}
        isOpen={isOpen}
        onClose={() => {
          setIsOpen(false);
        }}
        onSave={props.onSave}
      />
    </React.Fragment>
  );
};
