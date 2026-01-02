import { BrowserStorageService } from "./BrowserStorageService";
import {
  ClientOp,
  ClientOpRunInfo,
  SessionsService,
  SessionCalendarEntry,
} from "@soundbite/api";

import { ProviderFactory } from "./providers/ProviderFactory";
import { SbAuthError } from "./providers/auth/AadAuthProvider";

/**
 * Encapsulates all of the business logic for managing client operations requested from the API.
 * Some server-side operations require client-side interaction.  These mostly involve third-party
 * provider operations involving user access tokens that are more effectively managed on the client.
 */
class ClientOpServiceClass {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  /**
   * Stores an array of the client operations that are pending.
   */
  private failedClientOps: ClientOpRunInfo[];

  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  /**
   * Gets a flag indicating whether there are outstanding client operations that need user
   * interaction to acquire a third-party provider token in order to process.
   */
  get hasFailedOps() {
    return this.failedClientOps.length > 0;
  }

  /**
   * Gets a flag indicating whether any failed client operations require re-authentication to process.
   */
  get requiresAuthToProcessFailedOps() {
    return (
      this.failedClientOps && this.failedClientOps.some((i) => i.authRequired)
    );
  }

  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////

  /**
   * Initializes the ClientOpService class
   */
  constructor() {
    this.failedClientOps = BrowserStorageService.failedClientOps;
  }

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Registers a client operation for processing once the appropriate auth token has been acquired.
   * @param opsToRegister - array of client operations to register
   */
  registerFailedClientOp(failedOp?: ClientOpRunInfo) {
    if (failedOp) {
      this.failedClientOps.push(failedOp);
      BrowserStorageService.failedClientOps = this.failedClientOps;
    }
  }

  /**
   * Handles execution of all the specified client operations. Client operations are normally
   * received as the result of a API call that responds with a ClientOpWrapper.  The ClientOpWrapper
   * contains a list (which may be null or empty) of client operations that need to complete.
   * This method is intended to process that list.
   * @param clientOps - client operations to run
   * @returns - a flag indicating whether all client ops were processed
   */
  async processClientOps(clientOps?: ClientOp[]): Promise<ClientOpRunInfo[]> {
    let failedOps: ClientOpRunInfo[] = [];

    if (clientOps && clientOps.length > 0) {
      for (let i = 0; i < clientOps.length; i++) {
        const clientOp = clientOps[i];
        if (clientOp) {
          // By convention the operation type is the name of a function that handles that operation.
          // The following code casts this class to any and retrieves the function name using a
          // dictionary lookup.  If the handler function is located then it is executed.
          var op = (this as any)[clientOp.opType];
          try {
            if (op) {
              await op(clientOp.data);
            }
          } catch (ex: any) {
            const isAuthFailure = ex && ex instanceof SbAuthError;
            let clientOpInfo = clientOp as ClientOpRunInfo;
            clientOpInfo.failCount =
              clientOpInfo.failCount > 0 ? clientOpInfo.failCount++ : 1;
            clientOpInfo.authRequired = isAuthFailure;
            failedOps.push(clientOpInfo);
          }
        }
      }
    }

    if (failedOps.length > 0) {
      failedOps.forEach((failedOp) => {
        if (this.failedClientOps.indexOf(failedOp) < 0) {
          this.failedClientOps.push(failedOp);
        }
      });
      BrowserStorageService.failedClientOps = this.failedClientOps;
    }

    return failedOps;
  }

  /**
   * Re-runs any client operations that were registered as having failed.
   */
  async processFailedClientOps(): Promise<void> {
    const toRun = this.failedClientOps.filter((i) => i.failCount < 3);
    const failedAgain = await this.processClientOps(toRun);
    const succeeded = toRun.filter((i) => failedAgain.indexOf(i) < 0);
    this.failedClientOps = this.failedClientOps.filter(
      (i) => succeeded.indexOf(i) < 0
    );
    BrowserStorageService.failedClientOps = this.failedClientOps;
  }

  /**
   * Responsible for creating/updating the session calendar event reminder.
   * @param data - contains information about the calendar entry to create or update.
   */
  protected async CreateSessionReminderCalendarEntry(
    data: SessionCalendarEntry
  ): Promise<void> {
    var calendarProvider = ProviderFactory.getCalendarProvider();
    var calendarEntryId = await calendarProvider.createEvent(data);
    SessionsService.setReminderCalendarEntryId(
      data.orgRoute,
      data.sessionRoute,
      calendarEntryId
    );
  }
}

export const ClientOpService = new ClientOpServiceClass();
