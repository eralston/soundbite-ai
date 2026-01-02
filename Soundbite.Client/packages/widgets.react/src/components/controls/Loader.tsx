import React, { CSSProperties, useEffect, useState } from "react";
import { Spinner } from "reactstrap";

import { ShowWhen } from "./ShowWhen";

interface IProps {
  isLoadedWhen?: boolean;
  children?: React.ReactNode;
  style?: CSSProperties;
  className?: string;
  isDisplayBased?: boolean;
  isSmall?: boolean;

  /** Message displayed while loading */
  message?: string;

  /** Amount of time in seconds to wait before displaying the props.message text over the spinner; default zero */
  messageDelay?: number;
}

/**
 * Wraps an element with a Bootstrap spinner, which shows the contents when isLoadedWhen === true
 * @param props
 */
export const Loader = (props: IProps) => {
  let show = true;
  if (props.isLoadedWhen) show = false;

  const isMessageInstant =
    !props.isLoadedWhen && props.message != null && props.messageDelay == null;
  const [isShowingMessage, setShowingMessage] = useState(isMessageInstant);
  const [timeoutHandle, setTimeoutHandle] = useState<
    NodeJS.Timeout | undefined
  >(undefined);

  const isMessageEnabled = !!props.message || !!props.messageDelay;

  const showMessage = () => {
    setShowingMessage(!props.isLoadedWhen);
    clearTimer();
  };

  const clearTimer = () => {
    // if we're loaded now, but still counting time, then stop
    if (timeoutHandle != null) {
      clearTimeout(timeoutHandle);
      setTimeoutHandle(undefined);
    }
  };

  const startTimer = () => {
    if (timeoutHandle == null) {
      const delayTimeoutMillis =
        (props.messageDelay !== undefined ? props.messageDelay : 2) * 1000;
      const handle = setTimeout(showMessage, delayTimeoutMillis);
      setTimeoutHandle(handle);
    }
  };

  const checkMessage = () => {
    // If we're loaded, then we don't need to delay
    if (props.isLoadedWhen || !isMessageEnabled) return;

    if (isMessageInstant) {
      showMessage();
    } else {
      // If we're not loaded yet and we're not counting time, then start
      if (props.messageDelay && timeoutHandle == null) {
        startTimer();
      }
    }
  };

  const clearMessage = () => {
    setShowingMessage(false);
    clearTimer();
  };

  useEffect(() => {
    if (props.isLoadedWhen) {
      clearMessage();
    } else {
      checkMessage();
    }
  }, [props.message, props.messageDelay, props.isLoadedWhen]);

  const defaultStyles: CSSProperties = {
    minHeight: "4em",
  };

  const smallClassName = props.isSmall ? "spinner-grow-sm " : "";

  return (
    <React.Fragment>
      <ShowWhen is={show}>
        <div className="w-100 d-table" style={props.style ?? defaultStyles}>
          <div className="text-center d-table-cell align-middle sb-loader">
            <ShowWhen is={isShowingMessage}>
              <div className="text-muted mb-3">
                <small>{props.message ?? "Tuning Up The Orchestra..."}</small>
              </div>
            </ShowWhen>
            <Spinner
              type="grow"
              className={
                smallClassName + (props.className ?? "sb-spinner-default")
              }
            />
          </div>
        </div>
      </ShowWhen>
      <ShowWhen is={!show} isDisplayBased={props.isDisplayBased}>
        {props.children}
      </ShowWhen>
    </React.Fragment>
  );
};
