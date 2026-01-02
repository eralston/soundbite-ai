using System.Threading.Tasks;

namespace Masticore.Entity
{
    /// <summary>
    /// Infrastructure interface for <see cref="IdentityDb"/>
    /// </summary>
    public interface IIdentityInfrastructure : IBlobInfrastructure
    {
        Task<IIdentityDb> IdentityDbAsync();
    }
}
