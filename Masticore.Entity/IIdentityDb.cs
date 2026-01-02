using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Masticore.Entity
{
    /// <summary>
    /// Interface for an object that implements DbSets for Identity & an async save action
    /// </summary>
    public interface IIdentityDb : IDisposable
    {
        DbSet<EnvSetting> EnvSettings { get; set; }
        DbSet<MemberEntity> Members { get; set; }
        DbSet<OrganizationEntity> Organizations { get; set; }
        DbSet<OrganizationAuthProviderEntity> OrganizationAuthProviders { get; set; }
        DbSet<PersonEntity> People { get; set; }
        DbSet<GroupEntity> Groups { get; set; }
        DbSet<TenantEntity> Tenants { get; set; }
        DbSet<TokenSettingsEntity> TokenSettings { get; set; }
        DbSet<UserEntity> Users { get; set; }
        DbSet<UserIdentityEntity> UserIdentities { get; set; }
        DbSet<QueuedJobStatusEntity> QueuedJobStatuses { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Mimics the Update call for an EF Core DB context.
        /// </summary>
        /// <typeparam name="TEntity">.NET type of the entity to update</typeparam>
        /// <param name="entity">Entity to update</param>
        /// <returns>
        ///   The Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry for the entity. The
        ///   entry provides access to change tracking information and operations for the entity.
        /// </returns>
        EntityEntry<TEntity> Update<TEntity>([NotNull] TEntity entity) where TEntity : class;
    }
}