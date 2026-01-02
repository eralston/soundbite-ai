// This file is directly imported from the react-select-async-paginate project due to React version conflicts
// https://github.com/vtaits/react-select-async-paginate
// At some point, we should catch up to React 17, then it should be possible to replace these files with the real npm package again

import React, { ComponentType, Ref, ReactElement } from "react";
import { OptionTypeBase, Props as SelectProps } from "react-select";

import { useAsyncPaginate } from "./useAsyncPaginate";
import { useComponents } from "./useComponents";

import {
  UseAsyncPaginateResult,
  AsyncPaginateProps,
  WithAsyncPaginateType,
  GroupBase,
} from "./types";

export type Props<
  OptionType extends OptionTypeBase,
  Group extends GroupBase<OptionType>,
  Additional,
  IsMulti extends boolean
> = AsyncPaginateProps<OptionType, Group, Additional, IsMulti> & {
  useComponents?: typeof useComponents;
  useAsyncPaginate?: typeof useAsyncPaginate;
};

export function withAsyncPaginate(
  // eslint-disable-next-line @typescript-eslint/naming-convention
  SelectComponent: ComponentType<
    SelectProps<any, boolean> & {
      ref?: Ref<any>;
    }
  >
): WithAsyncPaginateType {
  function WithAsyncPaginate<
    OptionType extends OptionTypeBase,
    Group extends GroupBase<OptionType>,
    Additional,
    IsMulti extends boolean = false
  >(props: Props<OptionType, Group, Additional, IsMulti>): ReactElement {
    const {
      components,
      selectRef,
      isLoading: isLoadingProp,
      useComponents: useComponentsProp,
      useAsyncPaginate: useAsyncPaginateProp,
      cacheUniqs,
      ...rest
    } = props;

    // WARNING: Type inconsistency in the library
    if (useAsyncPaginateProp == null) {
      throw new Error("useAsyncPaginateProp cannot be null");
    }

    const asyncPaginateProps: UseAsyncPaginateResult<OptionType, Group> =
      useAsyncPaginateProp(rest, cacheUniqs);

    // WARNING: Type inconsistency in the library
    if (useComponentsProp == null) {
      throw new Error("useComponentsProp cannot be null");
    }

    const processedComponents = useComponentsProp<OptionType, IsMulti>(
      components
    );

    const isLoading =
      typeof isLoadingProp === "boolean"
        ? isLoadingProp
        : asyncPaginateProps.isLoading;

    return (
      <SelectComponent
        {...props}
        {...asyncPaginateProps}
        isLoading={isLoading}
        components={processedComponents}
        ref={selectRef}
      />
    );
  }

  WithAsyncPaginate.defaultProps = {
    selectRef: null,
    cacheUniqs: [],
    components: {},
    useComponents,
    useAsyncPaginate,
  };

  return WithAsyncPaginate;
}
