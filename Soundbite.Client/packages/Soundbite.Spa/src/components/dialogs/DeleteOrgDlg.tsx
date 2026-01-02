import React from "react";
import { OrganizationExtended } from "@soundbite/api";
import { SingletonDialogBase } from "@soundbite/widgets-react";

export class DeleteOrgDialog extends SingletonDialogBase<OrganizationExtended> {
  // Static

  static instance: DeleteOrgDialog;

  public static open(
    org?: OrganizationExtended,
    onSubmit?: (org?: OrganizationExtended) => void,
    onClose?: (org?: OrganizationExtended) => void
  ): void {
    DeleteOrgDialog.instance.open(org, onSubmit, onClose);
  }

  public static close(): void {
    DeleteOrgDialog.instance.close();
  }

  // Instance

  constructor(props: object) {
    super(props);
    DeleteOrgDialog.instance = this;
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
