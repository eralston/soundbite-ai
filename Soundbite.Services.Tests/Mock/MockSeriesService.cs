using Soundbite.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    public class MockSeriesService : ISeriesService
    {
        public Task<SeriesDetails> CreateAsync(string orgRoute, NewSession newSession)
        {
            return null;
        }

        public Task DeleteAsync(string orgRoute, string seriesRoute)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<SeriesPreview>> ReadAllAsync_Obsolete(string orgRoute)
        {
            IEnumerable<SeriesPreview> ret = new SeriesPreview[] { };
            return Task.FromResult(ret);
        }

        public Task<IEnumerable<SeriesPreview>> ReadAllAsync_Obsolete(string orgRoute, string grpRoute)
        {
            IEnumerable<SeriesPreview> ret = new SeriesPreview[] { };
            return Task.FromResult(ret);
        }

        public Task<SeriesDetails> ReadAsync(string orgRoute, string seriesRoute)
        {
            return null;
        }

        public Task<SeriesDetails> UpdateAsync(string orgRoute, string seriesRoute, NewSession session)
        {
            return null;
        }
    }
}
