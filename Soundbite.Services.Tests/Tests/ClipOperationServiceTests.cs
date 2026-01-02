using Microsoft.EntityFrameworkCore;
using Soundbite.Entity;
using Soundbite.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class ClipOperationServiceTests : ServiceTestBase
    {
        #region Constructor

        public ClipOperationServiceTests()
        {
        }

        protected ClipOperationService GetClipOpService()
        {
            ClipOperationService service = new ClipOperationService(
                Builder.SecurityContext.Value,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                Builder.Logger<ClipOperationService>(),
                Builder.Rbac.Value);

            return service;
        }

        #endregion

        #region Methods - Utility

        protected async Task<ClipOperation> AddClipOperation(ClipOperationService service, string clipRoute, ClipOperationType type, ClipOperationStateType state, string billingCode, string externalId, string dataJson)
        {
            ClipOperation clipOp = new ClipOperation()
            {
                BillingCode = billingCode,
                OperationState = state,
                ExternalId = externalId,
                OperationType = type,
                OperationDataJson = dataJson,
                ClipRoute = clipRoute
            };
            await service.SaveAsync(clipOp);
            return clipOp;
        }

        #endregion


        [Fact]
        public async Task ReadById()
        {
            SbDb db = await Builder.Infrastructure.Value.DbAsync();
            ClipEntity clip = await db.Clips.FirstOrDefaultAsync();

            ClipOperationService service = GetClipOpService();
            ClipOperation clipOp = await AddClipOperation(service, clip.Route,
                ClipOperationType.AzureMediaSvcEncoding,
                ClipOperationStateType.Requested,
                "billingCode1", "externalId1", "{x:5}");

            ClipOperation clipOpRetrieved = await service.ReadByRoute(clipOp.Route);
            Assert.NotNull(clipOp.Route);
            Assert.Equal(clipOp.Route, clipOpRetrieved.Route);
        }

        [Fact]
        public async Task ReadByExternalId()
        {
            SbDb db = await Builder.Infrastructure.Value.DbAsync();
            ClipEntity clip = await db.Clips.FirstOrDefaultAsync();
            string externalId = "externalId1";

            ClipOperationService service = GetClipOpService();
            await AddClipOperation(service, clip.Route,
                ClipOperationType.AzureMediaSvcEncoding,
                ClipOperationStateType.Requested,
                "billingCode1", externalId, "{x:5}");

            ClipOperation clipOpRetrieved = await service.ReadByExternalId(externalId);
            Assert.NotNull(clipOpRetrieved);
            Assert.Equal(externalId, clipOpRetrieved.ExternalId);
        }

        [Fact]
        public async Task Save()
        {
            await ReadById(); // Save happens in here so if works save works
        }

        [Fact]
        public async Task ReadByClipRoute()
        {
            SbDb db = await Builder.Infrastructure.Value.DbAsync();
            ClipEntity clip = await db.Clips.FirstOrDefaultAsync();

            ClipOperationService service = GetClipOpService();
            await AddClipOperation(service, clip.Route, ClipOperationType.AzureMediaSvcEncoding, ClipOperationStateType.Requested, "billingCode1", "externalId1", "{x:1}");
            await AddClipOperation(service, clip.Route, ClipOperationType.AzureMediaSvcEncoding, ClipOperationStateType.Requested, "billingCode2", "externalId2", "{x:2}");
            await AddClipOperation(service, clip.Route, ClipOperationType.AzureMediaSvcEncoding, ClipOperationStateType.Requested, "billingCode3", "externalId3", "{x:3}");

            IList<ClipOperation> ops = await service.ReadByClipRoute(clip.Route);

            Assert.NotNull(ops);
            Assert.Equal(3, ops.Count);
        }
    }
}
