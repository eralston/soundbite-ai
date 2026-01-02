import React, { useContext, useRef, useState } from "react";
import InfiniteLoader from "react-window-infinite-loader";
import { FixedSizeList, FixedSizeListProps } from "react-window";
import AutoSizer from "react-virtualized-auto-sizer";

import { ShowWhen } from "./ShowWhen";

interface IProps {
  className?: string;
  itemSize: number;
  count?: number;
  loadMore: (startIndex: number, stopIndex: number) => void | Promise<void>;
  isItemLoaded: (index: number) => boolean;
  minimumBatchSize?: number;
  header?: React.ReactNode;
  footer?: React.ReactNode;
  row: FixedSizeListProps["children"];
}

/**
 * Show a dynamically loading table that will take up 100% width and 100% height in its container
 * @param props
 */
export const InfiniteTable: React.FC<IProps> = (props: IProps) => {
  /** Context for cross component communication */
  const TableContext = React.createContext<{
    top: number;
    setTop: (top: number) => void;
    header: React.ReactNode;
    footer: React.ReactNode;
  }>({
    top: 0,
    setTop: (value: number) => {},
    header: <></>,
    footer: <></>,
  });

  /**
   * The Inner component of the virtual list. This is the "Magic".
   * Capture what would have been the top elements position and apply it to the table.
   * Other than that, render an optional header and footer.
   **/
  const Table = React.forwardRef<
    HTMLDivElement,
    React.HTMLProps<HTMLDivElement>
  >(function Inner({ children, ...rest }, ref) {
    const className = props.className ?? "table align-items-center";
    const { header, footer, top } = useContext(TableContext);
    return (
      <div {...rest} ref={ref}>
        <table
          className={className}
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
    row,
    header,
    footer,
    ...rest
  }: {
    header?: React.ReactNode;
    footer?: React.ReactNode;
    row: FixedSizeListProps["children"];
  } & Omit<FixedSizeListProps, "children" | "innerElementType">) {
    const listRef = useRef<FixedSizeList | null>();
    const [top, setTop] = useState(0);

    return (
      <TableContext.Provider value={{ top, setTop, header, footer }}>
        <FixedSizeList
          {...rest}
          innerElementType={Table}
          onItemsRendered={(props) => {
            const style =
              listRef.current &&
              // @ts-ignore private method access
              listRef.current._getItemStyle(props.overscanStartIndex);
            setTop((style && style.top) || 0);

            // Call the original callback
            rest.onItemsRendered && rest.onItemsRendered(props);
          }}
          ref={(el) => (listRef.current = el)}
        >
          {row}
        </FixedSizeList>
      </TableContext.Provider>
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
                  itemSize={props.itemSize}
                  onItemsRendered={onItemsRendered}
                  row={props.row}
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
