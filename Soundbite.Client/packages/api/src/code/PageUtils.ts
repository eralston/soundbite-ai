import { IndexPageResponse } from "../generated/models/IndexPageResponse.model";

/** Utility class for page utilities */
export class PageUtils {
  /**
   * Loads the given page into the given map, using the determined index for the page
   * @param page
   * @param map
   */
  static addPageToMap<TPageType>(
    page: IndexPageResponse<TPageType>,
    map: Map<number, TPageType>
  ) {
    const skip = page.request.skip ?? 0;
    for (let i = 0; i < page.result.length; ++i) {
      const person = page.result[i];
      map.set(skip + i, person);
    }
  }

  /**
   * Returns true if the given page response likely has more pages ahead of it
   * @param response
   */
  static hasMorePages<T>(response: IndexPageResponse<T>): boolean {
    const request = response.request;
    // Be pessimistic for this one
    const skip = request?.skip ?? 0;
    const take = request?.take ?? 1;

    // At the end of all pages
    if (skip >= response.maxTotalResults) {
      return false;
    }

    if (response.totalCount != null) {
      return response.totalCount > skip + take;
    }

    return response.result.length >= take;
  }
}
