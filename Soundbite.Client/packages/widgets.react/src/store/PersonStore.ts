import {
  Invite,
  InviteResult,
  IndexPageRequest,
  IndexPageResponse,
  PeopleService,
  Person,
} from "@soundbite/api";
import { action, makeObservable, observable } from "mobx";
import { IPersonStore } from "../interfaces/IPersonStore";

/**
 * MobX store class containing states and actions for people
 */
class PersonStoreClass implements IPersonStore {
  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {
      // Actions
      deletePersonAsync: action,
      invitePersonAsync: action,
      readAllAsync: action,
      reset: action,
      updatePersonAsync: action,
    });
  }

  //////////[ Observable Data Properties ]//////////////////////////////////////////////////////////

  protected lastOrgRoute?: string = undefined;

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  /** Clear the state */
  reset(): void {
    this.lastOrgRoute = undefined;
  }

  /** Based on the last arguments, reloads the properties for this store */
  async refresh(): Promise<void> {
    // NOTHING
  }

  async readAllAsync(
    orgRoute: string,
    pageRequest?: IndexPageRequest
  ): Promise<IndexPageResponse<Person>> {
    const ret = await PeopleService.readAllAsync(orgRoute, pageRequest);
    this.lastOrgRoute = orgRoute;
    return ret;
  }

  /**
   * Stores updates to the specified person.
   * @param orgRoute - route of the organization with which the person is associated.
   * @param person - person with updates to persist.
   */
  async updatePersonAsync(orgRoute: string, person: Person): Promise<Person> {
    const ret = await PeopleService.updateAsync(orgRoute, person.route, person);
    return ret;
  }

  /**
   * Deletes the specified person.
   * @param orgRoute - route of the organization with which the person is associated.
   * @param personRoute - route of the person to delete.
   */
  async deletePersonAsync(
    orgRoute: string,
    personRoute: string
  ): Promise<void> {
    await PeopleService.delete(orgRoute, personRoute);
    if (orgRoute === this.lastOrgRoute) {
      await this.refresh();
    }
  }

  /**
   * Invites a person to an organization.
   * @param orgRoute - route of the organization to which people should be invited.
   * @param invites - person invitation information.
   */
  async invitePersonAsync(
    orgRoute: string,
    invites: Invite[]
  ): Promise<InviteResult[]> {
    const ret = await PeopleService.inviteAsync(orgRoute, invites);
    if (orgRoute === this.lastOrgRoute) {
      this.refresh();
    }
    return ret;
  }
}

// Export a singleton instance of the store
export const PersonStore = new PersonStoreClass();
