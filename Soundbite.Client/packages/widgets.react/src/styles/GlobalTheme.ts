import {
  Utils,
  Theme,
  ThemeColors,
  ThemeFonts,
  ThemeName,
} from "@soundbite/api";
import moment from "moment";
import tinycolor from "tinycolor2";
import { ThemeTest } from ".";
import { LocalValue } from "../modules/StorageValue";
import { ThemeProcessor } from "./ThemeProcessor";
import { ContrastLevel } from "./ThemeTest";

/**
 * The default fonts for Soundbite
 * */
export const defaultFont: ThemeFonts = {
  fontFamily: 'Lexend, "Nunito Sans", sans-serif',
  fontFamilyMono:
    'SFMono-Regular, Menlo, Monaco, Consolas,"Liberation Mono", "Courier New", monospace',
};

/**
 * Given a base theme name, return a unique string. It is string sortable as being "more than" any previously generated unique name
 * @param name
 */
export function uniqueThemeName(name: ThemeName): string {
  const uniqueTime = moment().utc().format();
  return `${uniqueTime}:${name}`;
}

/**
 * Declares the default lights colors
 * */
export const lightColors: ThemeColors = {
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
    default: "#9B89A9",
    primary: "#0081d4", // Brand blue is #0081d4
    secondary: "#9B89A9", // Brand purple is #2a073d, this is desat
    info: "#0da3be",
    success: "#25a870",
    danger: "#f5365c",
    warning: "#AF9300",
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
export const lightTheme: Theme = {
  name: ThemeName.Light,
  colors: lightColors,
  fonts: defaultFont,
  images: {
    logo: "/img/logo_transparent.png",
    logoAlt: "/img/logo_white.png",
    mark: "/img/logomark_transparent.png",
  },
};

// Login dark blue: #172b4d

/**
 * Declares the default lights colors
 * */
export const darkColors: ThemeColors = {
  // Reversed collection of colors to be dark
  neutrals: {
    background: "#334667",
    midground: "#172b4d",
    foreground: "#32325d",
    min: "#282828",
    n100: "#6A6A6A",
    n200: "#7A7A7A",
    n300: "#8B8B8B",
    n400: "#9C9C9C",
    n500: "#ACACAC",
    n600: "#BDBDBD",
    n700: "#CDCDCD",
    n800: "#DEDEDE",
    n900: "#EEEEEE",
    max: "#FFFFFF",
  },

  // TODO: Consider different hues for dark
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

  // TODO: Consider altered bootstrap colors for dark
  bootstrap: {
    default: "#8FACDD",
    primary: "#8F9DDD",
    secondary: "#DEEAF2",
    info: "#55DBF3",
    success: "#48D799",
    danger: "#F97F97",
    warning: "#F9B412",
  },

  brand: {
    first: "#8FB9D4",
    second: "#8FACDD",
  },

  transparent: "transparent",
};

/**
 * The default light theme
 * */
export const darkTheme: Theme = {
  name: ThemeName.Dark,
  colors: darkColors,
  fonts: defaultFont,
  images: {
    logo: "/img/logo_white.png",
    logoAlt: "/img/logo_white.png",
    mark: "/img/logomark_transparent.png",
  },
};

/**
 * Class that manages global theme
 * All themeable SB components should read from this class as a fallback
 * */
export class GlobalTheme {
  protected static _instance?: Theme = undefined;
  public static currentOrgRoute?: string;
  public static loadedOrgRoute?: string;
  public static targetContrastLevel: ContrastLevel = ContrastLevel.Minimum;
  protected static _lastTester?: ThemeTest;
  protected static _lastProcessor?: ThemeProcessor;

  protected static lastOrgRouteStore = new LocalValue<string>(
    GlobalTheme.name,
    "lastOrgRouteStore"
  );

  protected static getOrgThemeStore(
    orgRoute?: string
  ): LocalValue<Theme> | undefined {
    if (orgRoute == null || orgRoute === "") {
      return undefined;
    }
    const key = `storedTheme:${orgRoute}`;
    return new LocalValue<Theme>(GlobalTheme.name, key);
  }

  public static get lastTester(): ThemeTest | undefined {
    return GlobalTheme._lastTester;
  }

  public static get lastProcessor(): ThemeProcessor | undefined {
    return GlobalTheme._lastProcessor;
  }

  public static get isNeedReload(): boolean {
    return (
      this.currentOrgRoute != null &&
      this.loadedOrgRoute != null &&
      this.currentOrgRoute !== this.loadedOrgRoute
    );
  }

  /** Global theme instance, which overrides any non-specified values in an SB component */
  public static get current(): Theme {
    if (this._instance === undefined) {
      const lastOrgRoute = this.lastOrgRouteStore.get();

      let theme: Theme | null | undefined = null;

      // First we try the org specific theme
      if (lastOrgRoute != null) {
        this.loadedOrgRoute = lastOrgRoute;
        theme = this.getOrgThemeStore(lastOrgRoute)?.get();
      }

      // Finally, figure out if we need to fallback to the default
      if (theme == null || !this.isThemeValid(theme)) {
        theme = Utils.clone(lightTheme);
      }

      // Prep and return
      this._instance = this.process(theme).theme;
      GlobalTheme.test(this._instance);
    }
    return this._instance;
  }

  public static process(theme: Theme): ThemeProcessor {
    this._lastProcessor = ThemeProcessor.process(theme);
    return this._lastProcessor;
  }

  public static isThemeValid(theme?: Theme): boolean {
    if (theme == null) {
      return false;
    }

    return (
      theme.colors != null &&
      theme.colors.bootstrap != null &&
      theme.colors.brand != null &&
      theme.colors.neutrals != null &&
      theme.colors.hues != null &&
      theme.images != null
    );
  }

  /**
   * Sets the global theme for the application
   * This will only be applied to components supporting theming that have not already been loaded
   * @param theme
   */

  public static set(
    theme: Theme,
    orgRoute: string,
    persist: boolean = true
  ): void {
    // Cannot set an unpublished or older theme than the one we currently have
    if (!this.isThemeValid(theme)) {
      if (this.current.name === ThemeName.Draft) {
        theme = this.current;
      } else {
        theme = Utils.clone(lightTheme);
      }
    }

    this.processAndSave(theme, orgRoute, persist);
  }

  public static processAndSave(
    theme: Theme,
    orgRoute: string,
    persist: boolean = true
  ): void {
    this._lastProcessor = ThemeProcessor.process(theme);
    GlobalTheme.test(theme);
    if (this._instance == null) {
      this.loadedOrgRoute = orgRoute;
    }
    this._instance = theme;
    if (
      persist &&
      orgRoute != null &&
      (theme.name === ThemeName.Draft || theme.publishedUtc != null)
    ) {
      const orgStore = this.getOrgThemeStore(orgRoute);
      if (orgStore != null) {
        orgStore.set(theme);
      }
    } else if (
      persist &&
      (theme.name === ThemeName.Dark || theme.name === ThemeName.Light)
    ) {
      const orgStore = this.getOrgThemeStore(orgRoute);
      if (orgStore != null) {
        orgStore.remove();
      }
    }

    this.currentOrgRoute = orgRoute;
    this.lastOrgRouteStore.set(orgRoute);
  }

  public static setDraft(theme: Theme, orgRoute: string) {
    if (theme == null) {
      return;
    }

    if (!this.isThemeValid(theme)) {
      return;
    }

    theme = Utils.clone(theme);
    theme.name = ThemeName.Draft;
    theme.publishedUtc = undefined;

    this.processAndSave(theme, orgRoute);
  }

  public static test(theme: Theme, isLogging: boolean = true): ThemeTest {
    this._lastTester = ThemeTest.assert(
      theme,
      isLogging,
      this.targetContrastLevel
    );
    return this._lastTester;
  }

  public static clear(orgRoute?: string): void {
    if (orgRoute == null) {
      return;
    }
    this.getOrgThemeStore(orgRoute)?.remove();
    this.lastOrgRouteStore.set(orgRoute);
    this.currentOrgRoute = orgRoute;
  }

  public static apply(): void {
    window.location.reload();
  }

  public static mix(
    weight: number,
    color1: string = GlobalTheme.current.colors.brand.first,
    color2: string = GlobalTheme.current.colors.brand.second
  ) {
    const mixedColor = tinycolor.mix(color1, color2, weight);
    const backgroundColor = mixedColor.toHexString();
    return backgroundColor;
  }
}

/* Hack window to offer theme swapping utilizies */
(window as any).sbSetLightTheme = () => {
  GlobalTheme.set(lightTheme, GlobalTheme.currentOrgRoute ?? "");
  GlobalTheme.apply();
};
(window as any).sbSetDarkTheme = () => {
  GlobalTheme.set(darkTheme, GlobalTheme.currentOrgRoute ?? "");
  GlobalTheme.apply();
};
(window as any).sbSetResetTheme = () => {
  localStorage.clear();
  GlobalTheme.clear();
  GlobalTheme.apply();
};
