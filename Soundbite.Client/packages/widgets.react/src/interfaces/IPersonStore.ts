import {
  Invite,
  InviteResult,
  IndexPageResponse,
  IndexPageRequest,
  Person,
} from "@soundbite/api";

/**
 * Interface defininig the contract for a person store implementation.
 */
export interface IPersonStore {
  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  currentPeople?: Person[];

  /** Clears the properties for this store */
  reset(): void;

  /** Reloads the data for this store to update its properties to reflect the last values */
  refresh(): Promise<void>;

  /**
   * Async get the people for the given organization, response broken up via paging
   * @param orgRoute
   * @param pageRequest
   */
  readAllAsync(
    orgRoute: string,
    pageRequest?: IndexPageRequest
  ): Promise<IndexPageResponse<Person>>;

  /**
   * Stores updates to the specified person.
   * @param orgRoute - route of the organization with which the person is associated.
   * @param person - person with updates to persist.
   */
  updatePersonAsync(orgRoute: string, person: Person): Promise<Person>;

  /**
   * Deletes the specified person.
   * @param orgRoute - route of the organization with which the person is associated.
   * @param personRoute - route of the person to delete.
   */
  deletePersonAsync(orgRoute: string, personRoute: string): Promise<void>;

  /**
   * Invites a person to an organization.
   * @param orgRoute - route of the organization to which people should be invited.
   * @param invites - person invitation information.
   */
  invitePersonAsync(
    orgRoute: string,
    invites: Invite[]
  ): Promise<InviteResult[]>;
}
