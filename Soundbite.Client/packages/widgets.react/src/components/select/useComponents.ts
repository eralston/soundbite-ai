// This file is directly imported from the react-select-async-paginate project due to React version conflicts
// https://github.com/vtaits/react-select-async-paginate
// At some point, we should catch up to React 17, then it should be possible to replace these files with the real npm package again

import { useMemo } from "react";
import { OptionTypeBase, Props as SelectProps } from "react-select";

import { components as defaultComponents } from "react-select";

import { wrapMenuList } from "./wrapMenuList";

export const MenuList = wrapMenuList(defaultComponents.MenuList as any);

type SelectComponentsConfig<
  OptionType extends OptionTypeBase,
  IsMulti extends boolean
> = Partial<SelectProps<OptionType, IsMulti>["components"]>;

export const useComponentsPure = <
  OptionType extends OptionTypeBase,
  IsMulti extends boolean
>(
  useMemoParam: typeof useMemo,
  components: SelectComponentsConfig<OptionType, IsMulti>
): SelectComponentsConfig<OptionType, IsMulti> =>
  useMemoParam(
    () =>
      ({
        MenuList,
        ...components,
      } as SelectComponentsConfig<OptionType, IsMulti>),
    [components]
  );

export const useComponents = <
  OptionType extends OptionTypeBase,
  IsMulti extends boolean
>(
  components: SelectComponentsConfig<OptionType, IsMulti>
): SelectComponentsConfig<OptionType, IsMulti> =>
  useComponentsPure(useMemo, components);
