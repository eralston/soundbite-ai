import { DialogButtonStyleType } from "./DialogButtonStyleType";

/**
 * Interface for creating dialog action buttons.  Dialog action buttons allow for the dynamic
 * definition of actions in a dialog.
 **/
export interface DialogAction {
  /**
   * Name of the action as it appears to the user
   **/
  name: string;

  /**
   * Specifies the style of button (defaults to secondary)
   */
  style?: DialogButtonStyleType;

  /**
   * Flag indicating whether the action should be visible to the user.  A function that generates a
   * boolean value can also be provided.
   **/
  isVisible?: boolean | (() => boolean);

  /**
   * Flag indicating whether the action should be enabled for the user to click.  A function that
   * generates a boolean value can also be provided.
   **/
  isEnabled?: boolean | (() => boolean);

  /**
   * Code to execute when the action button is clicked by a user.
   * @returns a flag indicating success or failure of the custom code.  When false is returned, the
   * dialog will not close if isClose is set to true.
   **/
  onSelected?:
    | (() => boolean)
    | (() => void)
    | (() => Promise<boolean> | (() => Promise<void>));

  /**
   * Flag indicating that the dialog should close after the user clicks the button.  When this value
   * is set, the onSelected code will run before closing the dialog.
   **/
  isClose?: boolean;
}
