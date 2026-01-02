/** @jsx jsx */
import { jsx, css } from "@emotion/react";
import React, { Component, useEffect } from "react";
import tinycolor from "tinycolor2";

import { GlobalTheme } from "../../styles";
import { ThemeProcessor } from "../../styles/ThemeProcessor";
import { ColorPicker } from "./ColorPicker";

const baseStyles = css``;

function setColor(colorType: string, colorName: string, colorVal: string) {
  (GlobalTheme.current.colors as any)[colorType][colorName] = colorVal;
}

export const ColorConfig: React.FC = () => {
  function colorPicker(
    colorType: string,
    colorName: string,
    title?: string,
    onChange?: (colorVal: string) => void
  ) {
    return (
      <ColorPicker
        defaultColor={(GlobalTheme.current.colors as any)[colorType][colorName]}
        onChange={(colorVal: string) => {
          setColor(colorType, colorName, colorVal);
          if (onChange) {
            onChange(colorVal);
          }
        }}
        title={title ?? colorName}
      />
    );
  }

  function bootstrapColorPicker(colorName: string) {
    const lowName = colorName + "Low";
    const highName = colorName + "High";

    return colorPicker(
      "bootstrap",
      colorName,
      colorName,
      (colorVal: string) => {
        const isLight = tinycolor(
          GlobalTheme.current.colors.neutrals.min
        ).isLight();
        setColor(
          "bootstrap",
          highName,
          ThemeProcessor.upContrast(colorVal, isLight).toHexString()
        );
        setColor(
          "bootstrap",
          lowName,
          ThemeProcessor.downContrast(colorVal, isLight).toHexString()
        );
      }
    );
  }

  function brand() {
    return (
      <div className="mb-2">
        <h2 className="text-uppercase text-sm">Brand</h2>
        {colorPicker("brand", "first")}
        {colorPicker("brand", "second")}
      </div>
    );
  }

  function hues() {
    return (
      <div className="mb-2">
        <h2 className="text-uppercase text-sm">Hues</h2>
        {colorPicker("hues", "red")}
        {colorPicker("hues", "orange")}
        {colorPicker("hues", "yellow")}
        {colorPicker("hues", "green")}
        {colorPicker("hues", "blue")}
        {colorPicker("hues", "indigo")}
        {colorPicker("hues", "violet")}
        {colorPicker("hues", "pink")}
        {colorPicker("hues", "teal")}
        {colorPicker("hues", "cyan")}
      </div>
    );
  }

  function bootstrap() {
    return (
      <div className="mb-2">
        <h2 className="text-uppercase text-sm">Bootstrap</h2>
        <p className="text-sm text-muted">
          Each color has a High and Low contrast pair
        </p>

        {bootstrapColorPicker("default")}
        {bootstrapColorPicker("primary")}
        {bootstrapColorPicker("secondary")}
        {bootstrapColorPicker("info")}
        {bootstrapColorPicker("success")}
        {bootstrapColorPicker("warning")}
        {bootstrapColorPicker("danger")}
      </div>
    );
  }

  function neutrals() {
    return (
      <div className="mb-2">
        <h2 className="text-uppercase text-sm">Neutrals</h2>
        {colorPicker("neutrals", "min")}
        {colorPicker("neutrals", "n100")}
        {colorPicker("neutrals", "n200")}
        {colorPicker("neutrals", "n300")}
        {colorPicker("neutrals", "n400")}
        {colorPicker("neutrals", "n500")}
        {colorPicker("neutrals", "n600")}
        {colorPicker("neutrals", "n700")}
        {colorPicker("neutrals", "n800")}
        {colorPicker("neutrals", "n900")}
        {colorPicker("neutrals", "max")}
        {colorPicker("neutrals", "foreground")}
        {colorPicker("neutrals", "midground")}
        {colorPicker("neutrals", "background")}
      </div>
    );
  }

  return (
    <div css={baseStyles}>
      {brand()}
      {hues()}
      {bootstrap()}
      {neutrals()}
    </div>
  );
};
