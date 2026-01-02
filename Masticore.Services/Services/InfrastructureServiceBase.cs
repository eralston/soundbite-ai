using AutoMapper;
using Masticore.Entity;
using Masticore.Security;
using Microsoft.Extensions.Logging;
using System;

namespace Masticore.Services
{
    /// <summary>
    /// Defines a base class for IIdentityInfrastructure derivative services
    /// </summary>
    /// <typeparam name="TInfrastructure"></typeparam>
    public abstract class InfrastructureServiceBase<TInfrastructure>
        where TInfrastructure : class, IIdentityInfrastructure
    {
        protected ILogger Logger { get; }
        protected TInfrastructure Infrastructure { get; }
        protected ISecurityContext SecurityContext { get; }
        protected IMapper Mapper { get; }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
            where TSource : class
            where TDestination : class
        {
            if (source == null)
            {
                return destination;
            }

            if (Mapper == null)
            {
                throw new Exception("Service does not have a valid Mapper reference.");
            }

            return Mapper.Map(source, destination);
        }

        public InfrastructureServiceBase(
            TInfrastructure infrastructure,
            ILogger logger,
            ISecurityContext securityContext = null,
            IMapper mapper = null)
        {
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            SecurityContext = securityContext;
            Mapper = mapper;
        }
    }
}
