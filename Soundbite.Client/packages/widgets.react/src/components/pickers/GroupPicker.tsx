import React, { useState } from "react";
import { Group } from "@soundbite/api";
import { WidgetStore } from "../../store";
import { ItemPicker, IListItemInfo } from "./ItemPicker";
import { useEffect } from "react";

/***************************************************************************************************
 *  Enums
 **************************************************************************************************/

/**
 * Enumeration of the varoius automatic group loading options for the group picker.
 */
export enum GroupPickerLoadType {
  /** Do not automatically load groups */
  None,
  /** Only groups to which the user is a member should  be loaded */
  MyGroups,
  /** All groups should be loaded */
  AllGroups,
}

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  /** Flag indicating whether the picker is enabled or disabled */
  disabled?: boolean;

  groups?: Group[];

  /** Flag indicating whether the picker chooses a single item or multiple items */
  isMulti?: boolean;

  loadMode?: GroupPickerLoadType;

  /**
   *  Notifies a listener of changes that have occured in the list.  This includes which items
   *  have been added, removed, remained the same, and the order in which the items appear in
   *  the list. The order of the items is NOT guaranteed.  Items that have been removed have a
   *  positional index of -1.
   */
  onChange?: (changes: IListItemInfo[]) => void;

  /**
   * Notifies a listener when the component is in single-selection mode and an item is selected.
   */
  onItemSelected?: (item: any) => void;

  /** Route to the org from which we will pick groups */
  orgRoute: string;

  /** Placeholder text to display when an item is not selected. */
  placeHolder?: string;
}

/**
 * An interactive list of groups in the given org
 * @param props
 */
export const GroupPicker: React.FC<IProps> = (props: IProps) => {
  //////////[ Define State ]////////////////////////////////////////////////////////////////////////

  let [isLoaded, setIsLoaded] = useState<boolean>(false);
  let [groups, setGroups] = useState<Group[]>([]);

  //////////[ Variables ]///////////////////////////////////////////////////////////////////////////

  const loadMode = props.loadMode ?? GroupPickerLoadType.MyGroups;
  const placeHolder = isLoaded == true ? props.placeHolder : "Loading...";
  const disabled = isLoaded == true ? props.disabled : true;
  const isMulti = props.isMulti === true;

  //////////[ Initialize ]//////////////////////////////////////////////////////////////////////////

  async function initialize(): Promise<void> {
    // Only auto initialize when no groups specified and load mode is not none
    if (!props.groups && loadMode !== GroupPickerLoadType.None) {
      setIsLoaded(false);
      if (WidgetStore.isInitialized) {
        const groups =
          loadMode === GroupPickerLoadType.AllGroups
            ? await WidgetStore.organizations.readMyGroupsAsync(
                props.orgRoute,
                true
              )
            : await WidgetStore.organizations.readGroupsAsync(
                props.orgRoute,
                true
              );
        setIsLoaded(true);
        setGroups(groups);
      }
    } else {
      setGroups(props.groups ?? []);
      setIsLoaded(true);
    }
  }

  useEffect(() => {
    initialize();
  }, [WidgetStore.isInitialized, props.groups]);

  //////////[ Render ]//////////////////////////////////////////////////////////////////////////////

  return (
    <>
      <ItemPicker
        isMulti={isMulti}
        items={groups}
        onItemSelected={props.onItemSelected}
        onChange={props.onChange}
        placeHolder={placeHolder}
        disabled={disabled}
        getName={(i) => i.name}
        getValue={(i) => i}
      />
    </>
  );
};
