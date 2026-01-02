using System.Threading.Tasks;

namespace Masticore.Entity
{
    /// <summary>
    /// Infrastructure interface for Identity & a customn database 
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    public interface IDbInfrastructure<TDbContext> : IIdentityInfrastructure
    {
        Task<TDbContext> DbAsync();
    }
}
