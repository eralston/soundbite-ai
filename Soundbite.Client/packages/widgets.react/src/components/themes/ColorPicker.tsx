/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React from "react";
import { ColorResult, SketchPicker } from "react-color";
import { ShowWhen } from "../controls";
import tinycolor from "tinycolor2";

const baseStyles = css`
  position: relative;

  .sb-color-picker-popover {
    bottom: 2rem;
    position: absolute;
    z-index: 2;
  }

  .sb-color-swatch {
    border: 1px solid white;
    border-radius: 0.675rem;
    cursor: pointer;
    display: inline-block;
    min-height: 2rem;
    min-width: 2rem;
    padding: 2px 0.5rem;
    text-align: center;
  }

  .sb-cover {
    bottom: 0;
    left: 0;
    position: fixed;
    right: 0;
    top: 0;
    z-index: 1;
  }

  .sb-color-swatch.active {
    outline: 5px auto -webkit-focus-ring-color;
  }
`;

interface IProps {
  title?: string;
  defaultColor?: string;
  onChange: (colorHex: string) => void;
  className?: string;
}

interface IState {
  color: string;
  isOpen: boolean;
}

// Based on example from https://casesandberg.github.io/react-color/
export class ColorPicker extends React.Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);
    this.state = {
      color: props.defaultColor ?? "#fff",
      isOpen: false,
    };
  }

  onChangeComplete = (
    color: ColorResult /*, event: React.ChangeEvent<HTMLInputElement>*/
  ) => {
    this.setState({ color: color.hex });
    this.props.onChange(color.hex);
  };

  render() {
    const isLight = tinycolor(this.state.color).isLight();
    const offsetColor = isLight ? "black" : "white";
    const swatchClasses =
      "sb-color-swatch mb-1 mr-1" + (this.state.isOpen ? " active" : "");

    return (
      <span css={baseStyles} className={this.props.className}>
        <span
          className={swatchClasses}
          style={{
            backgroundColor: this.state.color,
            borderColor: offsetColor,
          }}
          onClick={() => {
            this.setState({ isOpen: true });
          }}
        >
          <ShowWhen is={this.props.title != null}>
            <span style={{ color: offsetColor }}>{this.props.title}</span>
          </ShowWhen>
        </span>
        <ShowWhen is={this.state.isOpen}>
          <div
            className="sb-cover"
            onClick={() => {
              this.setState({ isOpen: false });
            }}
          ></div>
          <div className="sb-color-picker-popover">
            <SketchPicker
              color={this.state.color}
              onChangeComplete={this.onChangeComplete}
            />
          </div>
        </ShowWhen>
      </span>
    );
  }
}
