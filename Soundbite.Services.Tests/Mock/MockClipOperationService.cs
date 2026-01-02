using Soundbite.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    public class MockClipOperationService : IClipOperationService
    {
        public Task<IList<ClipOperation>> ReadByClipRoute(string clipRoute)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClipOperation> ReadByExternalId(string externalId)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClipOperation> ReadByRoute(string route)
        {
            throw new System.NotImplementedException();
        }

        public Task<ClipOperation> SaveAsync(ClipOperation clipOp)
        {
            throw new System.NotImplementedException();
        }

        public Task SetState(string clipOpRoute, ClipOperationStateType state, string errorDetails = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
