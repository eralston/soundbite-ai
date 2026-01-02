import React, { Component } from "react";

import {
  DeleteUserDialog,
  ErrorDlg,
  InviteUserDialog,
} from "@soundbite/widgets-react";

import { AddMemberDialog } from "./AddMemberDialog";
import { DeleteMemberDlg } from "./DeleteMemberDlg";
import { DeleteOrgDialog } from "./DeleteOrgDlg";
import { DeleteGroupDialog } from "./DeleteGroupDialog";

export class SingletonDialogs extends Component {
  render() {
    return (
      <div>
        <AddMemberDialog />
        <DeleteMemberDlg />
        <DeleteOrgDialog />
        <DeleteGroupDialog />
        <DeleteUserDialog />
        <ErrorDlg />
        <InviteUserDialog />
      </div>
    );
  }
}
