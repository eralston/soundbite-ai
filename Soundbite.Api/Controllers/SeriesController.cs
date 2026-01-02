using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Soundbite.Models;
using Soundbite.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Api.Controllers
{
    /// <summary>
    /// Controller with endpoints for interacting with series definitions for sessions.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ApiController]
    [Authorize]
    public class SeriesController : ControllerBase
    {
        #region Properties

        /// <summary>
        /// Gets the <see cref="ISeriesService"/> underlying this controller
        /// </summary>
        protected ISeriesService Series { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SeriesController"/> class.
        /// </summary>
        /// <param name="seriesService">ISeriesService DI reference.</param>
        public SeriesController(ISeriesService seriesService)
        {
            Series = seriesService;
        }

        #endregion

        #region EndPoints

        /// <summary>
        /// Retrieves a series by route.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the series.</param>
        /// <param name="seriesRoute">Route of the series to retrieve.</param>
        /// <returns>a series details instance populated with the requested series information, otherwise throws an HTTP 404 not found exception.</returns>
        [Route("/organizations/{orgRoute}/Series/{seriesRoute}")]
        [HttpGet]
        public async Task<SeriesDetails> ReadAsync(string orgRoute, string seriesRoute)
        {
            return await Series.ReadAsync(orgRoute, seriesRoute);
        }

        /// <summary>
        /// Async get the collection of <see cref="SeriesPreview"/> objects for the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <returns></returns>
        [Route("/organizations/{orgRoute}/Series")]
        [HttpGet]
        public async Task<IEnumerable<SeriesPreview>> ReadAllAsync(string orgRoute)
        {
            return await Series.ReadAllAsync_Obsolete(orgRoute);
        }

        /// <summary>
        /// Async get the collection of <see cref="SeriesPreview"/> associated with the given team in the given org
        /// </summary>
        /// <param name="orgRoute"></param>
        /// <param name="groupRoute"></param>
        /// <returns></returns>
        [Route("/Organizations/{orgRoute}/Groups/{groupRoute}/Series")]
        // TODO: Remove "team" route
        [Route("/Organizations/{orgRoute}/Teams/{groupRoute}/Series")]
        [HttpGet]
        public async Task<IEnumerable<SeriesPreview>> ReadAllGroupAsync(string orgRoute, string groupRoute)
        {
            return await Series.ReadAllAsync_Obsolete(orgRoute, groupRoute);
        }

        /// <summary>
        /// Performs a "soft" delete of the series.
        /// </summary>
        /// <param name="orgRoute">Route of the organization containing the series.</param>
        /// <param name="seriesRoute">Route of the series to "soft" delete.</param>
        [Route("/organizations/{orgRoute}/Series/{seriesRoute}")]
        [HttpDelete]
        public async Task DeleteAsync(string orgRoute, string seriesRoute)
        {
            await Series.DeleteAsync(orgRoute, seriesRoute);
        }

        #endregion
    }
}