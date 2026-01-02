import { Theme } from "@soundbite/api";
import tinycolor from "tinycolor2";

/**
 * Enum of the contrast level expected in the colors
 * */
export enum ContrastLevel {
  /**  */
  None = "None",
  /** AA Contrast */
  Minimum = "AA",
  /** AAA Contrast */
  Enhanced = "AAA",
}

/**
 * Returns the target readability ratio for a given contrast level
 * For more strict ratings between colors, try https://webaim.org/resources/contrastchecker/
 * They basically recommends 4.5 for AA (assuming presence of small text) and 7 for AAA
 * @param contrast
 */
export function ratioForLevel(contrast: ContrastLevel): number {
  switch (contrast) {
    case ContrastLevel.None:
      return 1;
    case ContrastLevel.Minimum:
      return 3; // Assumes no "small" text in the interface, so this is a true minimum
    case ContrastLevel.Enhanced:
      return 7;
  }
}

/**
 * Given two colors, finds the target ratio for the second color
 * @param baseColor
 * @param targetColor
 * @param targetRatio
 */
export function findContrast(
  baseColor: string,
  targetColor: string,
  targetRatio: number
): string {
  // Setup
  const maxCount = 50;
  const increment = 0.5;
  let count = 0;
  let candidate = tinycolor(targetColor);
  const baseIsLight = tinycolor(baseColor).isLight();
  // Iterate to find the target contrast
  while (
    count < maxCount &&
    tinycolor.readability(baseColor, candidate) < targetRatio
  ) {
    const candidateTinycolor = tinycolor(candidate);
    candidate = baseIsLight
      ? candidateTinycolor.darken(increment)
      : candidateTinycolor.lighten(increment);
    ++count;
  }
  return candidate.toHexString().toUpperCase();
}

/** Describes the minimal interface for a theme check rule */
export interface ThemeAssert {
  assert(): boolean;
  message?: string;
}

/** Base class with fields and methods for ThemeAssert */
export class ThemeAssertBase implements ThemeAssert {
  public message?: string;

  public assert(): boolean {
    return true;
  }
}

/** Asserts that two colors */
export class RelativeContrastAssert extends ThemeAssertBase {
  constructor(
    protected readonly baseColor: string,
    protected readonly higherColor: string,
    protected readonly lowerColor: string,
    protected readonly description: string,
    protected readonly isLogging: boolean = true
  ) {
    super();
  }

  assert(): boolean {
    const baseToA = tinycolor.readability(this.baseColor, this.lowerColor);
    const baseToB = tinycolor.readability(this.baseColor, this.higherColor);
    if (baseToA < baseToB) {
      return true;
    } else {
      const targetRatio = baseToA * 1.05;
      const suggestedColor = findContrast(
        this.baseColor,
        this.higherColor,
        targetRatio
      );
      this.message = `Relative contrast fail ${
        this.description
      }: ${this.lowerColor.toUpperCase()} is higher contrast than ${this.higherColor.toUpperCase()}; try something like ${suggestedColor.toUpperCase()}`;

      if (this.isLogging) {
        console.warn(this.message);
      }

      return false;
    }
  }
}

/** Asserting the contrast ratio of two colors */
export class ContrastAssert extends ThemeAssertBase {
  public static readonly defaultLevel: ContrastLevel = ContrastLevel.Minimum;

  constructor(
    protected readonly colorA: string,
    protected readonly colorB: string,
    protected readonly description: string,
    protected readonly level: ContrastLevel = ContrastLevel.Minimum,
    protected readonly isLogging: boolean = true
  ) {
    super();
  }

  assert(): boolean {
    return this.checkContrast(
      this.colorA,
      this.colorB,
      this.description,
      this.level
    );
  }

  public checkContrast(
    colorA: string,
    colorB: string,
    description: string,
    level: ContrastLevel = ContrastAssert.defaultLevel
  ) {
    const contrast = tinycolor.readability(colorA, colorB);
    const targetRatio = ratioForLevel(level);
    if (contrast < targetRatio) {
      const candidate = findContrast(colorA, colorB, targetRatio);
      this.message = `Contrast fail ${description}: ${colorA.toUpperCase()} ${colorB.toUpperCase()} has a contrast of ${contrast} which is not ${level}; consider ${candidate}`;

      if (this.isLogging) {
        console.warn(this.message);
      }

      return false;
    } else {
      return true;
    }
  }
}

