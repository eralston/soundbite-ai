using Masticore.Models;
using Masticore.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Threading.Tasks;

namespace Masticore.Security
{
    /// <summary>
    /// Implementation of the <see cref="ISecurityContext"/>.
    /// </summary>
    public class SecurityContext : ISecurityContext
    {
        #region Fields

        private User _currentUser;
        private IClaims _claims;
        private int _currentUserId = 0;
        private int _currentTenantId = 0;
        private int _currentPersonId = 0;
        private Person _currentPerson = null;
        private bool? _isThirdPartyRequest = null;
        private string _tokenRaw;
        private JwtSecurityToken _tokenJwtUnvalidated;
        private JwtSecurityToken _tokenJwt;

        /// <summary>
        /// Current <see cref="Tenant"/> object for this context
        /// </summary>
        protected Tenant _currentTenant;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether changes are allowed to the security context once
        /// protected values are set. This defaults to <c>false</c> and should only be set to true
        /// within the mock security context in the unit tests.
        /// </summary>
        protected bool AllowChanges { get; set; } = false;

        /// <summary>
        /// Gets or sets a reference to a tenant service. This is a publicly exposed property because we
        /// have a chicken / egg issue between the security context and the tenant service.  They are 
        /// co-dependant.  To resolve this, we allow the service reference to be set using this property.
        /// NOTE: if it turns out that we "get" the tenant on most requests anyway, we can just pre-populate
        /// the tenant reference in this class in middleware and do away with this issue.
        /// </summary>
        public ITenantService TenantService { get; set; }

        /// <summary>
        /// Gets or sets the bearer token associated with the context.
        /// </summary>
        public string TokenRaw
        {
            get => _tokenRaw;
            set
            {
                if (!string.IsNullOrEmpty(_tokenRaw) && !AllowChanges)
                {
                    throw new SecurityException("BearerToken cannot be changed once it has been set.");
                }
                _tokenRaw = value;
            }
        }

        /// <summary>
        /// Gets or sets the unvalidated JWT token associated with the security context. This 
        /// property is populated any time the <see cref="TokenRaw"/> contains a valid JWT token
        /// but the JWT token is not a valid soundbite token.
        /// </summary>
        public JwtSecurityToken TokenJwtUnvalidated
        {
            get => _tokenJwtUnvalidated;
            set
            {
                if (_tokenJwtUnvalidated != null && !AllowChanges)
                {
                    throw new SecurityException("TokenUnvalidated cannot be changed once it has been set.");
                }
                _tokenJwtUnvalidated = value;
            }
        }

        /// <summary>
        /// Gets or sets the validated JWT token associated with the security context.  This
        /// property is populated any time the <see cref="TokenRaw"/> contains a Soundbite JWT
        /// token that has been passed validation.
        /// </summary>
        public JwtSecurityToken TokenJwt
        {
            get => _tokenJwt;
            set
            {
                if (_tokenJwt != null && !AllowChanges)
                {
                    throw new SecurityException("TokenValidated cannot be changed once it has been set.");
                }
                _tokenJwt = value;
            }
        }

        /// <summary>
        /// Gets or sets the group route associated with the security context.
        /// </summary>
        public string GroupRoute { get; set; }

