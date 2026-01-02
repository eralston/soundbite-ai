import tinycolor from "tinycolor2";
import { Utils, Theme } from "@soundbite/api";

export const defaultContrastPercentage = 10;

/**
 * Takes a theme object and adjusts it to be complete if needed
 * This includes generating color variants if needed
 * */
export class ThemeProcessor {
  /**
   * Augments the given theme with derived colors and any further processing
   * @param theme
   * @param constrastPercentage
   */
  public static process(
    theme: Theme,
    constrastPercentage: number = defaultContrastPercentage
  ): ThemeProcessor {
    const processor = new ThemeProcessor(theme, constrastPercentage);
    processor.process();
    return processor;
  }

  public originalTheme: Theme;
  protected isLight: boolean;

  constructor(
    public theme: Theme,
    public contrastPercentage: number = defaultContrastPercentage
  ) {
    this.originalTheme = Utils.clone(theme);
    this.isLight = tinycolor(theme.colors.neutrals.min).isLight();
  }

  public static upContrast(
    color: string,
    isLight: boolean,
    percent: number = defaultContrastPercentage
  ) {
    // If we don't have a color, then derive based on the basecolor
    let tinyColor = tinycolor(color);
    if (isLight) {
      return tinyColor.darken(percent);
    } else {
      return tinyColor.lighten(percent);
    }
  }

  public static downContrast(
    color: string,
    isLight: boolean,
    percent: number = defaultContrastPercentage
  ) {
    let tinyColor = tinycolor(color);
    if (isLight) {
      return tinyColor.lighten(percent);
    } else {
      return tinyColor.darken(percent);
    }
  }

  public upContrast(color: string | undefined, baseColor: string): string {
    // If we have a color, then just use that
    if (color !== undefined) return color;

    const tinyColor = ThemeProcessor.upContrast(
      baseColor,
      this.isLight,
      this.contrastPercentage
    );

    return tinyColor.toHexString();
  }

  public downContrast(color: string | undefined, baseColor: string): string {
    // If we have a color, then just use that
    if (color !== undefined) return color;

    // If we don't have a color, then derive based on the basecolor
    const tinyColor = ThemeProcessor.downContrast(
      baseColor,
      this.isLight,
      this.contrastPercentage
    );

    return tinyColor.toHexString();
  }

  /**
   * Fixes up the Bootstrap colors in the
   * */
  bootstrap(): void {
    const bootstrap = this.theme.colors.bootstrap;
    bootstrap.dangerHigh = this.upContrast(
      bootstrap.dangerHigh,
      bootstrap.danger
    );
    bootstrap.dangerLow = this.downContrast(
      bootstrap.dangerLow,
      bootstrap.danger
    );

    bootstrap.defaultHigh = this.upContrast(
      bootstrap.defaultHigh,
      bootstrap.default
    );
    bootstrap.defaultLow = this.downContrast(
      bootstrap.defaultLow,
      bootstrap.default
    );

    bootstrap.infoHigh = this.upContrast(bootstrap.infoHigh, bootstrap.info);
    bootstrap.infoLow = this.downContrast(bootstrap.infoLow, bootstrap.info);

    bootstrap.primaryHigh = this.upContrast(
      bootstrap.primaryHigh,
      bootstrap.primary
    );
    bootstrap.primaryLow = this.downContrast(
      bootstrap.primaryLow,
      bootstrap.primary
    );

    bootstrap.secondaryHigh = this.upContrast(
      bootstrap.secondaryHigh,
      bootstrap.secondary
    );
    bootstrap.secondaryLow = this.downContrast(
      bootstrap.secondaryLow,
      bootstrap.secondary
    );

    bootstrap.successHigh = this.upContrast(
      bootstrap.successHigh,
      bootstrap.success
    );
    bootstrap.successLow = this.downContrast(
      bootstrap.successLow,
      bootstrap.success
    );

    bootstrap.warningHigh = this.upContrast(
      bootstrap.warningHigh,
      bootstrap.warning
    );
    bootstrap.warningLow = this.downContrast(
      bootstrap.warningLow,
      bootstrap.warning
    );
  }

  /**
   * Returns a Theme object based on the Theme given to the constructor with enhancements
   * */
  public process(): Theme {
    this.bootstrap();
    return this.theme;
  }
}
