import React from "react";

import { Group, Member, Utils } from "@soundbite/api";

import { SingletonDialogBase } from "@soundbite/widgets-react";

export interface IMemberAndGroup {
  member: Member;
  group: Group;
}

export class DeleteMemberDlg extends SingletonDialogBase<IMemberAndGroup> {
  // Static

  static instance: DeleteMemberDlg;

  public static open(
    memberAndGroup?: IMemberAndGroup,
    onSubmit?: (memberAndGroup?: IMemberAndGroup) => void
  ): void {
    DeleteMemberDlg.instance.open(memberAndGroup, onSubmit);
  }

  public static close(): void {
    DeleteMemberDlg.instance.close();
  }

  // Instance

  constructor(props: object) {
    super(props);
    DeleteMemberDlg.instance = this;
    this.headerClassname = "bg-gradient-danger";
  }

  protected modalHeader() {
    return <React.Fragment>Delete Member</React.Fragment>;
  }

  protected modalBody() {
    let displayName = Utils.userDisplay(this.state.data?.member.person.user);

    return (
      <React.Fragment>
        Are you sure you want to remove <strong>{displayName}</strong> from{" "}
        <strong>{this.state.data?.group.name}</strong>?
      </React.Fragment>
    );
  }

  protected modalFooter() {
    return (
      <React.Fragment>
        <button
          type="button"
          className="btn btn-secondary"
          onClick={this.close.bind(this)}
        >
          Close
        </button>{" "}
        <button
          type="button"
          className="btn btn-danger"
          onClick={this.submit.bind(this)}
        >
          Delete
        </button>
      </React.Fragment>
    );
  }
}
