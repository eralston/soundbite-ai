import * as React from "react";

import {
  ParticipantRole,
  SeriesPreview,
  Session,
  SessionPreview,
  User,
} from "@soundbite/api";

import { DialogManagerLink } from "./DialogManagerLink";
import { PlayDlg } from "../components/PlayDlg";
import { RecordDlg } from "../components/RecordDlg";
import { SeriesDeleteDialog } from "./SeriesDeleteDialog";
import { SessionDeleteDialog } from "./SessionDeleteDialog";
import { SessionNewDialog } from "./SessionNewDialog";
import { SessionReportDialog } from "./SessionReportDialog";
import { WidgetStore } from "../store/WidgetStore";
import { ProfileDialog } from "./ProfileDialog";
import { ConfirmDialog } from "./ConfirmDialog";
import { DialogStyleType } from "./DialogStyleType";

/**
 * Responsible for displaying dialogs seen throughout the application.  Defining all dialogs in
 * this class allows them to be displayed anywhere in the application without having to re-wire
 * the dialog into various UI components.
 */
export class DialogManager {
  //////////[ Show Dialog Methods ]/////////////////////////////////////////////////////////////////

  static ShowRecordDialog(orgRoute: string, sessionRoute: string) {
    this.showDialog(
      <RecordDlg
        orgRoute={orgRoute}
        sessionRoute={sessionRoute}
        participantRole={ParticipantRole.Host}
        isOpen={true}
        onClose={this.close}
      />
    );
  }

  static ShowReportsDialog(orgRoute: string, session: SessionPreview) {
    this.showDialog(
      <SessionReportDialog
        orgRoute={orgRoute}
        session={session}
        onClose={this.close}
      />
    );
  }

  static ShowPlayerDialog(orgRoute: string, sessionRoute: string) {
    this.showDialog(
      <PlayDlg
        orgRoute={orgRoute}
        currentUserRoute={
          WidgetStore.organizations.currentOrg?.details.me?.user.route
        }
        sessionRoute={sessionRoute}
        openImmediately={true}
        onClose={this.close}
      />
    );
  }

  /**
   * Displays the profile dialog for the given user, defaulting to the current user, optionally showing the settings for that user
   */
  static ShowProfileDialog(user?: User, showSettings: boolean = true): void {
    this.showDialog(
      <ProfileDialog
        user={user}
        showSettings={showSettings}
        onClose={this.close}
      />
    );
  }

  /**
   * Displays the series delete confirmation dialog.
   */
  static ShowSeriesDeleteDialog(orgRoute: string, series: SeriesPreview): void {
    this.showDialog(
      <SeriesDeleteDialog
        orgRoute={orgRoute}
        series={series}
        close={this.close}
      ></SeriesDeleteDialog>
    );
  }

  static ShowSeriesEditDialog(orgRoute: string, series: SeriesPreview): void {
    console.warn("Show Series Edit Dialog");
  }

  /**
   * Displays the session delete confirmation dialog.
   */
  static ShowSessionDeleteDialog(orgRoute: string, session: Session): void {
    this.showDialog(
      <SessionDeleteDialog
        orgRoute={orgRoute}
        session={session}
        close={this.close}
      ></SessionDeleteDialog>
    );
  }

  /**
   * Displays the new session dialog.
   * @param orgRoute - route of the organization with which the session is associated.
   * @param isSeries - flag indicating whether the dialog is for a session (false) or a series (true)
   */
  static ShowSessionNewDialog(
    orgRoute: string,
    groupRoute?: string,
    isSeries: boolean = false,
    onClose?: () => void
  ): void {
    const close = () => {
      if (onClose != null) {
        onClose();
      }
      this.close();
    };
    this.showDialog(
      <SessionNewDialog
        orgRoute={orgRoute}
        groupRoute={groupRoute}
        close={close}
        isExpanded={isSeries}
      ></SessionNewDialog>
    );
  }

  static ShowConfirmDialog(
    title: string,
    description: string,
    onConfirm: () => void,
    buttonTitle: string = "Confirm",
    dialogStyle: DialogStyleType = DialogStyleType.Normal,
    onClose?: () => void
  ): void {
    const close = () => {
      if (onClose != null) {
        onClose();
      }
      this.close();
    };
    this.showDialog(
      <ConfirmDialog
        buttonTitle={buttonTitle}
        style={dialogStyle}
        title={title}
        description={description}
        onConfirm={onConfirm}
        close={close}
      ></ConfirmDialog>
    );
  }

  //////////[ Utility Methods ]/////////////////////////////////////////////////////////////////////

  /**
   * Closes the current dialog by setting the current dialog reference to null.
   */
  private static close() {
    if (DialogManagerLink.setCurrentDialogRef) {
      DialogManagerLink.setCurrentDialogRef(null);
    }
  }

  /**
   * Displays the specified dialog component
   * @param component - dialog component to display
   */
  private static showDialog(component: React.ReactNode): void {
    if (DialogManagerLink.setCurrentDialogRef) {
      DialogManagerLink.setCurrentDialogRef(component);
    } else {
      throw "Attempted to show a dialog but the DialogManagerLink has not been established.  Ensure there is a <DialogManagerComponent> element in the application.";
    }
  }
}
