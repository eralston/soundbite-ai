import React, { Component } from "react";
import { observer } from "mobx-react-lite";
import {
  Modal,
  ModalHeader,
  ModalBody,
  ModalFooter,
  FormGroup,
  InputGroup,
} from "reactstrap";

import {
  NewGroup,
  MemberRole,
  NewMember,
  PublicError,
  Utils,
} from "@soundbite/api";
import {
  Loader,
  ErrorCtrl,
  PeoplePicker,
  ShowWhen,
  WidgetStore,
  PeoplePickerMePolicy,
} from "@soundbite/widgets-react";

import NavService from "../../sdk/NavService";

interface IProps {
  isOpen: boolean;
  onClose: () => void;
  orgRoute: string;
}

interface IState {
  disabled: boolean;
  newGroup: NewGroup;
  error?: PublicError;
}

class NewGroupModal extends Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);

    this.state = {
      disabled: false,
      newGroup: this.freshNewGroup(),
      error: undefined,
    };
  }

  get disabled(): boolean {
    return this.state.disabled;
  }
  set disabled(is: boolean) {
    this.setState({ disabled: is });
  }

  get newGroup(): NewGroup {
    return this.state.newGroup;
  }

  set newGroup(group: NewGroup) {
    this.setState({ newGroup: group });
  }

  protected freshNewGroup(): NewGroup {
    const newMembers: NewMember[] = [];
    if (WidgetStore.organizations.currentOrg?.details.me) {
      const myNewMember: NewMember = {
        personToken: WidgetStore.organizations.currentOrg.details.me?.route,
        memberRole: MemberRole.Owner,
      };
      newMembers.push(myNewMember);
    }

    return {
      name: "",
      description: "",
      members: newMembers,
    } as NewGroup;
  }

  protected reset() {
    const newGroup = this.freshNewGroup();
    this.setState({ newGroup, disabled: false, error: undefined });
  }

  protected close() {
    this.props.onClose();
    this.reset();
  }

  protected async submit() {
    try {
      if (!WidgetStore.organizations.currentOrg)
        throw new PublicError(
          undefined,
          "Cannot Create Group without an active org"
        );

      this.disabled = true;
      const newGroup = await WidgetStore.groups.createGroupAsync(
        WidgetStore.organizations.currentOrg?.details.route,
        this.state.newGroup
      );
      NavService.goto(
        `/organizations/${WidgetStore.organizations.currentOrg.details.route}/groups/${newGroup.route}/feed`
      );
      this.close();
    } catch (error: any) {
      let err: PublicError = error;
      if (!(err instanceof PublicError)) err = new PublicError(error);
      this.setState({ error: err, disabled: false });
    }
  }

  protected get invalid() {
    const group = this.state.newGroup;
    if (!group.name || group.name.length === 0) return true;

    if (!group.members || group.members.length === 0) return true;

    // We need an owner that is already in the system
    const knownOwnerCount = group.members.filter(
      (m) => !Utils.isEmail(m.personToken) && m.memberRole >= MemberRole.Owner
    ).length;

    if (knownOwnerCount === 0) return true;

    return false;
  }

  protected onNameChange(event: React.ChangeEvent<HTMLInputElement>) {
    const group = this.newGroup;
    group.name = event.target.value;
    this.newGroup = group;
  }

  protected filterOutMembers(group: NewGroup, role: MemberRole): NewMember[] {
    return group.members.filter((m) => m.memberRole !== role);
  }

  protected mapToMembers(
    peopleTokens: string[],
    role: MemberRole
  ): NewMember[] {
    return peopleTokens.map((p) => {
      return { personToken: p, memberRole: role };
    });
  }

  protected onMembersChanged(peopleTokens: string[]) {
    const group = this.newGroup;
    const oldMembers = this.filterOutMembers(group, MemberRole.Member);
    const newMembers = this.mapToMembers(peopleTokens, MemberRole.Member);
    group.members = oldMembers.concat(newMembers);
    this.newGroup = group;
  }

  protected onOwnersChanged(peopleTokens: string[]) {
    const group = this.newGroup;
    const oldMembers = this.filterOutMembers(group, MemberRole.Owner);
    const newMembers = this.mapToMembers(peopleTokens, MemberRole.Owner);
    group.members = oldMembers.concat(newMembers);
    this.newGroup = group;
  }

  protected onDescriptionChange(event: React.ChangeEvent<HTMLTextAreaElement>) {
    const group = this.newGroup;
    group.description = event.target.value;
    this.newGroup = group;
  }

  render() {
    return (
      <Modal
        isOpen={this.props.isOpen}
        toggle={this.close.bind(this)}
        backdrop="static"
      >
        <ModalHeader
          toggle={this.close.bind(this)}
          className="bg-gradient-primary"
        >
          Create Group
        </ModalHeader>
        <ModalBody>
          <FormGroup>
            <InputGroup>
              <input
                aria-label="Group Name"
                className="form-control"
                defaultValue={this.newGroup.name}
                disabled={this.disabled}
                onChange={this.onNameChange.bind(this)}
                placeholder="Group Name"
                type="text"
                title="Group Name"
              />
            </InputGroup>
          </FormGroup>
          <FormGroup>
            <textarea
              className="form-control"
              defaultValue={this.newGroup.description}
              disabled={this.disabled}
              onChange={this.onDescriptionChange.bind(this)}
              placeholder="Group Description"
              title="Group Description"
            ></textarea>
          </FormGroup>
          <FormGroup>
            <label className="form-control-label">Members</label>
            <PeoplePicker
              allowInvite={true}
              disabled={this.disabled}
              mePolicy={PeoplePickerMePolicy.Include}
              onChange={this.onMembersChanged.bind(this)}
              orgRoute={this.props.orgRoute}
              title="Group Members"
            />
          </FormGroup>
          <FormGroup>
            <label className="form-control-label">Owners</label>
            <PeoplePicker
              allowInvite={true}
              disabled={this.disabled}
              mePolicy={PeoplePickerMePolicy.Include}
              onChange={this.onOwnersChanged.bind(this)}
              orgRoute={this.props.orgRoute}
              title="Group Owners"
            />
          </FormGroup>
        </ModalBody>
        <ModalFooter>
          <ErrorCtrl error={this.state.error} />
          <Loader isLoadedWhen={!this.disabled}>
            <button
              className="btn btn-outline-secondary"
              onClick={this.close.bind(this)}
              type="button"
              title="Close"
            >
              Close
            </button>{" "}
            <button
              className="btn btn-primary"
              disabled={this.invalid || this.disabled}
              onClick={this.submit.bind(this)}
              type="button"
              title="Create Group"
            >
              Create
            </button>
          </Loader>
        </ModalFooter>
      </Modal>
    );
  }
}

interface NewGroupDialogProps {
  isOpen: boolean;
  onClose: () => void;
  orgRoute: string;
}

export const NewGroupDialog: React.FC<NewGroupDialogProps> = observer(
  (props: NewGroupDialogProps) => {
    return (
      <ShowWhen is={!!WidgetStore.organizations.currentOrg}>
        <NewGroupModal
          isOpen={props.isOpen}
          onClose={props.onClose}
          orgRoute={props.orgRoute}
        />
      </ShowWhen>
    );
  }
);
