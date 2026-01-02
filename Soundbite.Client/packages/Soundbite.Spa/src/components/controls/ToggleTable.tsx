import { ShowWhen, Toggle } from "@soundbite/widgets-react";
import React from "react";

interface ToggleRowProps {
  title: string;
  subtitle?: string;
  onToggle: (onEnabled: boolean) => void;
  defaultValue: boolean;
  disabled?: boolean;
}

interface DropDownRowProps {
  title: string;
  subtitle?: string;
  onChange: (value: string) => void;
  choices: { label: string; value: string }[];
  defaultValue: string;
}

interface TextRowProps {
  title: string;
  subtitle?: string;
  onChange: (value: string) => void;
  defaultValue: string;
  value?: string;
}

/**
 * A single row with a title, optional subtitle, and a toggle control
 * @param props
 */
export const ToggleRow: React.FC<ToggleRowProps> = (props: ToggleRowProps) => {
  return (
    <tr>
      <th scope="row">
        <div>{props.title}</div>
        <ShowWhen is={props.subtitle != null}>
          <div className="text-muted text-sm d-none d-md-block font-weight-light">
            {props.subtitle}
          </div>
        </ShowWhen>
      </th>
      <td>
        <Toggle
          disabled={props.disabled ?? false}
          defaultValue={props.defaultValue}
          onToggle={props.onToggle}
          title={props.title}
        />
      </td>
    </tr>
  );
};

/**
 * A single row with a title, optional subtitle, and a toggle control
 * @param props
 */
export const DropDownRow: React.FC<DropDownRowProps> = (
  props: DropDownRowProps
) => {
  return (
    <tr>
      <th scope="row">
        <div>{props.title}</div>
        <ShowWhen is={props.subtitle != null}>
          <div className="text-muted text-sm d-none d-md-block font-weight-light">
            {props.subtitle}
          </div>
        </ShowWhen>
      </th>
      <td>
        <select
          onChange={(e) => {
            props.onChange(e.target.value);
          }}
        >
          {props.choices.map((item) => {
            return (
              <option
                value={item.value}
                selected={item.value === props.defaultValue}
              >
                {item.label}
              </option>
            );
          })}
        </select>
      </td>
    </tr>
  );
};

export const TextRow: React.FC<TextRowProps> = (props: TextRowProps) => {
  return (
    <tr>
      <th scope="row">
        <div>{props.title}</div>
        <ShowWhen is={props.subtitle != null}>
          <div className="text-muted text-sm d-none d-md-block font-weight-light">
            {props.subtitle}
          </div>
        </ShowWhen>
      </th>
      <td>
        <input
          type="Text"
          style={{ width: "100%" }}
          onChange={(e) => {
            props.onChange(e.target.value);
          }}
          defaultValue={props.defaultValue}
          value={props.value}
        />
      </td>
    </tr>
  );
};

interface ToggleHeaderProps {
  title?: string;
}

/**
 * The separate header for the toggle table
 * @param props
 */
export const ToggleHeader: React.FC<ToggleHeaderProps> = (
  props: ToggleHeaderProps
) => {
  return (
    <tr className="thead-light">
      <th scope="col" className="sort" data-sort="name">
        {props.title}
      </th>
      <th scope="col" className="sort" data-sort="action"></th>
    </tr>
  );
};

interface ToggleTableProps {
  title?: string;
  hideHeader?: boolean;
  children: React.ReactNode;
}

/**
 * The wrapper for the toggle table
 * To use:
 * <ToggleTable title="Example">
 *  <ToggleRows {...props} />
 * </ToggleTable>
 * @param props
 */
export const ToggleTable: React.FC<ToggleTableProps> = (
  props: ToggleTableProps
) => {
  return (
    <table className="table align-items-center">
      <ShowWhen is={!props.hideHeader}>
        <ToggleHeader title={props.title} />
      </ShowWhen>
      <tbody>{props.children}</tbody>
    </table>
  );
};
