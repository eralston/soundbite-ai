/** @jsx jsx */
import { jsx, css, SerializedStyles } from "@emotion/react";
import React from "react";
import { GlobalTheme, ShowWhen } from "../..";

let styles: SerializedStyles;
const getStyles = () => {
  if (styles == null) {
    styles = css`
      width: 100%;
      height: auto;
      min-height: 8px;
      background-color: ${GlobalTheme.current.colors.neutrals.n600};

      .progress-bar {
        height: auto;
        overflow: visible;
      }

      .sb-progress-bar-title {
        text-align: left;
        padding: 1rem;
      }
    `;
  }

  return styles;
};

interface IProps {
  progress: number;
  title?: string;
}

/**
 * A progress bar with optional title
 * @param props
 */
export const SbProgress: React.FC<IProps> = (props: IProps) => {
  const width = { width: `${props.progress}%` };

  return (
    <div className="progress sb-progress" css={getStyles()}>
      <div
        className="progress-bar progress-bar-animated sb-progress-background sb-progress-background-go"
        role="progressbar"
        style={width}
        aria-valuenow={props.progress}
      >
        <ShowWhen is={props.title != null}>
          <div className="sb-progress-bar-title">{props.title}</div>
        </ShowWhen>
      </div>
    </div>
  );
};