/**
 * A Composite class of ThemeTest, which runs over a grouping of them
 * */
export class ThemeTest extends ThemeAssertBase {
  public static assert(
    theme: Theme,
    isLogging: boolean = true,
    level: ContrastLevel = ContrastLevel.Minimum
  ): ThemeTest {
    const checker = new ThemeTest(theme, isLogging, level);
    checker.assert();
    return checker;
  }

  public asserts: ThemeAssert[] = [];
  public isValid: boolean = true;

  constructor(
    protected readonly theme: Theme,
    public isLogging: boolean = true,
    public level: ContrastLevel = ContrastLevel.Minimum
  ) {
    super();
    this.addNeutral();
    this.addBootstrap();
  }

  public add(assert: ThemeAssert) {
    this.asserts.push(assert);
  }

  public addContrastAssert(
    colorA: string,
    colorB: string,
    desc: string,
    level: ContrastLevel = this.level
  ) {
    const assert = new ContrastAssert(colorA, colorB, desc, level);
    this.add(assert);
  }

  public addRelativeContrastAssert(
    baseColor: string,
    higherColor: string,
    lowerColor: string,
    desc: string
  ) {
    const assert = new RelativeContrastAssert(
      baseColor,
      higherColor,
      lowerColor,
      desc
    );
    this.add(assert);
  }

  public addNeutral(): void {
    this.addContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.neutrals.max,
      "Min to Max - Must be AAA",
      ContrastLevel.Enhanced
    );
    // Neutral steps
    this.addContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.neutrals.n600,
      "min to 600"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.n100,
      this.theme.colors.neutrals.n700,
      "100 to 700"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.n200,
      this.theme.colors.neutrals.n800,
      "200 to 800"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.n300,
      this.theme.colors.neutrals.n900,
      "300 to 900"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.n400,
      this.theme.colors.neutrals.max,
      "400 to max"
    );
    // Typography
    this.addContrastAssert(
      this.theme.colors.neutrals.background,
      this.theme.colors.neutrals.n900,
      "Background to 900"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.midground,
      this.theme.colors.neutrals.n900,
      "Midground to 900"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.foreground,
      this.theme.colors.neutrals.n900,
      "Foreground to 900"
    );
  }

  public checkHues(): void {}

  public addBootstrap(): void {
    // Precendence of CTA colors
    this.addRelativeContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.primary,
      this.theme.colors.bootstrap.secondary,
      "Primary to Secondary"
    );
    this.addRelativeContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.primary,
      this.theme.colors.bootstrap.default,
      "Primary to Default"
    );
    this.addRelativeContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.primary,
      this.theme.colors.bootstrap.info,
      "Primary to Info"
    );
    this.addRelativeContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.primary,
      this.theme.colors.bootstrap.success,
      "Primary to Success"
    );
    this.addRelativeContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.primary,
      this.theme.colors.bootstrap.warning,
      "Primary to Warning"
    );
    this.addRelativeContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.primary,
      this.theme.colors.bootstrap.danger,
      "Primary to Danger"
    );

    // Readability of buttons, etc
    this.addContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.primary,
      "Min to Primary"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.secondary,
      "Min to Secondary"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.default,
      "Min to Default"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.info,
      "Min to Info"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.success,
      "Min to Success"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.warning,
      "Min to Warning"
    );
    this.addContrastAssert(
      this.theme.colors.neutrals.min,
      this.theme.colors.bootstrap.danger,
      "Min to Danger"
    );
  }

  /**
   * Runs all check rules, emits warnings when issues are found, and returns true if all rules pass - otherwise false
   * */
  public assert(): boolean {
    if (this.level === ContrastLevel.None) {
      this.isValid;
    }

    // Run all rules and return false if a single one fails
    for (const test of this.asserts) {
      const pass = test.assert();
      if (!pass) {
        this.isValid = false;
      }
    }

    return this.isValid;
  }
}
