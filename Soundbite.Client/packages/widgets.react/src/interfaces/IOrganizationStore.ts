import {
  Group,
  OrganizationExtended,
  OrganizationWithSettings,
} from "@soundbite/api";

/**
 * Interface defininig the contract for a organization store implementation.
 */
export interface IOrganizationStore {
  //////////[ Properties ]//////////////////////////////////////////////////////////////////////////

  /**
   * Contains a list of organizations with which the user is associated.
   */
  myOrgs?: OrganizationExtended[];

  /**
   * Reference to the current organization context under which the application is operating
   */
  currentOrg?: OrganizationWithSettings;

  /**
   * Reference to the current organization route under which the application is operating.
   */
  currentOrgRoute?: string;

  /** Reference to the current user's groups in the current org */
  groups?: Group[];

  /** Reference to all the groups in the current org the user is allowed to see */
  myGroups?: Group[];

  //////////[ Methods ]/////////////////////////////////////////////////////////////////////////////

  /** Clear the store, resetting it to its initial state */
  reset(): void;

  /** Reload the contents of the store based on its last parameters */
  refresh(): Promise<void>;

  /**
   * Populates the myOrgs property of the store.
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   * @returns a promise indicating success or failure of the operation
   */
  readMyOrgsAsync(allowCache?: boolean): Promise<OrganizationExtended[]>;

  /**
   * Gets and organization by organization route.
   * @param orgRoute - route identifying the organization to retrieve.
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  readOrgAsync(
    orgRoute: string,
    allowCache?: boolean
  ): Promise<OrganizationWithSettings>;

  /**
   * Retrieves the specified organization and sets it as the current organization
   * @param orgRoute - route identifying the organization to set as the current organization
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  setCurrentOrganization(
    orgRoute: string,
    allowCache?: boolean
  ): Promise<OrganizationWithSettings>;

  /**
   * Async retrieve the groups for the given org
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  readGroupsAsync(orgRoute: string, allowCache?: boolean): Promise<Group[]>;

  /**
   * Async retrieve the groups for the current user in the given org
   * @param orgRoute
   * @param allowCache - (optional) flag indicating whether caching is allowed (default:true)
   */
  readMyGroupsAsync(orgRoute: string, allowCache?: boolean): Promise<Group[]>;

  /**
   * Read all of the groups to which the current user can publish sessions
   * @param orgRoute
   * @param allowCache
   */
  readMyTargetGroupsAsync(
    orgRoute: string,
    allowCache?: boolean
  ): Promise<Group[]>;
}
