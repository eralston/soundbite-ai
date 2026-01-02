import { action, observable, makeAutoObservable, makeObservable } from "mobx";

import { Invite, User, UsersService } from "@soundbite/api";

import { IUserStore } from "../interfaces/IUserStore";

/**
 * MobX store class containing states and actions for users
 */
class UserStoreClass implements IUserStore {
  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      currentUser: observable,
      currentUserIsLoaded: observable,
      inviteUsersAsync: action,
      updateCurrentUserImageUrl: action,
    });
  }

  //////////[ Observable Data Properties ]//////////////////////////////////////////////////////////

  /**
   * Stores a reference to the current user's information.
   */
  currentUser?: User = undefined;

  /**
   * Flag indicating whether the current user's information is loaded.
   */
  currentUserIsLoaded: boolean = false;

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Allows the system to update the current user image URL with a new value.
   * @param userImgageUrl - new user image URL
   */
  updateCurrentUserImageUrl(userImgageUrl: string): void {
    if (this.currentUser) {
      this.currentUser.imageSrc = userImgageUrl;
    }
  }

  /**
   * Invites a user to the platform
   * @param invites - invite information (e.g. email address)
   */
  async inviteUsersAsync(invites: Invite[]): Promise<void> {
    await UsersService.inviteAsync(invites);
  }
}

// Export a singleton instance of the store
export const UserStore = new UserStoreClass();
