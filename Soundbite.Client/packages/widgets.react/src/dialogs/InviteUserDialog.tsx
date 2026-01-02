import React from "react";

import { Invite } from "@soundbite/api";

import { SingletonDialogBase } from "./SingletonDialogBase";
import { PeoplePicker } from "../components/pickers/PeoplePicker";

export class InviteUserDialog extends SingletonDialogBase<Invite[]> {
  // Static

  static instance: InviteUserDialog;

  public static open(
    invite?: Invite[],
    onSubmit?: (newInvite?: Invite[]) => void
  ): void {
    if (!invite) invite = [];
    InviteUserDialog.instance.open(invite, onSubmit);
  }

  public static close(): void {
    InviteUserDialog.instance.close();
  }

  // Instance

  constructor(props: object) {
    super(props);
    InviteUserDialog.instance = this;
  }

  protected onPeoplePickerChange(emails: string[]): void {
    const data = emails.map((e) => {
      return { token: e };
    });
    this.setState({ data: data });
  }

  protected modalHeader() {
    return <React.Fragment>Invite Users</React.Fragment>;
  }

  protected modalBody() {
    return (
      <React.Fragment>
        <PeoplePicker
          allowInvite={true}
          onChange={this.onPeoplePickerChange.bind(this)}
          title="Enter one or more E-mail addresses"
          placeholder="Enter one or more E-mail addresses"
        />
      </React.Fragment>
    );
  }

  protected modalFooter() {
    return (
      <React.Fragment>
        <button
          type="button"
          className="btn btn-outline-secondary"
          onClick={this.close.bind(this)}
          title="Close"
        >
          Close
        </button>{" "}
        <button
          type="button"
          className="btn btn-primary"
          onClick={this.submit.bind(this)}
          title="Invite User"
        >
          Invite
        </button>
      </React.Fragment>
    );
  }
}
