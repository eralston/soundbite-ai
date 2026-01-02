import React from "react";

import { Person, Utils } from "@soundbite/api";

import { SingletonDialogBase } from "./SingletonDialogBase";

/** Modal interaction to remove a person from their org */
export class DeleteUserDialog extends SingletonDialogBase<Person> {
  // Static

  static instance: DeleteUserDialog;

  public static open(user?: Person, onSubmit?: (user?: Person) => void): void {
    DeleteUserDialog.instance.open(user, onSubmit);
  }

  public static close(): void {
    DeleteUserDialog.instance.close();
  }

  // Instance

  constructor(props: object) {
    super(props);
    DeleteUserDialog.instance = this;
    this.headerClassname = "bg-gradient-danger";
  }

  protected modalHeader() {
    return <React.Fragment>Delete User</React.Fragment>;
  }

  protected modalBody() {
    let display = Utils.userDisplay(this.state.data?.user);
    return (
      <React.Fragment>
        Are you sure you want to permanently remove <strong>{display}</strong>{" "}
        from your organization?
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
          title="Close"
        >
          Close
        </button>{" "}
        <button
          type="button"
          className="btn btn-danger"
          onClick={this.submit.bind(this)}
          title="Delete User"
        >
          Delete
        </button>
      </React.Fragment>
    );
  }
}
