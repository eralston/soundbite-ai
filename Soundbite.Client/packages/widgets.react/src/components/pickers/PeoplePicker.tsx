/** @jsx jsx */
import { jsx } from "@emotion/react";
import React, { Component } from "react";
import Select from "react-select";
import CreatableSelect from "react-select/creatable";
import makeAnimated from "react-select/animated";

import { PageUtils, Person, Utils } from "@soundbite/api";

import {
  withAsyncPaginate,
  IOption,
  ILoadOptionsRequest,
  ILoadOptionsResponse,
  ILoadOptionGroup,
} from "../select";

import { shouldLoadMorePickerPages, ShowWhen, WidgetStore } from "../..";

const CreatablePaginatingSelect = withAsyncPaginate(CreatableSelect);
const PaginatingSelect = withAsyncPaginate(Select);

/** The behavior of the picker in regard to the current user */
export enum PeoplePickerMePolicy {
  /** Allow the current user on the list, but not pre-selected */
  Allow,
  /** Current user is included */
  Include,
  /** Current user is locked into the list to be included */
  Require,
  /** Does not show the current user whatsoever */
  Deny,
}

interface IProps {
  /** Sets a hardcoded group of people for the picker; otherwise this is based on presence of orgRoute/groupRoute */
  options?: Person[];
  /** Loads people from this org */
  orgRoute?: string;
  /** Loads people from this team in the given org */
  groupRoute?: string;
  /** The picker's behavior when it comes to the current user */
  mePolicy?: PeoplePickerMePolicy;
  /** Calls back with the set of person routes and/or newly created e-mail addresses */
  onChange: (tokens: string[]) => void;
  /** Read-only and faded if true; otherwise, interactive */
  disabled?: boolean;
  /** Flag indicating if the user create create new entires via e-mail */
  allowInvite?: boolean;
  /** Annotated title of the input */
  title?: string;
  /** placeholder text */
  placeholder?: string;
}

interface IState {
  value: any;
}

/**
 * A list of people in the organization, potentially subsetted to a team, that also optionally allows for new options via email
 * */
export class PeoplePicker extends Component<IProps, IState> {
  static personToOption(person: Person): IOption {
    let name = Utils.userDisplay(person.user, false, true);
    return { label: name, value: person.route };
  }

  static peopleToOptions(people?: Person[]) {
    if (people == null) {
      return [];
    }
    const ret = people.map((p) => PeoplePicker.personToOption(p));
    return ret;
  }

  animatedComponents: any;

  constructor(props: IProps) {
    super(props);

    this.doPropsOnChange = this.doPropsOnChange.bind(this);
    this.onChange = this.onChange.bind(this);
    this.onCreateOption = this.onCreateOption.bind(this);
    this.onFormatCreateLabel = this.onFormatCreateLabel.bind(this);
    this.loadOptions = this.loadOptions.bind(this);

    this.animatedComponents = makeAnimated();

    const value: IOption[] = [];

    const mePolicy = this.props.mePolicy ?? PeoplePickerMePolicy.Allow;
    if (mePolicy === PeoplePickerMePolicy.Include) {
      const me = WidgetStore.organizations.currentOrg?.details.me;
      if (me == null) {
        console.error("Trying to load me into a picker before initialization");
      } else {
        value.push(PeoplePicker.personToOption(me));
      }
    } else if (mePolicy !== PeoplePickerMePolicy.Allow) {
      console.warn(
        "People picker requested missing me policy:",
        PeoplePickerMePolicy[mePolicy]
      );
    }

    this.state = {
      value,
    };
  }

  doPropsOnChange(newValue: IOption[]) {
    const tokens: string[] = [];

    const myRoute = WidgetStore.organizations.currentOrg?.details.me?.route;
    const checkForMeAndFilter: boolean =
      this.props.mePolicy === PeoplePickerMePolicy.Deny &&
      WidgetStore.organizations.currentOrg?.details.me?.route != null;

    if (newValue) {
      const listOfOptions = newValue as IOption[];
      for (const option of listOfOptions) {
        if (checkForMeAndFilter && option.value === myRoute) {
          continue;
        }
        tokens.push(option.value);
      }
    }

    this.props.onChange(tokens);
  }

