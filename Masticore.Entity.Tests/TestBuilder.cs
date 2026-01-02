using AutoMapper;
using Masticore.Jobs;
using Masticore.Media;
using Masticore.Models;
using Masticore.Security;
using Masticore.Services;
using Masticore.Tests;
using Masticore.Transcription;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masticore.Entity.Tests
{
    /// <summary>
    /// A builder of mock objects for testing
    /// </summary>
    /// <typeparam name="TInfrastructure"></typeparam>
    /// <typeparam name="TDbContext"></typeparam>
    public class TestBuilder<TInfrastructure, TDbContext>
        where TDbContext : DbContext
        where TInfrastructure : class, IDbInfrastructure<TDbContext>, new()
    {
        #region Constructor

        public TestBuilder(TestUserContext userContext)
            : this()
        {
            SetCurrentUserContext(userContext).Wait();
        }

        public TestBuilder()
        {
            DbContext = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, TDbContext, TInfrastructure, TDbContext>(this, i => i.Infrastructure.Value.DbAsync().Result);
            DbSnapshot = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, DbSnapshot, TInfrastructure, TDbContext>(this, i => new DbSnapshot(i.DbContext.Value));
            IdentityDb = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IIdentityDb, TInfrastructure, TDbContext>(this, i => i.Infrastructure.Value.IdentityDbAsync().Result);
            ImageService = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IImageService, TInfrastructure, TDbContext>(this);
            Infrastructure = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, TInfrastructure, TInfrastructure, TDbContext>(this, i => new TInfrastructure());
            JobQueue = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IJobQueue, TInfrastructure, TDbContext>(this, i => new ImmediateQueue());
            Mapper = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IMapper, TInfrastructure, TDbContext>(this, i => MockMapper.Instance);
            MediaProcessing = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IMediaProcessingService, TInfrastructure, TDbContext>(this, i => new MockMediaProcessingService());
            MemberService = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IMemberService, TInfrastructure, TDbContext>(this);
            NotificationService = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, INotificationService, TInfrastructure, TDbContext>(this);
            OrganizationService = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IOrganizationService, TInfrastructure, TDbContext>(this);
            PersonService = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IPersonService, TInfrastructure, TDbContext>(this);
            QueuedJobStatus = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IQueuedJobStatusService, TInfrastructure, TDbContext>(this);
            Rbac = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IRbac, TInfrastructure, TDbContext>(this, i => new CurrentUserDbRbac(i.Logger<CurrentUserDbRbac>(), i.Infrastructure.Value, i.SecurityContext.Value));
            SecurityContext = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, ISecurityContext, TInfrastructure, TDbContext>(this, (i) => new MockSecurityContext() { });
            TokenDataService = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, ITokenDataService, TInfrastructure, TDbContext>(this);
            TranscriptionService = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, ITranscriptionService, TInfrastructure, TDbContext>(this);
            UserService = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IUserService, TInfrastructure, TDbContext>(this);
            HttpClientFactory = new ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IHttpClientFactory, TInfrastructure, TDbContext>(this, i => Extensions.CreateHttpClientFactory());
            _ = Infrastructure.Value;
        }

        #endregion

        #region Methods - SetCurrentUserContext

        public async Task SetCurrentUserContext(TestUserContext userContext)
        {
            switch (userContext)
            {
                case TestUserContext.Anonymous:
                    SecurityContext.Value.UniversalId = null;
                    SecurityContext.Value.CurrentUserId = 0;
                    SecurityContext.Value.CurrentUser = null;
                    SecurityContext.Value.CurrentTenant(null);
                    SecurityContext.Value.CurrentTenantId = 0;
                    break;

                case TestUserContext.ExternalOrgMember:
                    await SetCurrentUserContextByRoute(IdentityDbSeed.UserOutside.Route);
                    SecurityContext.Value.CurrentTenant(Mapper.Value.Map<Tenant>(IdentityDbSeed.Tenant));
                    SecurityContext.Value.CurrentTenantId = IdentityDbSeed.Tenant.Id;
                    break;

                case TestUserContext.GodUser:
                    await SetCurrentUserContextByRoute(IdentityDbSeed.UserGod.Route);
                    SecurityContext.Value.CurrentTenant(Mapper.Value.Map<Tenant>(IdentityDbSeed.Tenant));
                    SecurityContext.Value.CurrentTenantId = IdentityDbSeed.Tenant.Id;
                    break;

                case TestUserContext.OrgAdmin:
                case TestUserContext.OrgAdminGroupOwner:
                    await SetCurrentUserContextByRoute(IdentityDbSeed.UserOrgAdmin.Route);
                    SecurityContext.Value.CurrentTenant(Mapper.Value.Map<Tenant>(IdentityDbSeed.Tenant));
                    SecurityContext.Value.CurrentTenantId = IdentityDbSeed.Tenant.Id;
                    break;

                case TestUserContext.OrgMember:
                    await SetCurrentUserContextByRoute(IdentityDbSeed.UserRegular.Route);
                    SecurityContext.Value.CurrentTenant(Mapper.Value.Map<Tenant>(IdentityDbSeed.Tenant));
                    SecurityContext.Value.CurrentTenantId = IdentityDbSeed.Tenant.Id;
                    break;

                case TestUserContext.OrgMemberGroupOwner:
                    await SetCurrentUserContextByRoute(IdentityDbSeed.UserNonOrgAdminGroupOwner.Route);
                    SecurityContext.Value.CurrentTenant(Mapper.Value.Map<Tenant>(IdentityDbSeed.Tenant));
                    SecurityContext.Value.CurrentTenantId = IdentityDbSeed.Tenant.Id;
                    break;

                case TestUserContext.OrgMemberGroupMember:
                    await SetCurrentUserContextByRoute(IdentityDbSeed.UserGroupMember.Route);
                    SecurityContext.Value.CurrentTenant(Mapper.Value.Map<Tenant>(IdentityDbSeed.Tenant));
                    SecurityContext.Value.CurrentTenantId = IdentityDbSeed.Tenant.Id;
                    break;

                case TestUserContext.OrgMemberNotGroupMember:
                    await SetCurrentUserContextByRoute(IdentityDbSeed.UserOrgMemberNoGroupMembership.Route);
                    SecurityContext.Value.CurrentTenant(Mapper.Value.Map<Tenant>(IdentityDbSeed.Tenant));
                    SecurityContext.Value.CurrentTenantId = IdentityDbSeed.Tenant.Id;
                    break;

                case TestUserContext.RandomUser:
                    SecurityContext.Use(new MockSecurityContext(true, false));
                    SecurityContext.Value.CurrentTenant(Mapper.Value.Map<Tenant>(IdentityDbSeed.Tenant));
                    SecurityContext.Value.CurrentTenantId = IdentityDbSeed.Tenant.Id;
                    break;

                case TestUserContext.RandomUserRandomDir:
                    SecurityContext.Use(new MockSecurityContext(true, true));
                    break;

                case TestUserContext.SessionParticipantNoGroupAffiliation:
                    await SetCurrentUserContextByRoute(IdentityDbSeed.UserRegular.Route);
                    SecurityContext.Value.CurrentTenant(Mapper.Value.Map<Tenant>(IdentityDbSeed.Tenant));
                    SecurityContext.Value.CurrentTenantId = IdentityDbSeed.Tenant.Id;
                    break;
            }
        }

        public async Task SetCurrentUserContextByRoute(string userRoute)
        {
            UserEntity user = await UserForRoute(userRoute);
            SecurityContext.Value.UniversalId = user.UniversalId;
            SecurityContext.Value.CurrentUserId = user.Id;
            SecurityContext.Value.CurrentUser = Mapper.Value.Map<User>(user);
            SecurityContext.Value.Email = user.Email;
        }

        public async Task<UserEntity> UserForRoute(string userRoute)
        {
            return await IdentityDb.Value.Users.Where(i => i.Route == userRoute).FirstAsync();
        }

        public async Task SetCurrentUserContext(UserEntity user)
        {
            if (user != null)
            {
                await SetCurrentUserContextByRoute(user.Route);
            }
        }

        public bool UseNullLogger { get; set; }


        #endregion

        #region Methods

        public ILogger<T> Logger<T>(bool useNullLogger = false)
        {
            if (useNullLogger || UseNullLogger)
            {
                return new NullLogger<T>();
            }
            else
            {
                return new ThrowIfErrorLogger<T>();
            }
        }

        #endregion

        #region Properties

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, DbSnapshot, TInfrastructure, TDbContext> DbSnapshot { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IIdentityDb, TInfrastructure, TDbContext> IdentityDb { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, TDbContext, TInfrastructure, TDbContext> DbContext { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, TInfrastructure, TInfrastructure, TDbContext> Infrastructure { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IImageService, TInfrastructure, TDbContext> ImageService { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IMapper, TInfrastructure, TDbContext> Mapper { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IMediaProcessingService, TInfrastructure, TDbContext> MediaProcessing { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IMemberService, TInfrastructure, TDbContext> MemberService { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, INotificationService, TInfrastructure, TDbContext> NotificationService { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IOrganizationService, TInfrastructure, TDbContext> OrganizationService { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IPersonService, TInfrastructure, TDbContext> PersonService { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, ITokenDataService, TInfrastructure, TDbContext> TokenDataService { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IRbac, TInfrastructure, TDbContext> Rbac { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, ISecurityContext, TInfrastructure, TDbContext> SecurityContext { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IUserService, TInfrastructure, TDbContext> UserService { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, ITranscriptionService, TInfrastructure, TDbContext> TranscriptionService { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IJobQueue, TInfrastructure, TDbContext> JobQueue { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IQueuedJobStatusService, TInfrastructure, TDbContext> QueuedJobStatus { get; }

        public ValueResolver<TestBuilder<TInfrastructure, TDbContext>, IHttpClientFactory, TInfrastructure, TDbContext> HttpClientFactory { get; }

        #endregion
    }
}
