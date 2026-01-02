using AutoMapper;
using Masticore;
using Masticore.Entity;
using Masticore.Resources;
using Masticore.Security;
using Masticore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Soundbite.Entity;
using Soundbite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Soundbite.Services
{
    /// <summary>
    /// Implenentation of a service that handles persisting metadata as a Clip entity then returning a fully loaded IClip
    /// </summary>
    public class ClipOperationService : InfrastructureServiceBase<ISbInfrastructure>, IClipOperationService
    {
        #region Fields

        private readonly IRbac _rbac = null;

        #endregion

        #region Constructor

        public ClipOperationService(
            ISecurityContext securityContext,
            ISbInfrastructure infrastructure,
            IMapper mapper,
            ILogger<ClipOperationService> logger,
            IRbac rbac)
            : base(infrastructure, logger, securityContext, mapper)
        {
            _rbac = rbac;
        }

        #endregion

        #region IClipService

        /// <inheritdic />
        public async Task<ClipOperation> ReadByRoute(string route)
        {
            SbDb db = await Infrastructure.DbAsync();
            ClipOperationEntity entity = db.ClipOperations
                .Include(i => i.Clip)
                .FirstOrDefault(i => i.Route == route);
            ClipOperation result = entity != null
                ? Mapper.Map<ClipOperation>(entity)
                : null;
            return result;
        }

        /// <inheritdoc />
        public async Task<ClipOperation> ReadByExternalId(string externalId)
        {
            SbDb db = await Infrastructure.DbAsync();
            ClipOperationEntity entity = db.ClipOperations
                .Include(i => i.Clip)
                .FirstOrDefault(i => i.ExternalId == externalId);
            ClipOperation result = entity != null
                ? Mapper.Map<ClipOperation>(entity)
                : null;
            return result;
        }

        /// <inheritdic />
        public async Task<IList<ClipOperation>> ReadByClipRoute(string clipRoute)
        {
            SbDb db = await Infrastructure.DbAsync();
            ClipEntity clip = await db.Clips.FirstOrDefaultAsync(i => i.Route == clipRoute);
            IList<ClipOperation> result = Mapper.Map<List<ClipOperation>>(clip.ClipOperations);
            return result;
        }

        /// <inheritdic />
        public async Task<ClipOperation> SaveAsync(ClipOperation clipOp)
        {
            Validator.NotNull(nameof(clipOp), clipOp);
            SbDb db = await Infrastructure.DbAsync();
            ClipOperationEntity entity = string.IsNullOrEmpty(clipOp.Route) ? null
                : db.ClipOperations.FirstOrDefault(i => i.Route == clipOp.Route);

            if (entity == null)
            {
                // Find clip
                ClipEntity clip = db.Clips.FirstOrDefault(i => i.Route == clipOp.ClipRoute);
                clip.AssertFound($"Cannot create clip operation because the clip with route '{clipOp.ClipRoute}' was not found.");

                // Create entity
                entity = EntityBase.Create<ClipOperationEntity>();
                Mapper.Map(clipOp, entity);
                entity.NewRoute();
                entity.Clip = clip;
                await db.ClipOperations.AddAsync(entity);
            }
            else
            {
                // We do NOT allow changing of the clip so do not bother getting clip by route
                Mapper.Map(clipOp, entity);
                db.ClipOperations.Update(entity);
            }

            await db.SaveChangesAsync();
            Mapper.Map(entity, clipOp);
            return clipOp;
        }

        /// <inheritdic />
        public async Task SetState(string clipOpRoute, ClipOperationStateType state, string errorDetails = null)
        {
            Validator.ArgNotNullOrEmpty(nameof(clipOpRoute), clipOpRoute);
            SbDb db = await Infrastructure.DbAsync();
            ClipOperationEntity clipOp = await db.ClipOperations.FirstOrDefaultAsync(i => i.Route == clipOpRoute);
            clipOp.AssertFound($"Failed to locate Clip Operation with route {clipOpRoute}");
            clipOp.OperationState = state;
            clipOp.ErrorDetails = state == ClipOperationStateType.Error ? errorDetails : null;
            if (state == ClipOperationStateType.Complete)
            {
                clipOp.CompletedDate = DateTime.UtcNow;
            }
            db.ClipOperations.Update(clipOp);
            await db.SaveChangesAsync();
        }

        #endregion
    }
}