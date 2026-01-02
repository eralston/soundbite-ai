import React from "react";
import { Group } from "@soundbite/api";

import { SingletonDialogBase } from "@soundbite/widgets-react";

export class DeleteGroupDialog extends SingletonDialogBase<Group> {
  // Static

  static instance: DeleteGroupDialog;

  public static open(
    group?: Group,
    onSubmit?: (group?: Group) => void,
    onClose?: (group?: Group) => void
  ): void {
    DeleteGroupDialog.instance.open(group, onSubmit, onClose);
  }

  public static close(): void {
    DeleteGroupDialog.instance.close();
  }

  // Instance

  constructor(props: object) {
    super(props);
    DeleteGroupDialog.instance = this;
    this.headerClassname = "bg-gradient-danger";
  }

  subtitle() {
    return `${this.state.data && this.state.data.description}`;
  }

  protected modalHeader() {
    return (
      <React.Fragment>
        Delete {this.state.data && this.state.data.name}
        <span className="sb-modal-header-subtitle">
          <span className="">{this.subtitle()}</span>
        </span>
      </React.Fragment>
    );
  }

  protected modalBody() {
    return (
      <React.Fragment>
        Are you sure you want to permanently delete{" "}
        <strong>{this.state.data && this.state.data.name}</strong> from your
        organization?
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
          title="Delete Team"
        >
          Delete
        </button>
      </React.Fragment>
    );
  }
}
