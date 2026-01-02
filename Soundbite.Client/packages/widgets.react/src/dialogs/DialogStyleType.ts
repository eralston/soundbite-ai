/**
 * Enumeration of the various dialog styles
 */
export enum DialogStyleType {
  /**
   * Denotes the dialog is a normal dialog
   */
  Normal = 0,

  /**
   * Denotes that the dialog is a big ol' boy
   */
  Big = 1,

  /**
   * Denotes that the dialog exposes potentially dangers operations
   */
  Danger = 2,

  /**
   * Denotes that this is the new session dialog
   */
  SessionEditor = 3,

  /**
   * Denotes taht this is the big new session dialog
   */
  SessionEditorBig = 4,
}
