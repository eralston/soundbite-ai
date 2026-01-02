import React from "react";
import { Modal, ModalBody, ModalFooter, ModalHeader } from "reactstrap";

import { DialogAction } from "./DialogAction";
import { DialogButtonStyleType } from "./DialogButtonStyleType";
import { DialogStyleType } from "./DialogStyleType";

/**
 * Contains static helper methods for building dialog boxes.
 */
export class DialogHelper {
  //////////[ Methods - Public ]////////////////////////////////////////////////////////////////////

  /**
   * Renders a dialog based on configuration parameters.
   * @param dialogStyle - specifies the style of the dialog.
   * @param title - title that appears in the header of the dialog.
   * @param subTitle - sub-title that appears below the title in the header of the dialog.
   * @param actions - actions that appear as buttons in the footer of th dialog.
   * @param body - content to display in the dialog body.
   * @param isActionRunning - reference to the isActionRunning state value.
   * @param setIsActionRunning - referenced to the setIsActionRunning function to modify isActionRunning state.
   * @param closeDialog - method that closes the dialog when called.
   */
  static RenderDialog(
    dialogStyle: DialogStyleType,
    title: string,
    actions: DialogAction[],
    body: JSX.Element,
    isActionRunning: boolean,
    setIsActionRunning: (isRunning: boolean) => void,
    closeDialog: () => void
  ): JSX.Element {
    return DialogHelper.RenderDialogWithSubTitle(
      dialogStyle,
      title,
      "",
      actions,
      body,
      isActionRunning,
      setIsActionRunning,
      closeDialog
    );
  }

  /**
   * Renders a dialog based on configuration parameters.
   * @param dialogStyle - specifies the style of the dialog.
   * @param title - title that appears in the header of the dialog.
   * @param subTitle - sub-title that appears below the title in the header of the dialog.
   * @param actions - actions that appear as buttons in the footer of th dialog.
   * @param body - content to display in the dialog body.
   * @param isActionRunning - reference to the isActionRunning state value.
   * @param setIsActionRunning - referenced to the setIsActionRunning function to modify isActionRunning state.
   * @param closeDialog - method that closes the dialog when called.
   */
  static RenderDialogWithSubTitle(
    dialogStyle: DialogStyleType,
    title: string,
    subTitle: string,
    actions: DialogAction[],
    body: JSX.Element,
    isActionRunning: boolean,
    setIsActionRunning: (isRunning: boolean) => void,
    closeDialog: () => void
  ): JSX.Element {
    return (
      <Modal isOpen={true} className={DialogHelper.getModalStyle(dialogStyle)}>
        {this.renderHeader(dialogStyle, title, subTitle)}
        {this.renderBody(body)}
        {this.renderFooter(
          actions,
          isActionRunning,
          setIsActionRunning,
          closeDialog
        )}
      </Modal>
    );
  }

  //////////[ Methods - Private ]///////////////////////////////////////////////////////////////////

  /**
   * Disambiguates a boolean value when a variable is either a boolean value or a boolean function.
   * @param value - value to resolve
   * @param defaultValue - default value to apply if the value is undefined.
   */
  private static resolveBoolean(
    value: undefined | boolean | (() => boolean),
    defaultValue: boolean
  ) {
    if (value === undefined) {
      return defaultValue;
    } else if (typeof value === "function") {
      return (value as () => boolean)();
    } else {
      return value as boolean;
    }
  }

