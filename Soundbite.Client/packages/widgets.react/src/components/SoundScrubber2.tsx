/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React, { Component, useEffect, useState } from "react";
import { Scrubber } from "react-scrubber";
import { GlobalTheme } from "../styles";
import { Loader } from "./controls";
import { observer } from "mobx-react-lite";
import { IMediaPlayerContext } from "./video/context/IMediaPlayerContext";

/***************************************************************************************************
 *  Component Styles
 **************************************************************************************************/
function getStyles() {
  const styles = css`
    width: 100%;
    height: 1.5rem;
    padding-left: 0.75rem;
    padding-right: 0.75rem;
    background-color: ${GlobalTheme.current.colors.neutrals.n500};
    border-radius: 4px;
    overflow: hidden;
    position: relative;

    .sb-time-summary {
      color: ${GlobalTheme.current.colors.neutrals.n200};
      right: 0.5rem;
      pointer-events: none;
      position: absolute;
      top: 1px;
      z-index: 1;
    }

    &[aria-disabled="true"] {
      pointer-events: none;
    }

    &[aria-disabled="true"] .sb-time-summary {
      color: ${GlobalTheme.current.colors.neutrals.n300};
    }

    .scrubber.horizontal .bar {
      height: 100%;
    }

    .scrubber.horizontal .bar .bar__thumb {
      height: 1.5rem;
      width: 1.5rem;
      transform: translate(-50%, -50%);
      top: 50%;
    }

    .scrubber {
      width: 100%;
      height: 100%;
      position: relative;
      -webkit-user-select: none;
      user-select: none;
      touch-action: none;
    }

    .scrubber .bar {
      background-color: transparent;
      width: 100;
      position: relative;
      transition: height 0.2s linear, width 0.2s linear;
    }

    .scrubber * {
      -webkit-user-select: none;
      user-select: none;
    }

    .scrubber .bar__progress {
      position: absolute;
      background-color: ${GlobalTheme.current.colors.neutrals.n600};
      border-top-left-radius: 4px;
      border-bottom-left-radius: 4px;
      left: -0.75rem;
    }

    .scrubber .bar__buffer {
      position: absolute;
    }

    .scrubber .bar__thumb {
      position: absolute;
      width: 0px;
      height: 0px;
      border-radius: 4px;
      transition: height 0.2s linear, width 0.2s linear;
      background-color: ${GlobalTheme.current.colors.bootstrap.primary};
    }

    &[aria-disabled="true"] .scrubber .bar__thumb {
      background-color: ${GlobalTheme.current.colors.neutrals.n700};
    }

    .scrubber .bar__marker {
      position: absolute;
    }

    .scrubber.horizontal .bar__progress,
    .scrubber.horizontal .bar__marker,
    .scrubber.horizontal .bar__buffer {
      height: 100%;
    }

    .scrubber.horizontal .bar__progress::after {
      position: absolute;
      background-color: ${GlobalTheme.current.colors.neutrals.n600};
      right: -4px;
      width: 5px;
      height: 100%;
      content: " ";
    }

    .scrubber.horizontal .bar__marker {
      width: 12px;
    }

    .scrubber.horizontal .bar {
      top: 50%;
      left: 0;
      transform: translateY(-50%);
      width: 100%;
    }

    .scrubber.vertical .bar__progress,
    .scrubber.vertical .bar__marker,
    .scrubber.vertical .bar__buffer {
      width: 100%;
      bottom: 0;
    }

    .scrubber.vertical .bar {
      top: 0;
      left: 50%;
      transform: translateX(-50%);
      width: 100%;
      height: 100%;
    }

    .scrubber.vertical .bar__thumb {
      transform: translate(-50%, 50%);
      left: 50%;
    }

    .scrubber.hover.vertical .bar {
      width: 6px;
    }

    .scrubber.vertical .bar__marker {
      height: 100%;
    }
  `;

  return styles;
}

/***************************************************************************************************
 *  Component Properties Interface
 **************************************************************************************************/
interface IProps {
  disabled?: boolean;
  hideTime?: boolean;
  context: IMediaPlayerContext;
}

/***************************************************************************************************
 *  Component
 **************************************************************************************************/

export const Scrubber2: React.FC<IProps> = observer((props: IProps) => {
  //////////[ State ]///////////////////////////////////////////////////////////////////////////////

  const [context, setContext] = useState<IMediaPlayerContext>(props.context);

  useEffect(() => {
    if (context != props.context) {
      setContext(props.context);
    }
  }, [props.context]);

  //////////[ Build Component UI ]//////////////////////////////////////////////////////////////////

  const styles = getStyles();
  const scrubberClassName =
    "sb-sound-scrubber" +
    (props.disabled === true ? " sb-sound-scrubber-disabled" : "");

  return (
    <Loader
      isLoadedWhen={context?.duration > 0 || context?.hasError}
      isSmall={true}
    >
      <div
        css={styles}
        className={scrubberClassName}
        aria-disabled={props.disabled === true}
      >
        <span className="sb-time-summary">{context.timeSummary}</span>
        <Scrubber
          min={0}
          max={context.duration}
          value={context.time}
          onScrubStart={(time: number) => context.jumpTo(time)}
          onScrubEnd={(time: number) => context.jumpTo(time)}
          onScrubChange={(time: number) => context.jumpTo(time)}
        />
      </div>
    </Loader>
  );
});
