import React, { useEffect, useState } from "react";
import Select from "react-select";
import makeAnimated from "react-select/animated";

/***************************************************************************************************
 *  Models
 **************************************************************************************************/

interface IOption {
  label: string;
  value: any;
  item: any;
}

export interface IListItemInfo {
  item: any;
  index: number;
  action: ListItemActionType;
}

export enum ListItemActionType {
  add = 1,
  none = 0,
  remove = -1,
}

/***************************************************************************************************
 *  Utility Methods
 **************************************************************************************************/

/**
 * Converts an ad-hoc item into an IOption instance using custom getName/getValue functions.
 * @param items - array of ad-hoc items to convert into an array of IOption items.
 * @param getName - function used to extract the display name of an item.
 * @param getValue - function used to extract the value of an item.
 */
function mapToOption(
  items: any,
  getName: (item: any) => string,
  getValue: (item: any) => any
): IOption[] {
  // Make sure if items is nullish to return an empty array
  if (!items) return [];

  // Map the items into options (e.g. something consumable by react-select)
  const mappedItems = (items as []).map((item) => {
    return {
      label: getName(item),
      value: getValue(item),
      item: item,
    };
  });

  return mappedItems;
}

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/

interface IProps {
  /** Flag indicating whether the picker chooses a single item or multiple items */
  isMulti?: boolean;

  /** Flag indicating whether the picker is enabled or disabled */
  disabled?: boolean;

  /** Flag indicating whether the selection menu automatically closes when an item is selected */
  closeMenuOnSelect?: boolean;

  /** Array of items to display in the picker */
  items?: any[];

  /** Array of items that are selected in the picker */
  selectedItems?: any[];

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

  /** Function used to extract the display name of an item */
  getName: (item: any) => string;

  /** Function used to extract the value of an item. */
  getValue: (item: any) => any;

  /** Placeholder text to display when an item is not selected. */
  placeHolder?: string;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/

/**
 * The ItemPicker component is a generic picker component with single/multi selection capability.
 * @param props - component properties
 */
export const ItemPicker: React.FC<IProps> = (props: IProps) => {
  //////////[ Variables ]///////////////////////////////////////////////////////////////////////////

  const placeHolder = props.placeHolder ?? "Select an Item...";
  const isMulti = props.isMulti === true ? true : false;
  const closeMenuOnSelect =
    isMulti === false || props.closeMenuOnSelect === true ? true : false;
  const mappedOptions = mapToOption(
    props.selectedItems,
    props.getName,
    props.getValue
  );

  //////////[ State ]///////////////////////////////////////////////////////////////////////////////

  let [isMounting, setIsMounting] = useState(true);
  let [allOptions, setAllOptions] = useState<IOption[]>(
    mapToOption(props.items, props.getName, props.getValue)
  );
  let [selectedOrig, setSelectedOrig] = useState<IOption[]>(mappedOptions);
  let [selected, setSelected] = useState<IOption[]>(mappedOptions);

  //////////[ Initialization ]//////////////////////////////////////////////////////////////////////

  function initialize() {
    if (isMounting) {
      setIsMounting(false);
    } else {
      const mappedOptions = mapToOption(
        props.selectedItems,
        props.getName,
        props.getValue
      );
      setAllOptions(mapToOption(props.items, props.getName, props.getValue));
      setSelectedOrig(mappedOptions);
      setSelected(mappedOptions);
    }
  }

  useEffect(() => {
    initialize();
  }, [props.items, props.selectedItems]);

  //////////[ Event Handlers ]//////////////////////////////////////////////////////////////////////

  function onChange(selected: any, actionMeta: any): void {
    // Update state based on the selected value(s)
    setSelected(selected);

    if (isMulti) {
      onChangeList((selected as any[]) ?? []);
    } else {
      if (props.onItemSelected) {
        props.onItemSelected(selected?.item);
      }
      onChangeList(selected ? [selected.item] : []);
    }
  }

  function onChangeList(selectedOptions: any[]): void {
    if (props.onChange) {
      // Determine the status of all originally selected items
      let changes: IListItemInfo[] = selectedOrig.map((origSel: any) => {
        return {
          item: origSel.item,
          index: (selectedOptions as []).findIndex(
            (selectedOption: any) => selectedOption.value == origSel.value
          ),
          // When an originally selected item appears in the selected list, nothing has changed
          // When an originally selected item does NOT appear in the selected list, it was removed
          action: (selectedOptions as any[]).find(
            (curSel) => curSel.value == origSel.value
          )
            ? ListItemActionType.none
            : ListItemActionType.remove,
        };
      });

      // Iterate through currently selected items
      selectedOptions.forEach((currentSelItem, index) => {
        // When a currently selected item is not present in the list of changes it is an addition
        if (
          !changes.find(
            (changeEntry) =>
              props.getValue(changeEntry.item) == currentSelItem.value
          )
        ) {
          changes.push({
            index: index,
            item: currentSelItem.item,
            action: ListItemActionType.add,
          });
        }
      });

      // Let callers know about any changes
      props.onChange(changes);
    }
  }

  //////////[ Render ]//////////////////////////////////////////////////////////////////////////////

  return (
    <Select
      className="sb-picker"
      classNamePrefix="sb-picker"
      options={allOptions}
      defaultValue={selected}
      value={selected}
      components={makeAnimated()}
      isMulti={isMulti}
      closeMenuOnSelect={closeMenuOnSelect}
      onChange={onChange}
      isClearable={false}
      isDisabled={props.disabled}
      placeholder={placeHolder}
    />
  );
};
