// This file is directly imported from the react-select-async-paginate project due to React version conflicts
// https://github.com/vtaits/react-select-async-paginate
// At some point, we should catch up to React 17, then it should be possible to replace these files with the real npm package again

import { ReduceOptions } from "./types";

export const defaultReduceOptions: ReduceOptions<any, any, any> = (
  prevOptions,
  loadedOptions
) => [...prevOptions, ...loadedOptions];
