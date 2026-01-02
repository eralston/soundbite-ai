import React, {
  Context,
  CSSProperties,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
} from "react";
import InfiniteLoader from "react-window-infinite-loader";
import {
  FixedSizeList,
  FixedSizeListProps,
  VariableSizeList,
  VariableSizeListProps,
} from "react-window";
import AutoSizer from "react-virtualized-auto-sizer";

import { ShowWhen } from "./ShowWhen";
import { useWindowSize } from "../../modules/useWindowSize";

// InfiniteTable uses react-window to create a dynamically loading list of items using react-window-infinite-loader
// AutoSizer from react-virtualized-auto-sizer provides the ability to expand the list to take up its outer container
// VariableSizeList from react-window provides support for list items of varying height
// Variable height row mode requires monitoring the width of the window to dynamically evaluate height on window resize

/** A function that returns a variable height list item */
export type InfiniteListItemGetter = VariableSizeListProps["children"];

/** Properties for the table context that transmits state between components */
export interface IInfiniteListContext {
  top: number;
  setTop: (top: number) => void;
  header: React.ReactNode;
  footer: React.ReactNode;
  setItemHeight: (index: number, size: number) => void;
  windowWidth: number;
}

export type InfiniteListContext = Context<IInfiniteListContext>;

/** Props for InfiniteListItem */
interface IInfiniteListItemProps {
  className?: string;
  children: React.ReactNode;
  index: number;
  context: InfiniteListContext;
  style: CSSProperties;
}

/**
 * Wrapper element for an InfiniteListItem that will dynamically evaluate its size on load and window width change
 * Place your list item contents into this wrapper when loading items into the
 * @param props
 * @returns
 */
export const InfiniteListItem: React.FC<IInfiniteListItemProps> = (
  props: IInfiniteListItemProps
) => {
  const { className, children, index, context, style } = props;
  const { setItemHeight, windowWidth } = useContext(context);
  const root = useRef<HTMLTableRowElement>(null);

  const setHeight = () => {
    if (root.current) {
      const height = root.current.getBoundingClientRect().height;
      console.log(`Set Height of list item ${index} as ${height}`);
      setItemHeight(index, height);
    } else {
      console.log(
        `Tried to set height of list item ${index} but no root element`
      );
    }
  };

  useEffect(() => {
    setHeight();
  }, [index, windowWidth, setItemHeight]);

  useEffect(() => {
    setHeight();
  }, []);

  return (
    <tr data-row-index={index} ref={root} className={className} style={style}>
      {children}
    </tr>
  );
};

/** React props for InfiniteTable such that it can dynamically call out for items and rows to render it in fixed or variable height */
interface IInfiniteListProps {
  className?: string;
  count?: number;
  loadMore: (startIndex: number, stopIndex: number) => void | Promise<void>;
  isItemLoaded: (index: number) => boolean;
  minimumBatchSize?: number;
  header?: (context: InfiniteListContext) => React.ReactNode;
  footer?: (context: InfiniteListContext) => React.ReactNode;
  item: (context: InfiniteListContext) => InfiniteListItemGetter;
}

/**
 * Show a dynamically loading list that will take up 100% width and 100% height in its container with variable height rows
 * @param props
 */
export const InfiniteList: React.FC<IInfiniteListProps> = (
  props: IInfiniteListProps
) => {
  const rowHeightMap = useRef<{ [index: number]: number }>({});
  //const itemHeightMap = useRef<Map<number, number>>(new Map<number, number>());
  const setItemHeight = useCallback((index: number, size: number) => {
    rowHeightMap.current = { ...rowHeightMap.current, [index]: size };
    //itemHeightMap.current.set(index, size);
  }, []);
  const [windowWidth] = useWindowSize();
  /** Context for cross component communication */
  const ListContext: Context<IInfiniteListContext> =
    React.createContext<IInfiniteListContext>({
      top: 0,
      setTop: (value: number) => {},
      header: <></>,
      footer: <></>,
      setItemHeight,
      windowWidth: 0,
    });

  /**
   *  Read from the row height map; otherwise, call out to get the size
   * @param index
   * @returns
   */
  const getItemHeight = React.useCallback(
    (index) => rowHeightMap.current[index] ?? 30,
    []
  );

  /**
   * The Inner component of the virtual list. This is the "Magic".
   * Capture what would have been the top elements position and apply it to the table.
   * Other than that, render an optional header and footer.
   **/
  const List = React.forwardRef<
    HTMLDivElement,
    React.HTMLProps<HTMLDivElement>
  >(function Inner({ children, ...rest }, ref) {
    const { header, footer, top } = useContext(ListContext);
    return (
      <div {...rest} ref={ref}>
        <table
          className={props.className}
          style={{ top, position: "absolute", width: "100%" }}
        >
          {header}
          <tbody>{children}</tbody>
          {footer}
        </table>
      </div>
    );
  });

  function WindowWithContext({
    header,
    footer,
    ...rest
  }: {
    header?: React.ReactNode;
    footer?: React.ReactNode;
  } & Omit<FixedSizeListProps, "children" | "innerElementType" | "itemSize">) {
    const fixedListRef = useRef<FixedSizeList | null>();
    const variableListRef = useRef<VariableSizeList | null>();
    const [top, setTop] = useState(0);

    const rowGetter = props.item(ListContext);

    return (
      <ListContext.Provider
        value={{ top, setTop, header, footer, setItemHeight, windowWidth }}
      >
        <VariableSizeList
          {...rest}
          innerElementType={List}
          onItemsRendered={(props) => {
            const style =
              fixedListRef.current &&
              // @ts-ignore private method access
              fixedListRef.current._getItemStyle(props.overscanStartIndex);
            setTop((style && style.top) || 0);

            // Call the original callback
            rest.onItemsRendered && rest.onItemsRendered(props);
          }}
          ref={(el) => (variableListRef.current = el)}
          itemSize={getItemHeight}
        >
          {rowGetter}
        </VariableSizeList>
      </ListContext.Provider>
    );
  }

  const Body: React.FC = () => {
    return (
      <div className="sb-infinite-table">
        <AutoSizer>
          {({ height, width }: { height: number; width: number }) => (
            <InfiniteLoader
              isItemLoaded={props.isItemLoaded}
              itemCount={props.count ?? 0}
              loadMoreItems={props.loadMore}
              minimumBatchSize={props.minimumBatchSize}
            >
              {({ onItemsRendered, ref }) => (
                <WindowWithContext
                  footer={props.footer}
                  header={props.header}
                  height={height}
                  itemCount={props.count ?? 0}
                  onItemsRendered={onItemsRendered}
                  width={width}
                />
              )}
            </InfiniteLoader>
          )}
        </AutoSizer>
      </div>
    );
  };

  return (
    <ShowWhen is={props.count != null}>
      <Body />
    </ShowWhen>
  );
};