        /// <summary>
        /// Gets or sets the organization route associated with the security context.
        /// </summary>
        public string OrgRoute { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request was authenticated with a third-party
        /// token.  Third-party tokens can successfully authenticate into the Soundbite API and are
        /// needed for login handshaking.  However, requests authorized from third-party tokens should 
        /// not be authorized for general API data access outside of the login handhshake.
        /// </summary>
        public bool IsThirdPartyRequest
        {
            get => _isThirdPartyRequest.GetValueOrDefault(true);
            set
            {
                if (_isThirdPartyRequest != null && !AllowChanges)
                {
                    throw new SecurityException($"{nameof(IsThirdPartyRequest)} cannot be changed once it is set.");
                }
                _isThirdPartyRequest = value;
            }
        }

        /// <summary>
        /// Reference to the current user information.
        /// </summary>
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != null && !AllowChanges)
                {
                    throw new SecurityException("Current User cannot be changed once it has been set.");
                }
                else if (value != null)
                {
                    _currentUser = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the unique ID of the user.  The User ID value is not accessible from the
        /// <see cref="CurrentUser"/> property because it hides the underlying database value from
        /// going out into the world, so we store it here for reference.
        /// </summary>
        public int CurrentUserId
        {
            get => _currentUserId;
            set
            {
                if (_currentUserId > 0 && !AllowChanges)
                {
                    throw new SecurityException("User ID cannot be changed once it has been set.");
                }
                _currentUserId = value;
            }
        }

        /// <summary>
        /// Gets or sets the ID of the tenant under which the security context is operating.  Users
        /// may belong to multiple tenants and this helps clarify which one is currently in use.
        /// </summary>
        public int CurrentTenantId
        {
            get => _currentTenantId;
            set
            {
                if (_currentTenantId > 0 && !AllowChanges)
                {
                    throw new SecurityException("Tenant ID cannot be changed once it has been set.");
                }
                _currentTenantId = value;
            }
        }

        /// <summary>
        /// Gets or sets a reference to the Person information associated with the context.
        /// </summary>
        public Person CurrentPerson
        {
            get => _currentPerson;
            set
            {
                if (_currentPerson != null && !AllowChanges)
                {
                    throw new SecurityException("Current person cannot be changed once it has been set.");
                }
                _currentPerson = value;
            }
        }

        /// <summary>
        /// Gets or sets the unique ID of the person.  The Person ID value is not accessible from the
        /// <see cref="CurrentPerson"/> property because it hides the underlying database value from
        /// going out into the world, so we store it here for reference.
        /// </summary>
        public int CurrentPersonId
        {
            get => _currentPersonId;
            set
            {
                if (_currentPersonId != 0 && !AllowChanges)
                {
                    throw new SecurityException("Current person ID cannot be changed once it has been set.");
                }
                _currentPersonId = value;
            }
        }

        /// <summary>
        /// The claims object for the current user
        /// </summary>
        public IClaims Claims
        {
            get => _claims;
            set
            {
                if (_claims != null && !AllowChanges)
                {
                    throw new SecurityException("Claims cannot be changed once it has been set.");
                }
                _claims = value;
                DirectoryUniversalId = _claims.DirectoryUniversalId;
                Email = _claims.Email;
                FullName = _claims.FullName;
                UniversalId = _claims.UniversalId;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new <see cref="SecurityContext"/> instance.
        /// </summary>        
        public SecurityContext()
        {
        }

        #endregion

        #region IClaims Implementation

        /// <summary>
        /// Gets the email for this user
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets the fullname of the user
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets the unique ID for the directory authenticating this user
        /// </summary>
        public string DirectoryUniversalId { get; set; }

        /// <summary>
        /// Gets the unique ID for the user
        /// </summary>
        public string UniversalId { get; set; }

        /// <summary>
        /// Return provider type for this security context
        /// </summary>
        /// <remarks>
        /// Right now, we only support <see cref="ProviderType.AAD"/>
        /// </remarks>
        public ProviderType ProviderType => ProviderType.AAD;

        #endregion

        #region Methods

        /// <summary>
        /// Gets the current tenant
        /// </summary>
        /// <returns>a reference to the tenant associated with the current security context</returns>
        public virtual async Task<Tenant> CurrentTenant()
        {
            if (_currentTenant == null)
            {
                //NOTE: if we end up setting this value on most requests anyway, just do away lazy loading and populate it inside security middleware
                _currentTenant = await TenantService.GetByUserRouteAsync(CurrentUser.Route);
            }
            return _currentTenant;
        }

        /// <summary>
        /// Sets the current tenant for the security context.  This is primarily intended for testing scenarios.
        /// </summary>        
        /// <param name="currentTenant">Current tenant to assign to the security context.</param>
        public void CurrentTenant(Tenant currentTenant)
        {
            if (_currentTenant != null && !AllowChanges)
            {
                throw new SecurityException("Current Tenant cannot be changed.");
            }
            _currentTenant = currentTenant;
        }

        #endregion
    }
}
