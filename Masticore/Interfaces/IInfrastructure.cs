using System;

namespace Masticore
{
    /// <summary>
    /// Marker interface for an object that provides disposabe resources as a factory.
    /// EG, mapping the current request to an underlying database for a multi-tenant platform
    /// </summary>
    public interface IInfrastructure : IDisposable { }
}
