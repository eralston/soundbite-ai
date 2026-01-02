/**
 * Allows the DialogManager and the DialogManagerUi to communicate.  This could technically be on
 * the DialogManager directly but this hides the linkage so it does not appear in intellisense and
 * makes it harder to accidentally break.
 */
export class DialogManagerLink {
  static setCurrentDialogRef?: (currentDialog: React.ReactNode) => void;
}
