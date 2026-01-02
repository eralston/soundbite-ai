using Masticore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// A service for IPerson
    /// </summary>
    public interface IPersonService : IService
    {
        /// <summary>
        /// Async invites the given list of <see cref="Invite"/> objects to the given org. User must already be in the org.
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="invite"></param>
        /// <returns></returns>
        Task<InviteResult[]> InviteAsync(string orgRoute, Invite[] invite);

        /// <summary>
        /// Reads the <see cref="Person"/> record for the current user
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        Task<Person> ReadMeAsync(string orgRoute);

        /// <summary>
        /// Reads the <see cref="Person"/> record for the given person in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="personRoute"></param>
        /// <returns></returns>
        Task<Person> ReadAsync(string orgRoute, string personRoute);

        /// <summary>
        /// Reads all people for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Obsolete]
        Task<IEnumerable<Person>> ReadAllAsync_Obsolete(string orgRoute);

        /// <summary>
        /// Read all people with pagination
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="request"></param>
        /// <param name="personRole"></param>
        /// <returns></returns>
        Task<IndexPageResponse<Person>> ReadAllAsync(string orgRoute, IndexPageRequest request = null, PersonRole? personRole = null);

        /// <summary>
        /// Updates the <see cref="Person"/> record for the given person in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="personRoute"></param>
        /// <param name="personFields"></param>
        /// <returns></returns>
        Task<Person> UpdateAsync(string orgRoute, string personRoute, Person personFields);

        /// <summary>
        /// Remove the <see cref="Person"/> record for the given person from the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="personRoute"></param>
        /// <returns></returns>
        Task DeleteAsync(string orgRoute, string personRoute);
    }
}