using Masticore.Token;
using System.Threading.Tasks;

namespace Masticore.Services
{
    /// <summary>
    /// A service for managing tokens and related token data.
    /// </summary>
    public interface ITokenDataService : IService
    {
        /// <summary>
        /// Responsible for issuing a user and refresh token in response to a successful login.
        /// </summary>
        /// <param name="orgRoute">Route of the organization for which a token is being issued.</param>
        /// <returns>a <see cref="TokenInfo"/> instance containing a user token and a refresh token.</returns>
        Task<TokenInfo> CreateUserToken(string orgRoute);

        /// <summary>
        /// Responsible for validating the incomming refresh token and, if the refresh token is 
        /// valid, issuing a new user and refresh token.
        /// </summary>
        /// <param name="refreshToken">Refresh token used to acquire the new token.</param>
        /// <param name="orgRoute">Route of the organization assocaited with the token.</param>
        /// <returns>a <see cref="TokenInfo"/> instance containing a user token and a refresh token.</returns>
        Task<TokenInfo> RefreshUserToken(string refreshToken, string orgRoute);

        /// <summary>
        /// Deletes the token settings for an organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose settings are being deleted.</param>
        /// <returns>a task signaling success/failure of the operation</returns>
        Task DeleteOrgTokenSettings(string orgRoute);

        /// <summary>
        /// Deletes the token settings for a tenant.
        /// </summary>
        /// <param name="tenantRoute">Route of the organization whose settings are being persisted.</param>
        /// <param name="settings">Settings object to invalidate</param>
        /// <returns>a task signaling success/failure of the operation</returns>
        Task DeleteTenantTokenSettings(string tenantRoute, ITokenServiceSettings settings);

        /// <summary>
        /// Sets the token settings for an organization.
        /// </summary>
        /// <param name="orgRoute">Route of the organization whose settings are being persisted.</param>
        /// <param name="settings">Settings to persist.</param>
        /// <returns>a task signaling success/failure of the operation</returns>
        Task SetOrgTokenSettings(string orgRoute, ITokenServiceSettings settings);

        /// <summary>
        /// Sets the token settings for a tenant.
        /// </summary>
        /// <param name="tenantRoute">Route of the organization whose settings are being persisted.</param>
        /// <param name="settings">Settings to persist.</param>
        /// <returns>a task signaling success/failure of the operation</returns>
        Task SetTenantTokenSettings(string tenantRoute, ITokenServiceSettings settings);

        /// <summary>
        /// Gets the settings for the specified organization / tenant.  If the organization does not 
        /// have explicit settings defined, this method falls back to the settings defined on the
        /// tenant.  If tenant has no settings, defaults for the application are returned.
        /// </summary>
        /// <param name="orgRoute">Route of the organization.</param>
        /// <returns>an <see cref="ITokenServiceSettings"/> implementation populated with settings applicable to the organization.</returns>
        Task<TokenServiceSettings> GetTokenSettingsByOrgRoute(string orgRoute);

        /// <summary>
        /// Gets the settings for the specified organization / tenant.  If the organization does not 
        /// have explicit settings defined, this method falls back to the settings defined on the
        /// tenant.  If tenant has no settings, defaults for the application are returned.
        /// </summary>
        /// <param name="orgUniversalId">Universal ID of the organization.</param>
        /// <returns>an <see cref="ITokenServiceSettings"/> implementation populated with settings applicable to the organization.</returns>
        Task<TokenServiceSettings> GetTokenSettingsByOrgUniversalId(string orgUniversalId);
    }
}