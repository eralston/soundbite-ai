/** @jsx jsx */
import { jsx } from "@emotion/react";
import React, { Component } from "react";
import Select from "react-select";
import makeAnimated from "react-select/animated";

import { Person, Utils } from "@soundbite/api";

import { Loader } from "../controls";

export interface IParticipantOption {
  value: string;
  label: string;
  isFixed?: boolean;
}

interface IProps {
  people?: Person[];
  defaultParticipants?: IParticipantOption[];
  onPeopleChange?: (people: Person[]) => void;
  onParticipantChange?: (participants: IParticipantOption[]) => void;
  disabled?: boolean;
}

interface IState {
  allOptions: IParticipantOption[];
  selectedOptions: IParticipantOption[];
}

/** Picks individuals from the given org */
export class ParticipantPicker extends Component<IProps, IState> {
  public static peopleToOptions(people?: Person[]): IParticipantOption[] {
    if (!people) return [];

    return people.map((person: Person): IParticipantOption => {
      return {
        label: Utils.userDisplay(person.user),
        value: person.route,
      };
    });
  }

  animatedComponents = makeAnimated();

  constructor(props: IProps) {
    super(props);

    let peopleAsOptions = ParticipantPicker.peopleToOptions(props.people);
    let defaultParticipants =
      props.defaultParticipants ||
      ParticipantPicker.peopleToOptions(props.people);
    this.state = {
      selectedOptions: defaultParticipants,
      allOptions: peopleAsOptions,
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

    this.setState({ selectedOptions: selectedOptions });

    const participants: IParticipantOption[] =
      selectedOptions || ([] as IParticipantOption[]);

    if (this.props.onParticipantChange)
      this.props.onParticipantChange(participants);

    if (!this.props.onPeopleChange) return;

    let people: Person[] = [];
    if (this.props.people) {
      for (let person of this.props.people) {
        // TODO: Better perf
        let isPersonSelected =
          participants.filter((p: IParticipantOption) => {
            return p.value === person.route;
          }).length > 0;
        if (isPersonSelected) people.push(person);
      }
    }

    this.props.onPeopleChange(people);
  };

  render() {
    return (
      <Loader isLoadedWhen={this.props.people != null}>
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
          placeholder="Select Person..."
        />
      </Loader>
    );
  }
}
