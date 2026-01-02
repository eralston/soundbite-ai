import { ErrorDlg } from "../components/ErrorDlg";

/**
 * Helper that ensures only one active call can occur for an infinite scroll table at a time.
 * Data from each subsequent call is stored off for processing when the current call completes.
 * Each subsequent call also overwrites any previous subsequent calls.  The theory here is that
 * when the current request completes the last subsequent call fires because it represents the
 * current position of the table and all the other requests have been scrolled by and are no
 * longer applicable.
 */
export class InfiniteScrollDataHelper {
  constructor(
    getMoreItemsAction: (startIndex: number, stopIndex: number) => Promise<void>
  ) {
    this.getMoreItemsAction = getMoreItemsAction;
  }

  requestPromise: Promise<void> | null = null;
  waitPromise: Promise<void> | null = null;
  waitStart: number = 0;
  waitStop: number = 0;
  getMoreItemsAction: (startIndex: number, stopIndex: number) => Promise<void>;

  async getMoreItems(startIndex: number, stopIndex: number): Promise<void> {
    if (this.requestPromise == null) {
      this.requestPromise = this.getMoreItemsAction(startIndex, stopIndex);
      this.requestPromise.catch((err) => {
        ErrorDlg.show(
          err,
          "Error Loading Sessions",
          `Error querying for sessions ${startIndex} to ${stopIndex}:`
        );
      });
      this.requestPromise.then(() => (this.requestPromise = null));
      return this.requestPromise;
    } else {
      this.waitStart = startIndex;
      this.waitStop = stopIndex;
      if (this.waitPromise == null) {
        this.waitPromise = new Promise<void>(async (resolve, reject) => {
          try {
            await this.requestPromise;
            this.requestPromise = this.getMoreItemsAction(
              this.waitStart,
              this.waitStop
            );
            await this.requestPromise;
            this.waitPromise = null;
            this.requestPromise = null;
            resolve();
          } catch (ex) {
            reject(ex);
          }
        });
      }
      return this.waitPromise;
    }
  }
}
