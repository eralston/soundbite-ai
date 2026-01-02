// This file is directly imported from the react-select-async-paginate project due to React version conflicts
// https://github.com/vtaits/react-select-async-paginate
// At some point, we should catch up to React 17, then it should be possible to replace these files with the real npm package again

import { useState, useCallback } from "react";
import { InputActionMeta } from "react-select";

import { useAsyncPaginateBase } from "./useAsyncPaginateBase";

import {
  UseAsyncPaginateParams,
  UseAsyncPaginateBaseParams,
  UseAsyncPaginateBaseResult,
  UseAsyncPaginateResult,
  GroupBase,
} from "./types";

export const useAsyncPaginatePure = <
  OptionType,
  Group extends GroupBase<OptionType>,
  Additional
>(
  useStateParam: typeof useState,
  useCallbackParam: typeof useCallback,

  useAsyncPaginateBaseParam: (
    params: UseAsyncPaginateBaseParams<OptionType, Group, Additional>,
    deps: ReadonlyArray<any>
  ) => UseAsyncPaginateBaseResult<OptionType, Group>,

  params: UseAsyncPaginateParams<OptionType, Group, Additional>,
  deps: ReadonlyArray<any> = []
): UseAsyncPaginateResult<OptionType, Group> => {
  const {
    inputValue: inputValueParam,
    menuIsOpen: menuIsOpenParam,
    defaultInputValue: defaultInputValueParam,
    defaultMenuIsOpen: defaultMenuIsOpenParam,
    onInputChange: onInputChangeParam,
    onMenuClose: onMenuCloseParam,
    onMenuOpen: onMenuOpenParam,
  } = params;

  const [inputValueState, setInputValue] = useStateParam(
    defaultInputValueParam || ""
  );
  const [menuIsOpenState, setMenuIsOpen] = useStateParam(
    !!defaultMenuIsOpenParam
  );

  const inputValue: string =
    typeof inputValueParam === "string" ? inputValueParam : inputValueState;

  const menuIsOpen: boolean =
    typeof menuIsOpenParam === "boolean" ? menuIsOpenParam : menuIsOpenState;

  const onInputChange = useCallbackParam(
    (nextInputValue: string, actionMeta: InputActionMeta): void => {
      if (onInputChangeParam) {
        onInputChangeParam(nextInputValue, actionMeta);
      }

      setInputValue(nextInputValue);
    },
    [onInputChangeParam]
  );

  const onMenuClose = useCallbackParam((): void => {
    if (onMenuCloseParam) {
      onMenuCloseParam();
    }

    setMenuIsOpen(false);
  }, [onMenuCloseParam]);

  const onMenuOpen = useCallbackParam((): void => {
    if (onMenuOpenParam) {
      onMenuOpenParam();
    }

    setMenuIsOpen(true);
  }, [onMenuOpenParam]);

  const baseResult: UseAsyncPaginateBaseResult<OptionType, Group> =
    useAsyncPaginateBaseParam(
      {
        ...params,
        inputValue,
        menuIsOpen,
      },
      deps
    );

  return {
    ...baseResult,
    inputValue,
    menuIsOpen,
    onInputChange,
    onMenuClose,
    onMenuOpen,
  };
};

export const useAsyncPaginate = <
  OptionType,
  Group extends GroupBase<OptionType>,
  Additional
>(
  params: UseAsyncPaginateParams<OptionType, Group, Additional>,
  deps: ReadonlyArray<any> = []
): UseAsyncPaginateResult<OptionType, Group> =>
  useAsyncPaginatePure<OptionType, Group, Additional>(
    useState,
    useCallback,
    useAsyncPaginateBase,
    params,
    deps
  );
