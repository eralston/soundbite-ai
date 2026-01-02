import { action, observable } from "mobx";
import {
  OrganizationsService,
  Organization,
  OrganizationExtended,
  Utils,
} from "@soundbite/api";
import { ErrorDlg } from "@soundbite/widgets-react";

export default class OrgStore {
  //////////[ Fields ]//////////////////////////////////////////////////////////////////////////////

  private _orgsInitialized = false;
  private _archivedOrgsInitialized = false;

  //////////[ Observable Properties ]///////////////////////////////////////////////////////////////

  /**
   * Observable array containing all of the active organizations
   */
  @observable
  orgs: OrganizationExtended[] = [];

  /**
   * Observable array containing all of the archived organizations
   */
  @observable
  orgsArchived: OrganizationExtended[] = [];

  //////////[ Actions ]/////////////////////////////////////////////////////////////////////////////

  /**
   * Responsible for loading organization data into the orgs property of the store.  This method
   * should be called from a component before accessing the orgs property.
   * @param refresh - flag indicating whether cached data should be refreshed from the server
   */
  @action
  async loadOrgs(refresh: boolean = false): Promise<OrganizationExtended[]> {
    try {
      if (!this._orgsInitialized || refresh) {
        // Mark the orgs as having been initialized to avoid multiple calls (if requested)
        this._orgsInitialized = true;
        this.orgs = await OrganizationsService.readAllAsGodAsync();
      }
      return this.orgs;
    } catch (err: any) {
      ErrorDlg.show(
        err,
        "Error Loading Organizations",
        "Likely could not make contact with server"
      );
      throw err;
    }
  }

  /**
   * Responsible for loading archived organization data into the archivedOrgs property of the store.
   * This method should be called from a component before accessing the archivedOrgs property.
   * @param refresh - flag indicating whether cached data should be refreshed from the server
   */
  @action
  async loadArchivedOrgs(
    refresh: boolean = false
  ): Promise<OrganizationExtended[]> {
    try {
      if (!this._archivedOrgsInitialized || refresh) {
        // Mark the archived rgs as having been initialized to avoid multiple calls (if requested)
        this._archivedOrgsInitialized = true;
        this.orgsArchived = await OrganizationsService.readAllArchivedAsync();
      }
      return this.orgsArchived;
    } catch (err: any) {
      ErrorDlg.show(
        err,
        "Error Loading Archived Organizations",
        "Likely could not make contact with server"
      );
      throw err;
    }
  }

  /**
   * Responsible for loading a single organization by route.
   * @param orgRoute - route of the organization to load.
   */
  @action
  async readOrgByRoute(
    orgRoute: string,
    refresh: boolean = false
  ): Promise<OrganizationExtended | null> {
    try {
      await this.loadOrgs(refresh);
      const org = this.orgs.find((i) => i.route === orgRoute) ?? null;
      return org;
    } catch (err: any) {
      ErrorDlg.show(
        err,
        "Error Getting Organization",
        "Likely could not make contact with server"
      );
      throw err;
    }
  }

  /**
   * Creates the specified organization on the server
   * @param org - organization to create
   * @return - a reference to the saved organization with additional (e.g. route) information
   */
  @action
  async createOrg(org: Organization): Promise<OrganizationExtended> {
    try {
      var response = await OrganizationsService.createAsync(org);
      if (response && this._orgsInitialized) {
        this.orgs.push(response);
        this.orgs = this.orgs.sort((a, b) =>
          Utils.stringCompare(a?.name, b?.name)
        );
      }
      return response;
    } catch (err: any) {
      ErrorDlg.show(
        err,
        "Error Creating Organization",
        "Likely could not make contact with server"
      );
      throw err;
    }
  }

  /**
   * Updates the specified organization with the specified patch information.
   * @param orgRoute - identifies the organiation to which the updates should apply
   * @param patch - specifies the updates to apply to the organization
   */
  @action
  async updateOrg(orgRoute: string, patch: any): Promise<void> {
    try {
      if (patch) {
        var currentItem = await this.readOrgByRoute(orgRoute);
        if (currentItem) {
          var updatedItem = await OrganizationsService.patchAsync(
            orgRoute,
            patch
          );
          if (updatedItem) {
            // TODO: copying these values over because they are not returned by the service.  Need to
            updatedItem.imageSrc = currentItem.imageSrc;
            updatedItem.isAccepting = currentItem.isAccepting;
            // Copy the updated item over the current item
            Object.assign(currentItem, updatedItem);
          } else {
            throw new Error(
              `Updated appears to have succeeded but did not return a valid organization item for '${orgRoute}'`
            );
          }
        } else {
          throw new Error(
            `Could not find existing item with organization route '${orgRoute}'`
          );
        }
      }
    } catch (err: any) {
      ErrorDlg.show(
        err,
        "Error Updating Organization",
        "Likely could not make contact with server"
      );
    }
  }

  /**
   * Restores an archived organization to active status.
   * @param orgRoute - route of the organization to restore.
   */
  @action
  async restoreOrg(orgRoute: string) {
    try {
      await OrganizationsService.restoreAsync(orgRoute);
      let existing: OrganizationExtended | null | undefined = null;

      if (this._archivedOrgsInitialized) {
        existing = this.orgsArchived.find((i) => i.route === orgRoute);
        if (existing) {
          this.orgsArchived = this.orgsArchived.filter((i) => i !== existing);
        }
      }

      if (existing && this._orgsInitialized) {
        this.orgs.push(existing);
        this.orgs = this.orgs.sort((a, b) =>
          Utils.stringCompare(a?.name, b?.name)
        );
      }
    } catch (err: any) {
      ErrorDlg.show(
        err,
        "Error Restoring Organization",
        "Likely could not make contact with server"
      );
    }
  }

  /**
   * Archives an organization.  While this is called delete, the system actually performs a soft
   * delete and puts the organization in archive status.
   * @param orgRoute - route of the organization to archive.
   */
  @action
  async deleteOrg(orgRoute: string) {
    try {
      await OrganizationsService.deleteAsync(orgRoute);
      let existing: OrganizationExtended | null | undefined = null;

      if (this._orgsInitialized) {
        existing = this.orgs.find((i) => i.route === orgRoute);
        if (existing) {
          this.orgs = this.orgs.filter((i) => i !== existing);
        }
      }

      if (existing && this._archivedOrgsInitialized) {
        this.orgsArchived.push(existing);
        this.orgsArchived = this.orgsArchived.sort((a, b) =>
          Utils.stringCompare(a?.name, b?.name)
        );
      }
    } catch (err: any) {
      ErrorDlg.show(
        err,
        "Error Deleting Organization",
        "Likely could not make contact with server"
      );
    }
  }

  /**
   * Uploads an avatar image to represent the organization.
   * @param orgRoute - route of the organization for which an avatar image is being uplaoded.
   * @param file - file containing the uploaded file information
   * @param fileName - name of the file
   */
  @action
  async uploadOrgImage(
    orgRoute: string,
    file: any,
    fileName: string
  ): Promise<void> {
    try {
      let fd = new FormData();
      fd.append("image", file, fileName);
      var uploadUrl = await OrganizationsService.uploadImageAsync(orgRoute, fd);
      var org = await this.readOrgByRoute(orgRoute);
      if (org !== null) {
        org.imageSrc = uploadUrl;
      }
    } catch (err: any) {
      ErrorDlg.show(
        err,
        "Error Uploading Org Image",
        "Likely could not make contact with server"
      );
    }
  }
}
