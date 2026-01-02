import { ApiError } from "../generated/models";
import { Logger } from "./Logger";

/**
 * Error class for communicating issues back to the user - because exceptions are the UI of APIs
 * This wraps lower-level errors such that the technical information is still available
 * */
export class PublicError extends Error {
  name = "Application Error";
  message = "Echoes of the sound of silence...";
  innerError?: Error;

  public static Log(error?: Error, innerError?: Error) {
    // TODO: Put in the real support page
    const errorJson: string =
      (error ?? innerError) != null
        ? JSON.stringify(error, null, 2)
        : "UNKNOWN";
    Logger.LogError(
      `Unexpected error. Consider emailing Support@Soundbite.Freshdesk.Com or visiting https://soundbite.ai/ ${errorJson}`,
      error
    );
    if (innerError) Logger.LogError("Additional Error Info:", innerError);
  }

  constructor(innerError?: unknown, name?: string, message?: string) {
    super(message);

    this.innerError = innerError as Error;

    if (name) this.name = name;

    if (message) this.message = message;

    const apiError = this.ApiError;

    if (apiError != null && apiError.isUserSafe) {
      this.message = `${apiError.message}: ${apiError.statusCode} response trying to ${apiError.method} to ${apiError.path}`;
    }

    PublicError.Log(apiError, this.innerError);
  }

  public get ApiError(): ApiError | undefined {
    const data = (this.innerError as any)?.response?.data;
    if (data == null) {
      return undefined;
    }

    if (!data.isApiError) {
      return undefined;
    }
    const apiError = data as ApiError;
    console.error(
      "Consider contacting Support@Soundbite.Freshdesk.Com regarding API error",
      apiError
    );
    return apiError;
  }
}
