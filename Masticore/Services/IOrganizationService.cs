using Masticore.Models;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// Service for IOrganization and IOrganizationDetails
    /// </summary>
    public interface IOrganizationService : IService
    {
        /// <summary>
        /// Gets all organizations assigned to the current user
        /// </summary>
        /// <param name="includeAll">If true, then return every active org - only works for god users</param>
        /// <returns></returns>
        [Obsolete("This needs to be separated into one non-paging for my orgs and one paging for all orgs")]
        Task<IEnumerable<OrganizationExtended>> ReadAllAsync_Obsolete(bool includeAll);

        /// <summary>
        /// Reads all soft deleted orgs
        /// </summary>
        /// <returns></returns>
        [Obsolete("Need to convert to a paging version")]
        Task<IEnumerable<OrganizationExtended>> ReadAllArchivedAsync_Obsolete();

        /// <summary>
        /// Reads the details of the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<OrganizationDetails> ReadAsync(string orgRoute);

        /// <summary>
        /// TODO: REMOVE WHEN ABLE
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Obsolete("Deprecated for perf reasons; use ReadAsync")]
        Task<OrganizationDetails_Obsolete> ReadAsync_Obsolete(string orgRoute);

        /// <summary>
        /// Async reads the sync config for the given org
        /// WARNING: This should NEVER be returned via API of any kind
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<OrgSyncConfig> ReadSyncConfigAsync(string orgRoute);

        /// <summary>
        /// Updates the OrgSyncConfig for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="orgConfig"></param>
        /// <returns></returns>
        Task UpdateSyncConfigAsync(string orgRoute, OrgSyncConfig orgConfig);

        /// <summary>
        /// Creates an org matching the given object
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        Task<OrganizationExtended> CreateAsync(Organization organization);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="organization"></param>
        /// <returns></returns>
        Task<OrganizationExtended> SaveAsync(Organization organization);

        /// <summary>
        /// Soft deletes the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task DeleteAsync(string orgRoute);

        /// <summary>
        /// Restores the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<OrganizationExtended> RestoreAsync(string orgRoute);

        /// <summary>
        /// Updates the given org with the given specific fields
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="updates"></param>
        /// <returns></returns>
        Task<OrganizationExtended> PatchAsync(string orgRoute, JsonPatchDocument updates);

        /// <summary>
        /// Gets the route of an organization based on the organization's universal ID.
        /// </summary>
        /// <param name="universalId">Universal ID of the organization whose route is being sought.</param>
        /// <returns>The route associated with the organization with the specified universal ID or <c>null</c> if the organization is not found.</returns>
        Task<string> GetRouteFromUniversalId(string universalId);

        /// <summary>
        /// Persists an organization image to a data store.
        /// </summary>
        /// <param name="orgRoute">Unique but non-identifying arbitrary key associated with the organization (for now use orgRoute)</param>
        /// <param name="image">Stream containing image data.</param>
        /// <returns>a URL pointing to the image that was uploaded.</returns>
        Task<string> UploadImage(string orgRoute, Stream image);

        /// <summary>
        /// Determines whether the specified user is in the specified organization.
        /// </summary>
        /// <param name="userRoute">Route of the user.</param>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <returns>a task result containg <c>true</c> if the user is part of the organization; otherwise <c>false</c>.</returns>
        Task<bool> IsUserInOrg(string userRoute, string orgRoute);

        /// <summary>
        /// Throw an exception if the current user is NOT in the given org with at least the given role
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="personRole"></param>
        /// <returns></returns>
        Task AssertCurrentUserInOrg(string orgRoute, PersonRole personRole);
    }
}