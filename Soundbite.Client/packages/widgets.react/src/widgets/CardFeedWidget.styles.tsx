/** @jsx jsx */
import { jsx, css } from "@emotion/react";

import { GlobalTheme } from "../styles";

export function getStyles() {
  const styles = css`
    display: flex;
    justify-content: center;
    align-items: center;

    .sb-feed-contents {
      width: 100%;
      max-width: 470px;
    }

    .sb-content-card {
      width: 100%;
      overflow: hidden;

      h1 {
        font-weight: 100;
        font-size: 2.5rem;
        max-height: 5em;
        line-height: 1.5em;
        overflow: hidden;
        color: ${GlobalTheme.current.colors.neutrals.n800};
      }
    }

    .sb-author-summary {
      flex: 1;
      flex-shrink: 2;
      max-width: calc(100% - 72px);
    }

    .sb-content-actions {
      flex: 0 0 auto;
      flex-shrink: 0;
      min-width: 72px;

      &:empty {
        display: none;
      }
    }

    .truncate-text-2-lines {
      position: relative;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
      text-overflow: ellipsis;
      line-height: 1.4em; /* Adjust this value according to your design */
      max-height: 2.8em; /* (number of lines) * (line-height value) */
    }

    .truncate-text-2-lines::after {
      content: "";
      position: absolute;
      right: 0;
      bottom: 0;
      width: 4em; /* Adjust this value according to your design */
      height: 1.4em; /* Should be the same as the line-height value */
      background: linear-gradient(
        to right,
        ${GlobalTheme.current.colors.neutrals.min}00,
        ${GlobalTheme.current.colors.neutrals.min}FF 75%
      ); /* Replace the rgba() values with your background color */
      pointer-events: none;
    }

    .sb-btn-overlay {
      position: absolute;
      right: 1rem;
      top: 2.25rem;
      z-index: 1;
      background: linear-gradient(
        to right,
        ${GlobalTheme.current.colors.neutrals.min}00,
        ${GlobalTheme.current.colors.neutrals.min}FF 75%
      );
      padding-left: 3rem;

      .sb-play-btn,
      .sb-record-btn {
        border-radius: 50%;
        font-size: 28px;
        margin-right: 0;
        padding: 16px;

        svg {
          margin: 0;
        }

        .btn-inner--text {
          display: none;
        }
      }
    }

    .sb-card-bottom {
      max-width: 100%;
    }
  `;

  return styles;
}
