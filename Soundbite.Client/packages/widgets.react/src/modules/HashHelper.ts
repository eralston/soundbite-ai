import QueryString from "query-string";
import { WidgetStore } from "..";

/**
 * Available hash values in the system for displaying dialog
 * */
export enum DialogHash {
  Play = "sbplay",
  Record = "sbrecord",
  CreateSession = "sbcreatesession",
  CreateSessionExpanded = "sbcreatesessionexpanded",
}

/**
 * Checks the dialog hash for the given values
 * @param orgRoute
 * @param groupRoute
 */
export const checkDialogHash = (
  orgRoute: string,
  groupRoute?: string
): void => {
  const parsedQueryString = QueryString.parse(window.location.hash);

  // If the key is NOT in the string, parsedQueryString returns undefined
  // If the key is in the string but with no value, parsedQueryString returns null
  // If the key and value is in the string, parsedQueryString returns the value

  // Show the player if requested in the URL hash
  const playSessionRoute = parsedQueryString[DialogHash.Play] as
    | string
    | undefined;
  if (playSessionRoute) {
    WidgetStore.showPlayer(orgRoute, playSessionRoute);
  }

  // Show the recorder if requested in the URL hash
  const recordSessionRoute = parsedQueryString[DialogHash.Record] as
    | string
    | undefined;
  if (recordSessionRoute) {
    WidgetStore.showRecorder(orgRoute, recordSessionRoute);
  }

  // Show the recorder if sbcreatesession exists (is not exactly undefined)
  const createSessionHash = parsedQueryString[DialogHash.CreateSession] as
    | string
    | undefined;
  if (createSessionHash !== undefined) {
    WidgetStore.showNewSession(orgRoute, groupRoute);
  }

  // Show the recorder if sbcreateseries exists (is not exactly undefined)
  const createSeriesHash = parsedQueryString[
    DialogHash.CreateSessionExpanded
  ] as string | undefined;
  if (createSeriesHash !== undefined) {
    WidgetStore.showNewSession(orgRoute, groupRoute, true);
  }
};
