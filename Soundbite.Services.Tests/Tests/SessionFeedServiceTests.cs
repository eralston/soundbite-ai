using Masticore;
using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Models;
using Masticore.Security;
using Masticore.Services;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using Soundbite.Models;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    /// <summary>
    /// Test for <see cref="SessionFeedService"/> using the <see cref="SbDbSeed"/>
    /// </summary>
    public class SessionFeedServiceTests : ServiceTestBase
    {
        #region Supporting Methods

        public SessionFeedServiceTests()
        {
            Builder.UseNullLogger = true;
            Builder.SessionFeedService.Use(i => new SessionFeedService(
                Builder.Rbac.Value,
                Builder.Infrastructure.Value,
                Builder.Logger<SessionFeedService>(),
                Builder.Mapper.Value));

            Builder.SessionService.Use(i => new SessionService(
                Builder.Rbac.Value,
                Builder.SeriesService.Value,
                Builder.ImageService.Value,
                Builder.ClipFileService.Value,
                Builder.LifeCycleFactory.Value,
                Builder.Infrastructure.Value,
                Builder.ProviderFactory.Value,
                Builder.Mapper.Value,
                Builder.Logger<SessionService>()
                ));

            Builder.MemberService.Use(i => new MemberService(
                Builder.SecurityContext.Value,
                Builder.NotificationService.Value,
                Builder.ImageService.Value,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                Builder.Logger<MemberService>()));
        }

        private async Task InviteUserToGroup()
        {
            UserEntity user = await Builder.UserForRoute(IdentityDbSeed.UserRegular.Route);
            Invite[] invites = new Invite[]
            {
                new Invite { Token = user.Email }
            };

            SbDb db = Builder.DbContext.Value;
            IRbac rbac = Builder.Rbac.Value;
            int[] oldGroupIds = await db.GroupIdsAsync(IdentityDbSeed.OrgPrimary.Route, rbac.UniversalIdForCurrentUser);

            // ACT
            await Builder.SetCurrentUserContext(TestUserContext.GodUser);
            ISessionService sessions = Builder.SessionService.Value;
            SessionDetails details = await sessions.ReadAsync(IdentityDbSeed.OrgPrimary.Route, SbDbSeed.NonMemberSessionRoute);
            Assert.NotNull(details);

            IMemberService members = Builder.MemberService.Value;
            await members.InviteAsync(IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route, invites);
            await Builder.SetCurrentUserContext(TestUserContext.SessionParticipantNoGroupAffiliation);

            int[] newGroupIds = await db.GroupIdsAsync(IdentityDbSeed.OrgPrimary.Route, rbac.UniversalIdForCurrentUser);

            Assert.Equal(oldGroupIds.Length + 1, newGroupIds.Length);
        }

        #endregion

        #region Test Methods

        [Fact]
        public async Task ReadPendingAsync_Org_Admin()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            IndexPageResponse<SessionPreview> sessions = await sessionFeedService.ReadPendingAsync(
                IdentityDbSeed.OrgPrimary.Route);

            // ASSERT
            Assert.NotNull(sessions);
            Assert.Equal(2, sessions.Result.Count());
        }

        [Fact]
        public async Task ReadPendingAsync_Org_Person()
        {
            // ARRANGE
            // Regular user is associated to a single session by a participant record, but not group
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            IndexPageResponse<SessionPreview> sessions = await sessionFeedService.ReadPendingAsync(
                IdentityDbSeed.OrgPrimary.Route);

            // ASSERT
            Assert.NotNull(sessions);
            Assert.Single(sessions.Result);
        }

        [Fact]
        public async Task ReadPendingAsync_Org_NotPartOfOrg()
        {
            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.ExternalOrgMember);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await sessionFeedService.ReadPendingAsync(IdentityDbSeed.OrgPrimary.Route);
            });
        }

        [Fact]
        public async Task ReadPendingAsync_Group_Admin()
        {
            //TODO: The name of this test implies that the group admin can perform the action
            //      but we have to use the org admin because it was setup as the group admin
            //      for the test. I assume the normal group admin would work but the test data
            //      needs to be setup for it.  At some point we should do this to allow for
            //      testing with a non-admin level group owner.

            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            IndexPageResponse<SessionPreview> sessions = await sessionFeedService.ReadPendingAsync(
                IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);

            // ASSERT
            Assert.NotNull(sessions);
            // Admin has access to everything
            Assert.Single(sessions.Result);
        }

        [Fact]
        public async Task ReadPendingAsync_Group_NotPartOfOrg()
        {
            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.ExternalOrgMember);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await sessionFeedService.ReadPendingAsync(
                      IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            });
        }

        [Fact]
        public async Task ReadPendingAsync_Group_NotPartOfGroup()
        {
            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.OrgMemberNotGroupMember);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await sessionFeedService.ReadPendingAsync(
                      IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            });
        }

        [Fact]
        public async Task ReadPastAsync_Org_Admin()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            IndexPageResponse<SessionPreview> sessions = await sessionFeedService.ReadPastAsync(
                IdentityDbSeed.OrgPrimary.Route);

            // ASSERT
            Assert.NotNull(sessions);
            Assert.Single(sessions.Result);
        }

        [Fact]
        public async Task ReadPastAsync_Org_Person()
        {
            // ARRANGE
            // Regular user is associated to a single session by a participant record, but not group
            await Builder.SetCurrentUserContext(TestUserContext.SessionParticipantNoGroupAffiliation);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            IndexPageResponse<SessionPreview> sessions = await sessionFeedService.ReadPastAsync(
                IdentityDbSeed.OrgPrimary.Route);

            // ASSERT
            Assert.NotNull(sessions);
            Assert.Single(sessions.Result);
        }

        [Fact]
        public async Task ReadPastAsync_Org_NotPartOfOrg()
        {
            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.ExternalOrgMember);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await sessionFeedService.ReadPastAsync(IdentityDbSeed.OrgPrimary.Route);
            });
        }

        [Fact]
        public async Task ReadPastAsync_Group_Admin()
        {
            //TODO: The name of this test implies that the group admin can perform the action
            //      but we have to use the org admin because it was setup as the group admin
            //      for the test. I assume the normal group admin would work but the test data
            //      needs to be setup for it.  At some point we should do this to allow for
            //      testing with a non-admin level group owner.

            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.OrgAdmin);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            IndexPageResponse<SessionPreview> sessions = await sessionFeedService.ReadPublishedAsync(
                IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);

            // ASSERT
            Assert.NotNull(sessions);
            // Admin has access to everything
            Assert.Single(sessions.Result);
        }

        [Fact]
        public async Task ReadPastAsync_Group_NotPartOfOrg()
        {
            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.ExternalOrgMember);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await sessionFeedService.ReadPublishedAsync(
                      IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupOrgAdminOwned.Route);
            });
        }

        [Fact]
        public async Task ReadPastAsync_Group_NotPartOfGroup()
        {
            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.OrgMemberNotGroupMember);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;

            // ACT
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await sessionFeedService.ReadPublishedAsync(
                      IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            });
        }

        [Fact]
        public async Task ReadPastAsync_Group_AddedLaterToGroup()
        {
            // ARRANGE
            // There is one session on the given group, default user is god/admin
            await Builder.SetCurrentUserContext(TestUserContext.SessionParticipantNoGroupAffiliation);
            ISessionFeedService sessionFeedService = Builder.SessionFeedService.Value;
            ISessionService sessions = Builder.SessionService.Value;

            // #1 Verify accessing the feed for the group is locked out
            await Assert.ThrowsAsync<SecurityException>(async () =>
            {
                await sessionFeedService.ReadPublishedAsync(
                      IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);
            });

            SbDb db = Builder.DbContext.Value;
            SessionEntity groupSession = db.Sessions.Where(s => s.Route == SbDbSeed.NonMemberSessionRoute).Single();
            groupSession.PublishSent = Time.UtcNow;
            db.SaveChanges();

            await InviteUserToGroup();

            // ACT
            // Check feed again, it should return prior sessions now
            IndexPageResponse<SessionPreview> historyFeed = await sessionFeedService.ReadPublishedAsync(
                      IdentityDbSeed.OrgPrimary.Route, IdentityDbSeed.GroupMemberOwned.Route);

            // ASSERT

            Assert.NotNull(historyFeed);
            Assert.Single(historyFeed.Result);
        }

        #endregion
    }
}
