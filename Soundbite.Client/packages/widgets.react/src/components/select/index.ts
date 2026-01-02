// This file is directly imported from the react-select-async-paginate project due to React version conflicts
// https://github.com/vtaits/react-select-async-paginate
// At some point, we should catch up to React 17, then it should be possible to replace these files with the real npm package again

import Select from "react-select";

import { withAsyncPaginate } from "./withAsyncPaginate";

export { wrapMenuList } from "./wrapMenuList";
export { reduceGroupedOptions } from "./reduceGroupedOptions";

export { withAsyncPaginate };

export { useAsyncPaginateBase } from "./useAsyncPaginateBase";
export { useAsyncPaginate } from "./useAsyncPaginate";
export { useComponents } from "./useComponents";

export const AsyncPaginate = withAsyncPaginate(Select);

export * from "./types";
