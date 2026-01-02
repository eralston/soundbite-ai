import React from "react";

import { PeoplePicker, WidgetStore } from "@soundbite/widgets-react";
import { SingletonDialogBase } from "@soundbite/widgets-react";

export interface IAddMember {
  newPeopleTokens: string[];
  orgRoute?: string;
}

export class AddMemberDialog extends SingletonDialogBase<IAddMember> {
  // Static

  static instance: AddMemberDialog;

  // TODO: Restructure to make empty opens less dumb
  public static open(
    addMemberProps?: IAddMember,
    onSubmit?: (addMember?: IAddMember) => void
  ): void {
    if (!addMemberProps)
      addMemberProps = {
        newPeopleTokens: [],
        orgRoute: WidgetStore.organizations.currentOrg?.details.route,
      };
    AddMemberDialog.instance.open(addMemberProps, onSubmit);
  }

  public static close(): void {
    AddMemberDialog.instance.close();
  }

  // Instance

  constructor(props: IAddMember) {
    super(props);
    AddMemberDialog.instance = this;
    this.onPeoplePickerChange = this.onPeoplePickerChange.bind(this);
  }

  protected modalHeader() {
    return <React.Fragment>Add Members</React.Fragment>;
  }

  protected onPeoplePickerChange(peopleIds: string[]): void {
    let data = this.state.data;
    if (data) data.newPeopleTokens = peopleIds;
    this.setState({ data: data });
  }

  protected modalBody() {
    if (this.state.data?.orgRoute == null) {
      return <span className="text-muted">Loading Organization...</span>;
    }

    return (
      <React.Fragment>
        <PeoplePicker
          allowInvite={true}
          onChange={this.onPeoplePickerChange}
          orgRoute={this.state.data?.orgRoute}
        />
      </React.Fragment>
    );
  }
}
