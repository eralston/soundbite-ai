import React, { useEffect, useState } from "react";
import { Modal, ModalBody, ModalHeader } from "reactstrap";

import { ParticipantRole, SessionDetails, Utils } from "@soundbite/api";

import { WidgetStore } from "../store";
import { DialogHash } from "../modules/HashHelper";
import { RecordWidget } from "../widgets/RecordWidget";
import { Loader } from "./controls";

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  orgRoute?: string;
  sessionRoute?: string;
  participantRole: ParticipantRole;
  isOpen: boolean;
  onError?: (err: any) => void;
  onClose?: () => void;
  onSave?: () => void;
}

/**
 * A dialog wrapping the record widget
 * @param props
 */
export const RecordDlg: React.FC<IProps> = (props: IProps) => {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  let [session, setSession] = useState<SessionDetails>();

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onError(err: any): void {
    if (props.onError) {
      props.onError(err);
    }
  }

  function onClose(): void {
    Utils.setHash();
    if (props.onClose) {
      props.onClose();
    }
  }

  //////////[ Initialization ]//////////////////////////////////////////////////////////////////////

  useEffect(() => {
    async function init() {
      if (props.isOpen && props.orgRoute && props.sessionRoute) {
        try {
          Utils.setHash(`${DialogHash.Record}=${props.sessionRoute}`);
          const sessionDetails =
            await WidgetStore.sessions.readSessionDetailsAsync(
              props.orgRoute,
              props.sessionRoute
            );
          setSession(sessionDetails);
        } catch (err: any) {
          onError(err);
        }
      }
    }
    init();
  }, [props.isOpen, props.orgRoute, props.sessionRoute]);

  //////////[ Methods - UI Helpers ]////////////////////////////////////////////////////////////////

  function getSubtitle() {
    return (
      <span className="sb-modal-header-subtitle">
        {Utils.formatRelativeDate(session?.publish)}
      </span>
    );
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <React.Fragment>
      <Modal isOpen={props.isOpen} toggle={onClose} backdrop="static">
        <ModalHeader toggle={onClose} className="bg-gradient-primary">
          {session?.name || "Loading Soundbite Session..."}
          {getSubtitle()}
        </ModalHeader>
        <Loader isLoadedWhen={!!session} style={{ minHeight: "12rem" }}>
          <ModalBody>
            {props.orgRoute && session && (
              <RecordWidget
                orgRoute={props.orgRoute}
                participantRole={props.participantRole}
                session={session}
                onClose={onClose}
                onError={onError}
                onSave={props.onSave}
                showButtons={true}
              />
            )}
          </ModalBody>
        </Loader>
      </Modal>
    </React.Fragment>
  );
};
