using AutoMapper;
using Masticore.Entity;
using Masticore.Security;
using Microsoft.Extensions.Logging;

namespace Masticore.Services
{
    /// <summary>
    /// Base class for service that captures ILogger & Infrastructure
    /// </summary>
    public abstract class IdentityServiceBase : InfrastructureServiceBase<IIdentityInfrastructure>
    {
        public IdentityServiceBase(
            IIdentityInfrastructure infrastructure,
            ILogger logger,
            IMapper mapper = null,
            ISecurityContext securityContext = null)
            : base(infrastructure, logger, securityContext, mapper)
        {
        }
    }
}
