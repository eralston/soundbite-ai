// This file is directly imported from the react-select-async-paginate project due to React version conflicts
// https://github.com/vtaits/react-select-async-paginate
// At some point, we should catch up to React 17, then it should be possible to replace these files with the real npm package again

import { ReduceOptions } from "./types";

export const reduceGroupedOptions: ReduceOptions<any, any, any> = (
  prevOptions,
  loadedOptions
) => {
  const res = prevOptions.slice();

  const mapLabelToIndex: any = {};
  let prevOptionsIndex = 0;
  const prevOptionsLength = prevOptions.length;

  loadedOptions.forEach((group: any) => {
    const { label } = group;

    let groupIndex = mapLabelToIndex[label];
    if (typeof groupIndex !== "number") {
      for (
        ;
        prevOptionsIndex < prevOptionsLength &&
        typeof mapLabelToIndex[label] !== "number";
        ++prevOptionsIndex
      ) {
        const prevGroup = prevOptions[prevOptionsIndex];
        mapLabelToIndex[prevGroup.label] = prevOptionsIndex;
      }

      groupIndex = mapLabelToIndex[label];
    }

    if (typeof groupIndex !== "number") {
      mapLabelToIndex[label] = res.length;
      res.push(group);
      return;
    }

    res[groupIndex] = {
      ...res[groupIndex],
      options: res[groupIndex].options.concat(group.options),
    };
  });

  return res;
};
