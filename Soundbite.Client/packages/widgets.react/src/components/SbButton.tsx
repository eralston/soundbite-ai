import React, { Fragment, ReactNode, useRef, useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { IconDefinition } from "@fortawesome/free-solid-svg-icons";

/***************************************************************************************************
 *  Enums
 **************************************************************************************************/

export enum SbButtonType {
  Primary,
  PrimaryOutline,
  Secondary,
  SecondaryOutline,
}

export enum SbButtonSize {
  Small,
  Large,
}

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  type?: SbButtonType; // Type of button
  size?: SbButtonSize; // Size of the button
  icon?: IconDefinition; // Icon associated with the button
  title?: string; // Hover text to display
  text?: string; // Button text to display
  disabled?: boolean;
  onClick?: () => void;
  btnClassName?: string; // Special button class to append with other btn class names
  style?: React.CSSProperties;
  children?: ReactNode;
}

/**
 * Provides a standard way of creating buttons in Soundbite Studio.
 */
export const SbButton: React.FC<IProps> = (props: IProps) => {
  const lastEventTime = useRef(0);
  //////////[ Methods - Utility ]///////////////////////////////////////////////////////////////////

  function getClassName(): string {
    const btnClassName: string = props.btnClassName
      ? ` ${props.btnClassName}`
      : "";
    const iconClass: string = props.icon ? " btn-icon" : "";
    let typeClass: string = "";
    let sizeClass: string = "btn-sm";

    switch (props.type) {
      case SbButtonType.Primary:
        typeClass = " btn-primary";
        break;
      case SbButtonType.PrimaryOutline:
        typeClass = " btn-outline-primary";
        break;
      case SbButtonType.Secondary:
        typeClass = " btn-secondary";
        break;
      case SbButtonType.SecondaryOutline:
        typeClass = " btn-outline-secondary";
        break;
    }

    switch (props.size) {
      case SbButtonSize.Large:
        sizeClass = " btn-lg";
        break;
      case SbButtonSize.Small:
      default:
        sizeClass = " btn-sm";
        break;
    }

    const result = `btn${iconClass}${typeClass}${sizeClass}${btnClassName}`;
    return result;
  }

  function TextContent() {
    if (props.children) {
      return <span className="btn-inner--text">{props.children}</span>;
    } else {
      return <span className="btn-inner--text">{props.text}</span>;
    }
  }

  function ButtonContent() {
    const hasText = props.text || props.children;
    if (props.icon && hasText) {
      return (
        <span className="btn-inner--icon">
          <FontAwesomeIcon icon={props.icon} />
          <TextContent />
        </span>
      );
    } else if (props.icon) {
      return (
        <span className="btn-inner--icon">
          <FontAwesomeIcon icon={props.icon} />
        </span>
      );
    } else {
      return <TextContent />;
    }
  }

  function onInteract(
    event:
      | React.MouseEvent<HTMLButtonElement>
      | React.TouchEvent<HTMLButtonElement>
  ) {
    const currentTime = new Date().getTime();

    const timeDiff = currentTime - lastEventTime.current;

    // Check if the time difference between the current event and the last event is less than a threshold (e.g., 300 ms)
    if (timeDiff < 300) {
      // Prevent the event from firing if the time difference is too small
      event?.preventDefault();
      event?.stopPropagation();
      return;
    }

    // Update the last event time
    lastEventTime.current = currentTime;

    props?.onClick?.();
  }

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  return (
    <button
      type="button"
      className={getClassName()}
      title={props.title}
      onClick={onInteract}
      onTouchEnd={onInteract}
      onPointerUp={onInteract}
      disabled={props.disabled}
      style={props.style}
    >
      <ButtonContent />
    </button>
  );
};
