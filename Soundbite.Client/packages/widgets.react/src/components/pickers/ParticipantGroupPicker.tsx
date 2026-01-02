/** @jsx jsx */
import { jsx } from "@emotion/react";
import React, { Component } from "react";
import Select from "react-select";
import makeAnimated from "react-select/animated";

import { Group } from "@soundbite/api";

interface IParticipantGroupOption {
  value: string;
  label: string;
}

interface IProps {
  groups?: Group[];
  defaultGroups?: IParticipantGroupOption[];
  onChangeParticipantGroup?: (group: IParticipantGroupOption[]) => void;
  onChangeGroup?: (group: Group[]) => void;
  disabled?: boolean;
  title?: string;
  className?: string;
}

interface IState {
  allOptions: IParticipantGroupOption[];
  selectedOptions: IParticipantGroupOption[];
}

/** Picks groups from the given org */
export class ParticipantGroupPicker extends Component<IProps, IState> {
  animatedComponents = makeAnimated();

  static convertToOptions(groups?: Group[]): IParticipantGroupOption[] {
    if (!groups) return [];

    return groups.map((grp: Group): IParticipantGroupOption => {
      return {
        label: grp.name,
        value: grp.route.toString(),
      };
    });
  }

  constructor(props: IProps) {
    super(props);

    let allOptions = ParticipantGroupPicker.convertToOptions(props.groups);
    let selectedOptions = props.defaultGroups || allOptions;
    this.state = {
      allOptions,
      selectedOptions,
    };
  }

  onChange = (selectedOptions: any, actionMeta: any) => {
    switch (actionMeta.action) {
      case "remove-value":
      case "pop-value":
        if (actionMeta.removedValue?.isFixed) return;
        break;
      case "clear":
        return;
    }

    const selectedGroups: IParticipantGroupOption[] = selectedOptions || [];

    this.setState({ selectedOptions: selectedGroups });

    if (this.props.onChangeParticipantGroup)
      this.props.onChangeParticipantGroup(selectedGroups);

    if (!this.props.onChangeGroup) return;

    let people: Group[] = [];
    if (this.props.groups) {
      for (let grp of this.props.groups) {
        let isGroupSelected =
          selectedGroups.filter((p: IParticipantGroupOption) => {
            return p.value === grp.route;
          }).length > 0;
        if (isGroupSelected) people.push(grp);
      }
    }

    this.props.onChangeGroup(people);
  };

  render() {
    return (
      <div title={this.props.title} className={this.props.className}>
        <Select
          className="sb-picker"
          classNamePrefix="sb-picker"
          options={this.state.allOptions}
          value={this.state.selectedOptions}
          components={this.animatedComponents}
          isMulti
          closeMenuOnSelect={false}
          onChange={this.onChange.bind(this)}
          isClearable={false}
          isDisabled={this.props.disabled}
          placeholder="Select Group..."
        />
      </div>
    );
  }
}