  onChange(newValue: any) {
    this.setState({ value: newValue });
    this.doPropsOnChange(newValue);
  }

  onCreateOption(inputValue: string) {
    const email = Utils.validateEmail(inputValue);
    if (!email) return;
    const newOption: IOption = {
      label: email,
      value: email,
    };
    let newValue: IOption[] = [...(this.state.value || []), newOption];
    this.setState({
      value: newValue,
    });
    this.doPropsOnChange(newValue);
  }

  onFormatCreateLabel(inputValue: string): string {
    return `Send Invite to ${inputValue}...`;
  }

  get isEmptyMode(): boolean {
    return this.props.orgRoute == null;
  }

  async loadOptions(
    filter: string,
    prevOptions?: ILoadOptionsRequest
  ): Promise<ILoadOptionsResponse> {
    // If we don't have an org associated, then we don't load groups at all - we're just empty
    if (this.isEmptyMode || this.props.orgRoute == null) {
      return {
        options: [] as ILoadOptionGroup[],
        hasMore: false,
      };
    }
    const page = await WidgetStore.people.readAllAsync(this.props.orgRoute, {
      filter,
      skip: prevOptions?.length ?? 0,
    });

    const hasMore = PageUtils.hasMorePages(page);
    const options = PeoplePicker.peopleToOptions(page.result);
    return {
      options,
      hasMore,
    };
  }

  pendingEmails(): IOption[] {
    const { value } = this.state;
    let emailOptions: IOption[] = [];
    if (value)
      emailOptions = value.filter(
        (o: IOption) => Utils.validateEmail(o.value) !== undefined
      );
    return emailOptions;
  }

  staticPicker() {
    const options = PeoplePicker.peopleToOptions(this.props.options);

    return (
      <CreatableSelect
        className="sb-picker"
        classNamePrefix="sb-picker"
        closeMenuOnSelect={this.isEmptyMode}
        components={this.animatedComponents}
        formatCreateLabel={this.onFormatCreateLabel}
        isDisabled={this.props.disabled}
        isMulti
        onChange={this.onChange}
        options={options}
        placeholder={
          this.props.placeholder ?? "Select Person or Enter E-Mail..."
        }
        value={this.state.value}
      />
    );
  }

  pagingPicker() {
    if (this.props.allowInvite) {
      return (
        <CreatablePaginatingSelect
          className="sb-picker"
          classNamePrefix="sb-picker"
          closeMenuOnSelect={this.isEmptyMode}
          debounceTimeout={300}
          formatCreateLabel={this.onFormatCreateLabel}
          isMulti
          loadOptions={this.loadOptions}
          onChange={this.onChange}
          placeholder={
            this.props.placeholder ?? "Select Person or Enter E-Mail..."
          }
          shouldLoadMore={shouldLoadMorePickerPages}
          value={this.state.value}
        />
      );
    } else {
      return (
        <PaginatingSelect
          className="sb-picker"
          classNamePrefix="sb-picker"
          closeMenuOnSelect={this.isEmptyMode}
          debounceTimeout={300}
          formatCreateLabel={this.onFormatCreateLabel}
          isMulti
          loadOptions={this.loadOptions}
          onChange={this.onChange}
          placeholder={
            this.props.placeholder ?? "Select Person or Enter E-Mail..."
          }
          shouldLoadMore={shouldLoadMorePickerPages}
          value={this.state.value}
        />
      );
    }
  }

  picker() {
    if (this.props.options != null) {
      return this.staticPicker();
    } else {
      return this.pagingPicker();
    }
  }

  render() {
    // Pull e-mails
    const pendingEmails = this.pendingEmails();

    return (
      <div title={this.props.title}>
        {this.picker()}
        <ShowWhen is={pendingEmails.length > 0}>
          <ul className="sb-peoplepicker-invites">
            {pendingEmails.map((opt: IOption) => {
              return (
                <li key={opt.value} className="text-muted">
                  Invite will be sent to '{opt.value}'
                </li>
              );
            })}
          </ul>
        </ShowWhen>
      </div>
    );
  }
}
