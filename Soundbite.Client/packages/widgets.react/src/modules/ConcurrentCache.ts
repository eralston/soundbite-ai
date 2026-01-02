/**
 * Implements a cache that not only holds onto objects, it holds onto and reuses reuests for those objects
 * This enables not only easy management of current data; it ensures only one request per key is active in a given time
 * */
export class ConcurrentCache<TargetType> {
  /** A mapping from keys to objects */
  protected objects: Map<string, TargetType> = new Map<string, TargetType>();

  /** A mapping from keys to pending promises */
  protected promises: Map<string, Promise<TargetType>> = new Map<
    string,
    Promise<TargetType>
  >();

  /** Clear the cache */
  clear(): void {
    this.objects.clear();
    this.promises.clear();
  }

  /**
   * Clear the given key in the cache
   * @param cacheKey
   * @returns True if it was seen before deletion; otherwise, false
   */
  delete(cacheKey: string): boolean {
    const wasCached = this.objects.delete(cacheKey);
    const wasPending = this.promises.delete(cacheKey);
    return wasCached || wasPending;
  }

  /**
   * Manually and synchronously set the value for a given object; this invalidates any pending requests for that item
   * @param cacheKey
   * @param value
   */
  set(cacheKey: string, value: TargetType): void {
    this.promises.delete(cacheKey);
    this.objects.set(cacheKey, value);
  }

  /**
   * Async retrieve the object, but only if is currently live in the cache
   * @param cacheKey
   */
  get(cacheKey: string): TargetType | undefined {
    return this.objects.get(cacheKey);
  }

  /**
   * Returns true if the given promise is live and never discarded; otherwise, false if it has been invalidated or superseded
   * @param cacheKey
   * @param currentPromise
   */
  protected isPendingPromise(
    cacheKey: string,
    currentPromise: Promise<TargetType>
  ): boolean {
    const pendingPromise = this.promises.get(cacheKey);
    return pendingPromise === currentPromise;
  }

  /**
   * Async retrieve an object; optionally reusing cached items and active requests
   * @param cacheKey Must be unique value
   * @param reader Optionally returns a promise to call in the event of a cache miss
   * @param allowCache By default, reuse values from the cache
   */
  async getAsync(
    cacheKey: string,
    reader: () => Promise<TargetType> | undefined,
    allowCache: boolean = true
  ): Promise<TargetType> {
    // Try to find in cache
    let cached: TargetType | undefined = allowCache
      ? this.get(cacheKey)
      : undefined;

    if (cached) {
      return cached;
    }

    // Try to reuse promise if pending
    let promised: Promise<TargetType> | undefined = allowCache
      ? this.promises.get(cacheKey)
      : undefined;

    if (promised != null) {
      return promised;
    }

    // If we're forced, then use reader, but we may not get anything
    const newPromise = reader();

    if (newPromise == null) {
      throw new Error(
        `Could not start read request for item with cache key '${cacheKey}'`
      );
    }

    // Since we have a useful promise, we hold onto it for concurrency
    this.promises.set(cacheKey, newPromise);

    // We never await on this promise, so we have to resolve old school
    newPromise
      .then((value) => {
        // We cannot allow empty values in this class; they should be treated like errors
        // In a perfect world, we would get a 404 from the API before this is ever called
        if (value == null) {
          throw new Error(
            `No value returned for read request with cache key '${cacheKey}'`
          );
        }

        // If the promise was kicked out while waiting, then we do NOT save it for later
        if (this.isPendingPromise(cacheKey, newPromise)) {
          this.objects.set(cacheKey, value);
        }

        return value;
      })
      .finally(() => {
        // While waiting on the new promise, this may have evicted this promise
        // If that is the case, then the poor soul waiting on the old promise will get nothing
        if (this.isPendingPromise(cacheKey, newPromise)) {
          this.promises.delete(cacheKey);
        }
      });
    return newPromise;
  }
}
