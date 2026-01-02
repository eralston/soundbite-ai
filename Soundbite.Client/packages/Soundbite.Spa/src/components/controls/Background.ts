export enum BackgroundMode {
  Default = "",
  Dark = "sb-dark-background",
}

export default class Background {
  static remove(mode: BackgroundMode) {
    if (mode.length === 0) return;

    document.body.classList.remove(mode);
  }

  static add(mode: BackgroundMode) {
    if (mode.length === 0) return;

    document.body.classList.add(mode);
  }

  public static apply(mode: BackgroundMode = BackgroundMode.Default) {
    Background.remove(BackgroundMode.Default);
    Background.remove(BackgroundMode.Dark);

    Background.add(mode);
  }
}
