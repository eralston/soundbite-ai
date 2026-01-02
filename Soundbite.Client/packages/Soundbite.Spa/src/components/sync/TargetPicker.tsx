import Select, { OptionTypeBase } from "react-select";
import { observer } from "mobx-react-lite";
import React, { useState } from "react";

import {
  TokenPageRequest,
  TokenPageResponse,
  SyncTarget,
} from "@soundbite/api";
import {
  ILoadOptionsRequest,
  ILoadOptionsResponse,
  IOption,
  shouldLoadMorePickerPages,
  withAsyncPaginate,
} from "@soundbite/widgets-react";

const PaginatingSelect = withAsyncPaginate(Select);
interface IProps {
  title: string;
  onLoad: (
    request?: TokenPageRequest
  ) => Promise<TokenPageResponse<SyncTarget>>;
  selected?: SyncTarget[];
  onChange: (entities: SyncTarget[]) => void;
}

// STATIC

const targetsToOptions = (targets?: SyncTarget[]): IOption[] => {
  if (targets == null) {
    return [];
  }
  return targets.map((target) => {
    const ret: IOption = {
      value: target.id,
      label: target.name ?? "Loading...",
    };
    return ret;
  });
};

const optionsToTargets = (options?: OptionTypeBase[]): SyncTarget[] => {
  if (options == null) return [];

  const entities: SyncTarget[] = options.map((o): SyncTarget => {
    return { id: o.value, name: o.label };
  });

  return entities;
};

/** Takes a list of SyncEntities and manages a picker */
export const TargetPicker: React.FC<IProps> = observer((props: IProps) => {
  const [value, setValue] = useState<any>(targetsToOptions(props.selected));
  const [skipToken, setSkipToken] = useState<string | undefined>(undefined);
  const [lastFilter, setLastFiler] = useState<string | undefined>(undefined);

  const loadOptions = async (
    filter: string,
    prevOptions?: ILoadOptionsRequest
  ): Promise<ILoadOptionsResponse> => {
    // TODO: Caching?
    const isSameFilter = filter === lastFilter;

    if (!isSameFilter) {
      setLastFiler(filter);
    }

    const page = await props.onLoad({
      filter,
      skipToken: isSameFilter ? skipToken : undefined,
    });

    setSkipToken(page.skipToken);

    const hasMore = page.skipToken != null;
    const options = targetsToOptions(page.result);
    return {
      options,
      hasMore,
    };
  };

  /**
   * Called when the picker changes options, calling out to the onChange handler passed by props
   * @param selected IEntityOption[]
   * @param meta
   */
  const onChange = (selected?: any, meta?: any) => {
    setValue(selected);
    props.onChange(optionsToTargets(selected));
  };

  return (
    <div className="sb-aad-picker">
      <PaginatingSelect
        className="sb-picker"
        classNamePrefix="sb-picker"
        debounceTimeout={300}
        isMulti
        loadOptions={loadOptions}
        onChange={onChange}
        placeholder={`Select ${props.title}...`}
        shouldLoadMore={shouldLoadMorePickerPages}
        value={value}
      />
    </div>
  );
});
