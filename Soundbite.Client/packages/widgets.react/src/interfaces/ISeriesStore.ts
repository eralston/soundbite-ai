import { SeriesPreview, SeriesDetails } from "@soundbite/api";

/**
 * Interface defininig the contract for a series store implementation.
 */
export interface ISeriesStore {
  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Gets the specified series.
   * @param orgRoute - route of the organization with which the series is associated.
   * @param seriesRoute - route of the series to acquire.
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  readSeriesAsync(
    orgRoute: string,
    seriesRoute: string,
    allowCache?: boolean
  ): Promise<SeriesDetails>;

  /**
   * Updates the specified series.
   * @param orgRoute - route of the organization with which the series is associated.
   * @param series - updated series to save.
   */
  updateSeriesAsync(orgRoute: string, series: SeriesPreview): Promise<void>;

  /**
   * Deletes the specified series.
   * @param orgRoute - route of the organization with which the series is assocaited.
   * @param seriesRoute - route of the series to delete.
   */
  deleteSeriesAsync(orgRoute: string, seriesRoute: string): Promise<void>;
}
