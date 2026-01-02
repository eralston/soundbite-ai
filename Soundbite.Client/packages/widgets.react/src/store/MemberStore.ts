import { Invite, Member, MembersService } from "@soundbite/api";
import { makeObservable } from "mobx";
import { IMemberStore } from "../interfaces/IMemberStore";
import { GroupStore } from "./GroupStore";

/**
 * MobX store class containing states and actions for members
 */
class MemberStoreClass implements IMemberStore {
  //////////[ Constructor ]/////////////////////////////////////////////////////////////////////////////

  constructor() {
    makeObservable(this, {});
  }

  //////////[ Observable Data Properties ]//////////////////////////////////////////////////////////

  /* None */

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Async read the members of the given group in the given org
   * @param orgRoute
   * @param groupRoute
   */
  async readMembersAsync(
    orgRoute: string,
    groupRoute: string
  ): Promise<Member[]> {
    const members = await MembersService.readAllAsync_Obsolete(
      orgRoute,
      groupRoute
    );
    return members;
  }

  /**
   * Adds a new group member
   * @param orgRoute - route of the organization with which the group/member are associated.
   * @param groupRoute - route of the group with which to associated the user.
   * @param invites - identifies the user to invite to the group.
   */
  async addMembersAsync(
    orgRoute: string,
    groupRoute: string,
    invites: Invite[]
  ): Promise<void> {
    await MembersService.inviteAsync(orgRoute, groupRoute, invites);
    await GroupStore.onAnyMemberChanged(orgRoute, groupRoute);
  }

  /**
   * Stores updates to the specified member.
   * @param orgRoute - route of the organization with which the group/member are associated.
   * @param groupRoute - route of the group with which the user is associated.
   * @param member - member with updates to persist.
   */
  async updateMemberAsync(
    orgRoute: string,
    groupRoute: string,
    member: Member
  ): Promise<Member> {
    const updatedMember = await MembersService.updateAsync(
      orgRoute,
      groupRoute,
      member.route,
      member
    );
    await GroupStore.onAnyMemberChanged(orgRoute, groupRoute);
    return updatedMember;
  }

  /**
   * Deletes the specified member from a group.
   * @param orgRoute - route of the organization with which the group/member are associated.
   * @param groupRoute - route of the group from which the member should be deleted.
   * @param memberRoute - route of the member to delete.
   */
  async deleteMemberAsync(
    orgRoute: string,
    groupRoute: string,
    memberRoute: string
  ): Promise<void> {
    await MembersService.deleteAsync(orgRoute, groupRoute, memberRoute);
    await GroupStore.onAnyMemberChanged(orgRoute, groupRoute);
  }
}

// Export a singleton instance of the store
export const MemberStore = new MemberStoreClass();
