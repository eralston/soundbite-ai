import { Session } from "@soundbite/api";

export const compareLex = (left: string = "", right: string = "") => {
  if (left < right) {
    return -1;
  }
  if (left > right) {
    return 1;
  }
  return 0;
};

type getter<TObj> = (obj: TObj) => string | undefined;

/**
 * Sorts the given collection using the given functio to get the target field
 * @param collection
 * @param getFunc
 */
export const sortBy = <TObj>(
  getFunc: getter<TObj>,
  collection?: TObj[]
): TObj[] | undefined => {
  if (collection == null) return undefined;

  const ret: TObj[] = collection.sort(function (left, right) {
    const leftVal = getFunc(left);
    const rightVal = getFunc(right);
    return compareLex(leftVal, rightVal);
  });
  return ret;
};

/**
 * Sorts the given set of sessions, returning a new collection
 * @param sessions
 */
export const sortSessions = <SessionType extends Session>(
  sessions?: SessionType[]
): SessionType[] | undefined => {
  const ret = sortBy((s) => {
    return s.publish;
  }, sessions);
  return ret;
};

//export const sortSeries = <SeriesType extends Series>(series?: SeriesType[]): SeriesType[] | undefined => {
//  const ret = sortBy((s) => { return s. }, series);
//  return ret;
//}
