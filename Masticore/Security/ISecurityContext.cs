using Masticore.Models;
using Masticore.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Masticore.Security
{
    /// <summary>
    /// Contract for a security context which provides security-centric information
    /// </summary>
    public interface ISecurityContext : IClaims
    {
        #region Properties

        /// <summary>
        /// Gets or sets a reference to a tenant service. This is a publicly exposed property because we
        /// have a chicken / egg issue between the security context and the tenant service.  They are 
        /// co-dependant.  To resolve this, we allow the service reference to be set using this property.
        /// NOTE: if it turns out that we "get" the tenant on most requests anyway, we can just pre-populate
        /// the tenant reference in this class in middleware and do away with this issue.
        /// </summary>
        ITenantService TenantService { get; set; }

        /// <summary>
        /// Gets or sets the raw bearer token value associated with the context.
        /// </summary>
        string TokenRaw { get; set; }

        /// <summary>
        /// Gets or sets the unvalidated JWT token associated with the security context. This 
        /// property is populated any time the <see cref="TokenRaw"/> contains a valid JWT token
        /// but the JWT token is not a valid soundbite token.
        /// </summary>
        JwtSecurityToken TokenJwtUnvalidated { get; set; }

        /// <summary>
        /// Gets or sets the validated JWT token associated with the security context.  This
        /// property is populated any time the <see cref="TokenRaw"/> contains a Soundbite JWT
        /// token that has been validated
        /// </summary>
        JwtSecurityToken TokenJwt { get; set; }

        /// <summary>
        /// Gets or sets the group route associated with the security context.
        /// </summary>
        string GroupRoute { get; set; }

        /// <summary>
        /// Gets or sets the organization route associated with the security context.
        /// </summary>
        string OrgRoute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request was authenticated with a third-party
        /// token.  Third-party tokens can successfully authenticate into the Soundbite API and are
        /// needed for login handshaking.  However, requests authorized from third-party tokens should 
        /// not be authorized for general API data access outside of the login handhshake.
        /// </summary>
        bool IsThirdPartyRequest { get; set; }

        /// <summary>
        /// Reference to the current user information.
        /// </summary>
        User CurrentUser { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the user.  The User ID value is not accessible from the
        /// <see cref="CurrentUser"/> property because it hides the underlying database value from
        /// going out into the world, so we store it here for reference.
        /// </summary>
        int CurrentUserId { get; set; }

        /// <summary>
        /// Gets or sets a reference to the Person information associated with the context.
        /// </summary>
        Person CurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets the unique ID of the person.  The Person ID value is not accessible from the
        /// <see cref="CurrentPerson"/> property because it hides the underlying database value from
        /// going out into the world, so we store it here for reference.
        /// </summary>
        int CurrentPersonId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the tenant under which the security context is operating.  Users
        /// may belong to multiple tenants and this helps clarify which one is currently in use.
        /// </summary>
        int CurrentTenantId { get; set; }

        /// <summary>
        /// Gets or sets claims associated with third-party authentication providers.
        /// </summary>
        IClaims Claims { get; set; }

        /// <summary>
        /// Override the email property from IClaims to be read/write
        /// </summary>
        new string Email { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current tenant
        /// </summary>
        /// <returns>a reference to the tenant associated with the current security context</returns>
        Task<Tenant> CurrentTenant();

        /// <summary>
        /// Sets the current tenant for the security context.  This is primarily intended for testing scenarios.
        /// </summary>        
        /// <param name="currentTenant">Current tenant to assign to the security context.</param>
        void CurrentTenant(Tenant currentTenant);

        #endregion        
    }
}