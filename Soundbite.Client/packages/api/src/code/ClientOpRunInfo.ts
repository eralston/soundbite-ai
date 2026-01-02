import { ClientOp } from "..";

/**
 * Extends the ClientOp to include information that is only used within the application to manage
 * authentication and execution issues that occur when running operations requested by the server.
 */
export interface ClientOpRunInfo extends ClientOp {
  /**
   * Gets or sets the number of times a client operation has failed.
   */
  failCount: number;

  /**
   * Gets or sets a flag indicating that an authentication issue occured running the client
   * operation and interactive user authenticaiton is required to run the operation again.
   */
  authRequired: boolean;
}
