/** @jsx jsx */
import { jsx, css, SerializedStyles } from "@emotion/react";
import React, { Component } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import {
  faPlay,
  faPause,
  faStop,
  faMicrophone,
} from "@fortawesome/free-solid-svg-icons";

import ProgressButton from "react-progress-button";
import { GlobalTheme } from "../..";

const getBaseStyles = () => {
  const styles = css`
    .pb-button span {
      width: 100%;
      height: 100%;
    }

    .pb-button .svg-inline--fa {
      height: 1rem;
      width: 0.875em;
      overflow: visible;
    }

    .pb-container .pb-button svg.svg-inline--fa path {
      opacity: 1;
      fill: ${GlobalTheme.current.colors.bootstrap.primary};
    }

    .pb-container .pb-button:hover svg.svg-inline--fa path {
      fill: ${GlobalTheme.current.colors.neutrals.min};
    }

    .pb-container .pb-button svg.svg-inline--fa {
      height: 20px;
      width: 20px;
      left: 10px;
      top: 15px;
    }

    .pb-container {
      display: inline-block;
      text-align: center;
      width: 100%;
      height: 60px;
    }

    .pb-container .pb-button {
      background: ${GlobalTheme.current.colors.neutrals.midground};
      border: 1px solid ${GlobalTheme.current.colors.bootstrap.primary};
      border-radius: 0.25rem;
      color: currentColor;
      cursor: pointer;
      padding: 0.7em 1em;
      text-decoration: none;
      text-align: center;
      height: 40px;
      width: 40px;
      -webkit-tap-highlight-color: transparent;
      outline: none;
      transition: background-color 0.3s, width 0.3s, border-width 0.3s,
        border-color 0.3s, border-radius 0.3s;
      position: relative;
    }

    .pb-container .pb-button:hover,
    .pb-container.loading .pb-button:hover {
      background-color: ${GlobalTheme.current.colors.bootstrap.primaryHigh};
      border-color: ${GlobalTheme.current.colors.bootstrap.primaryHigh};
      color: white;
    }

    .pb-container .pb-button span {
      transition: all 0.15s;
      display: inherit;
      transition: opacity 0.3s 0.1s;
      font-size: 1.4em;
      font-weight: 100;
      position: absolute;
      top: 3px;
      left: calc(50% - 9px);
    }

    .pb-container .pb-button span i {
      width: 19px;
    }

    .pb-container .pb-button svg {
      height: 40px;
      width: 40px;
      position: absolute;
      transform: translate(-50%, -50%);
      pointer-events: none;
    }

    .pb-container .pb-button svg path {
      opacity: 0;
      fill: none;
    }

    .pb-container .pb-button svg.pb-progress-circle {
      animation: spin 0.9s infinite cubic-bezier(0.085, 0.26, 0.935, 0.71);
    }

    .pb-container .pb-button svg.pb-progress-circle path {
      stroke: currentColor;
      stroke-width: 5;
    }

    .pb-container .pb-button svg.pb-checkmark path,
    .pb-container .pb-button svg.pb-cross path {
      stroke: #fff;
      stroke-linecap: round;
      stroke-width: 4;
    }

    .pb-container.disabled .pb-button {
      cursor: not-allowed;
    }

    .pb-container.loading .pb-button {
      background-color: ${GlobalTheme.current.colors.neutrals.midground};
      padding: 0;
      border-radius: 50%;
    }

    .pb-container.loading .pb-button span {
      transition: all 0.15s;
    }

    .pb-container.loading .pb-button .pb-progress-circle > path {
      transition: opacity 0.15s 0.3s;
      opacity: 1;
    }

    .pb-container.success .pb-button span {
      transition: all 0.15s;
      opacity: 0;
      display: none;
    }

    .pb-container.success .pb-button .pb-checkmark > path {
      opacity: 1;
    }

    .pb-container.error .pb-button span {
      transition: all 0.15s;
      opacity: 0;
      display: none;
    }

    .pb-container.error .pb-button .pb-cross > path {
      opacity: 1;
    }

    @keyframes spin {
      from {
        transform: translate(-50%, -50%) rotate(0deg);
        transform-origin: center center;
      }

      to {
        transform: translate(-50%, -50%) rotate(360deg);
        transform-origin: center center;
      }
    }
  `;
  return styles;
};

let baseStyles: SerializedStyles | undefined = undefined;

interface IProps {
  onClick?: () => void;
  isRunning?: boolean;
  allowClickOnLoading?: boolean;
  disabled?: boolean;
  isRecordMode?: boolean;
  title?: string;
}

/** A button that shows active progress when pressed */
export class SbProgressButton extends Component<IProps> {
  render() {
    let buttonState = this.props.isRunning ? "loading" : "";
    if (this.props.disabled) buttonState = "disabled";

    let icon = this.props.isRecordMode
      ? this.props.isRunning
        ? faStop
        : faMicrophone
      : this.props.isRunning
      ? faPause
      : faPlay;

    if (!baseStyles) baseStyles = getBaseStyles();

    return (
      <div className="sb-pb-button" css={baseStyles}>
        <ProgressButton
          onClick={this.props.onClick}
          state={buttonState}
          shouldAllowClickOnLoading={this.props.allowClickOnLoading}
          title={this.props.title}
        >
          <FontAwesomeIcon icon={icon} size="xs" />
        </ProgressButton>
      </div>
    );
  }
}
