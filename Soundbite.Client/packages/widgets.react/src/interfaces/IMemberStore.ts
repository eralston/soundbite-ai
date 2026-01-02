import { Invite, Member } from "@soundbite/api";

/**
 * Interface defininig the contract for a member store implementation.
 */
export interface IMemberStore {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////
  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Async read the members of the given group in the given org
   * @param orgRoute
   * @param groupRoute
   */
  readMembersAsync(orgRoute: string, groupRoute: string): Promise<Member[]>;

  /**
   * Adds a new group member
   * @param orgRoute - route of the organization with which the group/member are associated.
   * @param groupRoute - route of the group with which to associated the user.
   * @param invites - identifies the user to invite to the group.
   */
  addMembersAsync(
    orgRoute: string,
    groupRoute: string,
    invites: Invite[]
  ): Promise<void>;

  /**
   * Stores updates to the specified member.
   * @param orgRoute - route of the organization with which the group/member are associated.
   * @param groupRoute - route of the group with which the user is associated.
   * @param member - member with updates to persist.
   */
  updateMemberAsync(
    orgRoute: string,
    groupRoute: string,
    member: Member
  ): Promise<Member>;

  /**
   * Deletes the specified member from a group.
   * @param orgRoute - route of the organization with which the group/member are associated.
   * @param groupRoute - route of the group from which the member should be deleted.
   * @param memberRoute - route of the member to delete.
   */
  deleteMemberAsync(
    orgRoute: string,
    groupRoute: string,
    memberRoute: string
  ): Promise<void>;
}
