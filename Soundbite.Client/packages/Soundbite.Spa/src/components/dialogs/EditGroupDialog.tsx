import React, { Component } from "react";
import {
  FormGroup,
  InputGroup,
  Modal,
  ModalHeader,
  ModalFooter,
  ModalBody,
} from "reactstrap";
import { Group, OrganizationDetails, Utils, PublicError } from "@soundbite/api";

import { PatchService } from "../../modules/PatchService";
import { WidgetStore } from "@soundbite/widgets-react";

/**
 * Defines the properties passed into the component
 */
interface IComponentProps {
  disabled: boolean;
  isOpen: boolean;
  onClose: () => void;
  org?: OrganizationDetails;
  group: Group;
}

/**
 * Defines the state on the component
 */
interface IComponentState {
  item: Group; // Reference to the item being edited
  disabled: boolean; // Flag indicating whether form input is disabled
  error?: PublicError; // Reference to last error that occured
}

/**
 * Renders an editor capable of updating group information
 */
export class EditGroupDialog extends Component<
  IComponentProps,
  IComponentState
> {
  /////[ Constructor ]//////////////////////////////////////////////////////////////////////////////

  constructor(props: IComponentProps) {
    super(props);
    this.state = {
      item: props.group,
      disabled: props.disabled,
    } as IComponentState;
  }

  /////[ Methods ] /////////////////////////////////////////////////////////////////////////////////

  /**
   * Responsible for updating component state based on external changes to component properties
   * @param props - current property state
   * @param currentState - current component state
   * @returns - new component state
   */
  static getDerivedStateFromProps(
    props: IComponentProps,
    currentState: IComponentState
  ) {
    if (props.group?.route !== currentState?.item?.route) {
      return {
        item: Utils.clone(props.group) || ({} as Group),
        disabled: false,
      } as IComponentState;
    } else {
      return currentState;
    }
  }

  render() {
    return (
      <Modal
        isOpen={this.props.isOpen}
        toggle={this.props.onClose.bind(this)}
        backdrop="static"
      >
        <ModalHeader className="bg-gradient-primary">
          Edit {this.props.group?.name}
          <span className="sb-modal-header-subtitle">
            <span className="">{this.props.group?.description}</span>
          </span>
        </ModalHeader>
        <ModalBody>
          <FormGroup>
            <InputGroup>
              <input
                type="text"
                className="form-control"
                placeholder="Team Name"
                aria-label="Team Name"
                value={this.state.item?.name}
                onChange={this.onNameChange.bind(this)}
                disabled={this.state.disabled}
              />
            </InputGroup>
          </FormGroup>
          <FormGroup>
            <textarea
              className="form-control"
              placeholder="Team Description"
              value={this.state.item?.description}
              onChange={this.onDescriptionChange.bind(this)}
              disabled={this.state.disabled}
            ></textarea>
          </FormGroup>
        </ModalBody>
        <ModalFooter>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={this.props.onClose.bind(this)}
            disabled={this.state.disabled}
          >
            Close
          </button>{" "}
          <button
            type="button"
            className="btn btn-primary"
            onClick={this.onSubmit.bind(this)}
            disabled={this.state.disabled}
          >
            Save
          </button>
        </ModalFooter>
      </Modal>
    );
  }

  protected onNameChange(event: React.ChangeEvent<HTMLInputElement>) {
    const item = this.state.item;
    item.name = event.target.value;
    this.setState({ item });
  }

  protected onDescriptionChange(event: React.ChangeEvent<HTMLTextAreaElement>) {
    const item = this.state.item;
    item.description = event.target.value;
    this.setState({ item });
  }

  protected async onSubmit(): Promise<void> {
    //var teamUpdates = Utils.diff(this.props.team, this.state.item) as ITeam;
    var patchService = new PatchService();
    var patch = patchService.generatePatch(this.props.group, this.state.item);

    //var teamsApi = new TeamsApiService();
    //var result = await teamsApi.updateAsync(this.props.org?.route || '', this.props.team.route, patch);

    this.setState({ disabled: true });

    await WidgetStore.groups
      .updateGroupAsync(
        this.props.org?.route || "",
        this.props.group.route,
        patch
      )
      .then(() => {
        this.setState({ disabled: false });
        this.props.onClose();
      })
      .catch(() => {
        this.setState({ disabled: false });
      });
  }
}
