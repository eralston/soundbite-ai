import { Logger } from "@soundbite/api";
import { BrowserStorageService } from "./BrowserStorageService";

/**
 * Provides navigation services to other services in the application.  React makes it difficult to
 * access hooks outside of components (e.g. in a service).  This service allows a top-level
 * component to initialize the navigation service class with a delegate that has access to the React
 * router.  The navigation service can then "hand-off" service calls to the component seamlessly.
 */
class NavServiceClass {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  /**
   * Stores the delegate function for routing that has access to the React router.
   */
  private gotoFunc: (path: string) => void = () => {};

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Initializes the service by passing in a delegate function (presumably from a component) that
   * is capable of handling routing calls.
   * @param navToFunc - delegate that handles routing calls
   */
  initialize(navToFunc: (path: string) => void): void {
    this.gotoFunc = navToFunc;
  }

  /**
   * Gets a flag indicating whether there is a return to page defined.
   */
  get hasReturnPage(): boolean {
    return !!BrowserStorageService.returnToPage;
  }

  /**
   * Sets the return page when a user is redirected away from the site for authentication purposes.
   * @param path - path to which the user should be returned after successfully logging in
   */
  setReturnPage(path: string | null) {
    BrowserStorageService.returnToPage = path;
  }

  /**
   * Navigates to the return page if defined.  This method clears the return page value.
   */
  gotoReturnPage() {
    if (this.hasReturnPage) {
      const returnToPage = BrowserStorageService.returnToPage;
      BrowserStorageService.returnToPage = null;
      this.goto(returnToPage);
    }
  }

  /**
   * Routes the application to the specified path.
   * @param path - path to which the application should be routed.
   */
  goto(path: string | null) {
    if (path) {
      this.gotoFunc(path);
      Logger.LogTrace(`NavService - navigating to ${path}`);
    } else {
      Logger.LogWarning(`NavService - navigation path was null/empty`);
    }
  }
}

export default new NavServiceClass();
