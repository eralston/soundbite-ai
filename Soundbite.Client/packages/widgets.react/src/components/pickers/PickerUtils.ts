/**
 * Consumed by a paginating picker to evaluate if we're close enough to the bottom of the picker frame
 * @param scrollHeight
 * @param clientHeight
 * @param scrollTop
 */
export const shouldLoadMorePickerPages = (
  scrollHeight: number,
  clientHeight: number,
  scrollTop: number
): boolean => {
  if (scrollHeight <= clientHeight) {
    return false;
  }

  // Is the bottom of the frame touching the total height yet within a boundary?
  const bottomOfFrame = scrollTop + clientHeight;
  const padding = 2;
  const threshold = scrollHeight - bottomOfFrame - padding;
  const shouldLoad = threshold < 0;
  return shouldLoad;
};
