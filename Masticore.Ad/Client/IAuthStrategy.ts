/**
 * Describes an object that receives login events
 * */
export interface IAuthObserver {
  onAuth: (strategy: IAuthStrategy) => void;
}

/**
 * Interface for describing an object that implements SPA authentication
 * */
export default interface IAuthStrategy {
  /**
   * Async return true if user is logged in; otherwise, return false
   * */
  isLoggedInAsync(observer?: IAuthObserver): Promise<boolean>;

  /**
   * If the strategy is currently waiting on a login
   * If this is false, try calling isLoggedInAsync - if that returns true, then it should be done logging in afterward
   * */
  isLoggingIn(): boolean;

  /**
   * Async login the user, preferably redirecting rather tha popping up
   * May throw AuthError
   * */
  loginAsync(): Promise<void>;

  /**
   * Async returns an access token. This may have the side-effect of loggin the user in again
   * */
  tokenAsync(): Promise<string>;

  /**
   * Logs out the current user, redirecting them
   * */
  logoutAsync(): void;
}
