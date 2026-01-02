using Masticore.Queue;
using Microsoft.Extensions.Logging;
using Soundbite.Services;
using System;

namespace Soundbite.Queue
{
    /// <summary>
    /// <see cref="JobBase"/> that relies upon an <see cref="ISbInfrastructure"/>
    /// </summary>
    public abstract class SbJobBase : JobBase
    {
        public ISbInfrastructure Infrastructure { get; set; }
        public ILogger Logger { get; set; }

        public SbJobBase() { }

        public SbJobBase(ISbInfrastructure infrastructure, ILogger logger)
        {
            Infrastructure = infrastructure ?? throw new ArgumentNullException(nameof(infrastructure));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}
