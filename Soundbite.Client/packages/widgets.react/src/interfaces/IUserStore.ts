import { Invite, User } from "@soundbite/api";

/**
 * Interface defininig the contract for a user store implementation.
 */
export interface IUserStore {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  /**
   * Stores a reference to the current user's information.
   */
  currentUser?: User;

  /**
   * Flag indicating whether the current user's information is loaded.
   */
  currentUserIsLoaded: boolean;

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Allows the system to update the current user image URL with a new value.
   * @param userImgageUrl - new user image URL
   */
  updateCurrentUserImageUrl(userImgageUrl: string): void;

  /**
   * Invites a user to the platform
   * @param invites - invite information (e.g. email address)
   */
  inviteUsersAsync(invites: Invite[]): Promise<void>;
}
