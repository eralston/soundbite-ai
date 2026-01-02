// This file is directly imported from the react-select-async-paginate project due to React version conflicts
// https://github.com/vtaits/react-select-async-paginate
// At some point, we should catch up to React 17, then it should be possible to replace these files with the real npm package again

import { useEffect, useRef, useCallback } from "react";
import { ComponentType, FC, Ref } from "react";
import composeRefs from "@seznam/compose-react-refs";

import { ShouldLoadMore } from "./types";
import React from "react";

export const CHECK_TIMEOUT = 300;

export type Props = {
  selectProps: {
    handleScrolledToBottom?: () => void;
    shouldLoadMore: ShouldLoadMore;
  };

  innerRef: Ref<HTMLElement>;

  useEffect?: typeof useEffect;
  useRef?: typeof useRef;
  useCallback?: typeof useCallback;
  setTimeout?: typeof setTimeout;
  clearTimeout?: typeof clearTimeout;

  [key: string]: any;
};

type ComponentProps = {
  innerRef: Ref<HTMLElement>;
};

// eslint-disable-next-line @typescript-eslint/naming-convention
export const wrapMenuList = (
  MenuList: ComponentType<ComponentProps>
): FC<Props> => {
  function WrappedMenuList(props: Props) {
    const {
      selectProps: { handleScrolledToBottom, shouldLoadMore },
      innerRef,

      useEffect: useEffectProp,
      useRef: useRefProp,
      useCallback: useCallbackProp,

      setTimeout: setTimeoutProp,
      clearTimeout: clearTimeoutProp,
    } = props;

    // WARNING: Type inconsistency in the library
    if (useRefProp == null) {
      throw new Error("useRefProp cannot be null");
    }

    // WARNING: Type inconsistency in the library
    if (useCallbackProp == null) {
      throw new Error("useCallbackProp cannot be null");
    }

    // WARNING: Type inconsistency in the library
    const checkTimeoutRef = useRefProp<NodeJS.Timeout>(
      null as unknown as NodeJS.Timeout
    );
    const menuListRef = useRefProp<HTMLElement>(null);

    const shouldHandle = useCallbackProp(() => {
      const el = menuListRef.current;

      // menu not rendered
      if (!el) {
        return false;
      }

      const { scrollTop, scrollHeight, clientHeight } = el;

      return shouldLoadMore(scrollHeight, clientHeight, scrollTop);
    }, [shouldLoadMore]);

    const checkAndHandle = useCallbackProp(() => {
      if (shouldHandle()) {
        if (handleScrolledToBottom) {
          handleScrolledToBottom();
        }
      }
    }, [shouldHandle, handleScrolledToBottom]);

    // WARNING: Type inconsistency in the library
    if (setTimeoutProp == null) {
      throw new Error("setTimeoutProp cannot be null");
    }

    const setCheckAndHandleTimeout = useCallbackProp(() => {
      checkAndHandle();

      checkTimeoutRef.current = setTimeoutProp(
        setCheckAndHandleTimeout,
        CHECK_TIMEOUT
      );
    }, [checkAndHandle]);

    // WARNING: Type inconsistency in the library
    if (useEffectProp == null) {
      throw new Error("useEffectProp cannot be null");
    }

    // WARNING: Type inconsistency in the library
    if (clearTimeoutProp == null) {
      throw new Error("clearTimeoutProp cannot be null");
    }

    useEffectProp(() => {
      setCheckAndHandleTimeout();

      return (): void => {
        if (checkTimeoutRef.current) {
          clearTimeoutProp(checkTimeoutRef.current);
        }
      };
    }, []);

    return (
      <MenuList
        {...props}
        innerRef={composeRefs<HTMLElement>(innerRef, menuListRef)}
      />
    );
  }

  WrappedMenuList.defaultProps = {
    useEffect,
    useRef,
    useCallback,
    setTimeout,
    clearTimeout,
  };

  return WrappedMenuList;
};
