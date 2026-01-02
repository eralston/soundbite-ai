/**
 * Enum for platform
 * */
export enum Platform {
  Web = 0,
  Teams = 1,
}

/**
 * Class for capturing intrinsic settings
 * There are NOT loaded from the server
 * */
export class EnvironmentSettings {
  // TODO: Figure out how to determine if we're in Teams or not
  platform = Platform.Web;

  // Singleton
  static instance = new EnvironmentSettings();
}