  /**
   * Responsible for building an action button based on the specified DialogAction configuration.
   * @param action - action button configuration
   * @param index - action buttion index used for giving the button a unique key
   */
  private static buildActionButton(
    action: DialogAction,
    index: number,
    isActionRunning: boolean,
    setIsActionRunning: (isRunning: boolean) => void,
    closeDialog: () => void
  ) {
    if (DialogHelper.resolveBoolean(action.isVisible, true)) {
      let buttonAction = async () => {
        // Acquire the onSelected value (if applicable)
        let onSelectedValue: any = true;

        // Process the onSelected action of the button (if applicable)
        if (action.onSelected) {
          // Run the action
          setIsActionRunning(true);
          try {
            onSelectedValue = action.onSelected();
          } catch {
            setIsActionRunning(false);
          }

          // Determine if onSelectedValue is a promise that needs to be awaited
          if (typeof onSelectedValue === "object") {
            // Action is a promise so await it
            try {
              onSelectedValue = await onSelectedValue;
            } finally {
              setIsActionRunning(false);
            }
          } else {
            // Action was not a promise and is therefore complete
            setIsActionRunning(false);
          }
        }

        // Determine if action closes dialog and onSelectedValue did not return false
        if (action.isClose && onSelectedValue !== false) {
          closeDialog();
        }
      };

      return (
        <button
          key={index}
          className={DialogHelper.getActionButtonStyle(action.style)}
          disabled={
            isActionRunning ||
            !DialogHelper.resolveBoolean(action.isEnabled, true)
          }
          onClick={() => {
            buttonAction();
          }}
          title={action.name}
        >
          {action.name}
        </button>
      );
    } else {
      return null;
    }
  }

  /**
   * Converts the dialog button style enum into a CSS class string
   * @param buttonStyle - button style enum to convert to CSS class string
   */
  static getActionButtonStyle(buttonStyle?: DialogButtonStyleType): string {
    switch (buttonStyle) {
      case DialogButtonStyleType.Danger:
        return "btn btn-danger";
      case DialogButtonStyleType.Primary:
        return "btn btn-primary";
      default:
        return "btn btn-secondary";
    }
  }

  /**
   * Converts the dialog style enum into a CSS class string for the modal header element
   * @param dialogStyle - style enum to convert to a CSS class string
   */
  private static getModalStyle(dialogStyle?: DialogStyleType): string {
    switch (dialogStyle) {
      case DialogStyleType.SessionEditor:
        return "sb-new-session-modal";
      case DialogStyleType.SessionEditorBig:
        return "sb-new-session-modal sb-big-dialog";
      default:
        return "";
    }
  }

  /**
   * Converts the dialog style enum into a CSS class string for the modal header element
   * @param dialogStyle - style enum to convert to a CSS class string
   */
  private static getHeaderStyle(dialogStyle?: DialogStyleType): string {
    switch (dialogStyle) {
      case DialogStyleType.Danger:
        return "bg-gradient-danger";
      default:
        return "bg-gradient-primary";
    }
  }

  private static renderSubTitle(subTitle: string): JSX.Element | null {
    if (subTitle) {
      return (
        <span className="sb-modal-header-subtitle">
          <span className="">{subTitle}</span>
        </span>
      );
    } else {
      return null;
    }
  }

  private static renderHeader(
    dialogStyle: DialogStyleType,
    title: string,
    subTitle: string
  ): JSX.Element | null {
    if (title) {
      return (
        <ModalHeader className={this.getHeaderStyle(dialogStyle)}>
          {title}
          {this.renderSubTitle(subTitle)}
        </ModalHeader>
      );
    } else {
      return null;
    }
  }

  private static renderBody(component: JSX.Element): JSX.Element | null {
    return (
      <ModalBody>
        {component ? component : <p>No dialog body component provided ...</p>}
      </ModalBody>
    );
  }

  private static renderFooter(
    actions: DialogAction[],
    isActionRunning: boolean,
    setIsActionRunning: (isRunning: boolean) => void,
    closeDialog: () => void
  ): JSX.Element | null {
    if (actions && actions.length > 0) {
      return (
        <ModalFooter>
          {actions.map((action, index) =>
            DialogHelper.buildActionButton(
              action,
              index,
              isActionRunning,
              setIsActionRunning,
              closeDialog
            )
          )}
        </ModalFooter>
      );
    } else {
      return null;
    }
  }
}
