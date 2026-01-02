import { makeObservable, runInAction } from "mobx";
import { SeriesService, SeriesPreview, SeriesDetails } from "@soundbite/api";
import { ISeriesStore } from "../interfaces/ISeriesStore";
import { SessionStore } from "./SessionStore";

/**
 * MobX store class containing states and actions for series
 */
class SeriesStoreClass implements ISeriesStore {
  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {});
  }

  //////////[ Observable Data Properties ]//////////////////////////////////////////////////////////

  /* None */

  //////////[ Cache Members ]///////////////////////////////////////////////////////////////////////

  seriesCache: { [name: string]: SeriesDetails } = {};

  /**
   * Responsible for clearing ALL cached data in the store.
   */
  clearCache(): void {
    this.seriesCache = {};
  }

  /**
   * Creates a key used for caching operations
   * @param orgRoute - route of the organization with which the session is associated
   * @param seriesRoute - route of the series
   */
  private getCacheKey(orgRoute: string, seriesRoute: string): string {
    return `${orgRoute}-${seriesRoute}`;
  }

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Gets the specified series.
   * @param orgRoute - route of the organization with which the series is associated.
   * @param seriesRoute - route of the series to acquire.
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  async readSeriesAsync(
    orgRoute: string,
    seriesRoute: string,
    allowCache: boolean = true
  ): Promise<SeriesDetails> {
    let series: SeriesDetails | null = allowCache
      ? this.seriesCache[this.getCacheKey(orgRoute, seriesRoute)]
      : null;
    if (!series) {
      series = await SeriesService.readAsync(orgRoute, seriesRoute);
      this.seriesCache[this.getCacheKey(orgRoute, seriesRoute)] = series;
    }
    return series;
  }

  /**
   * Updates the specified series.
   * @param orgRoute - route of the organization with which the series is associated.
   * @param series - updated series to save.
   */
  async updateSeriesAsync(
    orgRoute: string,
    series: SeriesPreview
  ): Promise<void> {
    // Not Implemented?
  }

  /**
   * Deletes the specified series.
   * @param orgRoute - route of the organization with which the series is assocaited.
   * @param seriesRoute - route of the series to delete.
   */
  async deleteSeriesAsync(
    orgRoute: string,
    seriesRoute: string
  ): Promise<void> {
    await SeriesService.deleteAsync(orgRoute, seriesRoute);

    // Remove from local
    SessionStore.orgSeries = SessionStore.orgSeries?.filter(
      (i) => i.route !== seriesRoute
    );
    SessionStore.groupSeries = SessionStore.groupSeries?.filter(
      (i) => i.route !== seriesRoute
    );
    delete this.seriesCache[this.getCacheKey(orgRoute, seriesRoute)];
  }
}

// Export a singleton instance of the store
export const SeriesStore = new SeriesStoreClass();
