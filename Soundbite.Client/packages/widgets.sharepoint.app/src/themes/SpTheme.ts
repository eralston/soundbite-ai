import { Theme, ThemeColors, ThemeFonts } from "@soundbite/api";

/**
 * The default fonts for Soundbite
 * */
export const spDefaultFont: ThemeFonts = {
  fontFamily: 'Lexend, "Nunito Sans", sans-serif',
  fontFamilyMono:
    'SFMono-Regular, Menlo, Monaco, Consolas,"Liberation Mono", "Courier New", monospace',
};

/**
 * Declares the default lights colors
 * */
export const spDefaultColors: ThemeColors = {
  neutrals: {
    background: "#F0EEF0",
    midground: "#FFFFFF",
    foreground: "#FFFFFF",
    min: "#FFFFFF",
    n100: "#F8F8F8",
    n200: "#E6E6E6",
    n300: "#D4D4D4",
    n400: "#C2C2C2",
    n500: "#B1B1B1",
    n600: "#949494",
    n700: "#8D8D8D",
    n800: "#6A6A6A",
    n900: "#484848",
    max: "#424242",
  },
  hues: {
    red: "#f5365c",
    orange: "#fb6340",
    yellow: "#ffd600",
    green: "#2dce89",
    blue: "#5e72e4",
    indigo: "#5603ad",
    violet: "#8965e0",

    pink: "#f3a4b5",
    teal: "#11cdef",
    cyan: "#2bffc6",
  },
  bootstrap: {
    default: "#69797e",
    primary: "#0081d4", // Brand blue is #0081d4
    secondary: "#69797e",
    info: "#0da3be",
    success: "#25a870",
    danger: "#d80000",
    dangerLow: "#ff3b3b",
    warning: "#fb6340",
  },
  brand: {
    first: "#0081d4", // Logo Blue: Green Blue Crayola
    second: "#2a073d", // Logo Purple: Russian Violet
  },
  transparent: "transparent",
};

/**
 * The default light theme
 * */
export const spDefaultTheme: Theme = {
  name: "Soundbite Light",
  colors: spDefaultColors,
  fonts: spDefaultFont,
  images: {
    logo: "/img/logo_transparent.png",
    logoAlt: "/img/logo_white.png",
    mark: "/img/logomark_transparent.png",
  },
};
