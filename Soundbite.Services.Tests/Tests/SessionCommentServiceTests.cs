using Masticore.Entity;
using Masticore.Entity.Tests;
using Masticore.Exceptions;
using Masticore.Models;
using Masticore.Services;
using Microsoft.EntityFrameworkCore;
using Soundbite.Entity;
using Soundbite.Entity.Tests;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Soundbite.Services.Tests
{
    public class SessionCommentServiceTests : ServiceTestBase
    {
        #region Utility Methods

        public SessionCommentServiceTests()
        {
            Builder.UseNullLogger = true;

            Builder.SessionCommentService.Use(i => new SessionCommentService(
                Builder.Rbac.Value,
                Builder.Infrastructure.Value,
                Builder.Mapper.Value,
                Builder.Logger<SessionCommentService>(),
                Builder.ImageService.Value
                ));

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

        private ISessionCommentService CommentService()
        {
            return Builder.SessionCommentService.Value;
        }

        private PersonEntity CurrentPerson(SbDb db)
        {
            return db.People.Where(p =>
                    p.User.Id == Builder.SecurityContext.Value.CurrentUserId
                    && p.OrganizationId == IdentityDbSeed.OrgPrimary.Id)
                .Include(p => p.User)
                .FirstOrDefault();
        }

        private async Task<SessionComment> CreateComment(ISessionCommentService comments, NewSessionComment newComment)
        {
            SessionComment comment = await comments.CreateAsync(
                IdentityDbSeed.OrgPrimary.Route,
                SbDbSeed.SessionRoute,
                newComment);
            return comment;
        }

        private static async Task DisallowCommmentsOnSession(SbDb db)
        {
            SessionEntity session = db.Sessions.Where(s => s.Route == SbDbSeed.SessionRoute).Single();
            session.SessionCommentPolicy = Masticore.SessionCommentPolicy.Disallowed;
            await db.SaveChangesAsync();
        }

        private static async Task DisallowCommmentsOnOrg(SbDb db)
        {
            OrganizationEntity org = await db.OrgWhereRoute(IdentityDbSeed.OrgPrimary.Route);
            OrgSettings settings = org.GetSettings() ?? new OrgSettings();
            settings.Sessions ??= new OrgSessionSettings();
            settings.Sessions.SessionCommentsEnabled = false;
            org.SetSettings(settings);
            await db.SaveChangesAsync();
        }

        #endregion

        // Create

        [Fact]
        public async Task CreateAsync()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            SbDb db = Builder.DbContext.Value;

            PersonEntity currentPerson = CurrentPerson(db);
            Assert.NotNull(currentPerson);

            string commentText = "Example Commment";
            NewSessionComment newComment = new NewSessionComment
            {
                Content = commentText
            };

            ISessionCommentService comments = CommentService();

            // ACT
            SessionComment comment = await CreateComment(comments, newComment);

            // ASSERT
            Assert.NotNull(comment);
            Assert.Equal(commentText, comment.Content);
            Assert.Equal(currentPerson.Route, comment.Person.Route);
            Assert.Equal(currentPerson.User.Route, comment.Person.User.Route);
        }

        [Fact]
        public async Task CreateAsync_DisallowedOnSession()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            SbDb db = Builder.DbContext.Value;

            await DisallowCommmentsOnSession(db);

            PersonEntity currentPerson = CurrentPerson(db);
            Assert.NotNull(currentPerson);


            string commentText = "Example Commment";
            NewSessionComment newComment = new NewSessionComment
            {
                Content = commentText
            };

            ISessionCommentService comments = CommentService();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ConflictException>(async () =>
            {
                // This isn't possible because the session itself disallows comments
                await CreateComment(comments, newComment);
            });
        }

        [Fact]
        public async Task CreateAsync_DisallowedOnOrg()
        {
            // ARRANGE
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            SbDb db = Builder.DbContext.Value;

            await DisallowCommmentsOnOrg(db);

            PersonEntity currentPerson = CurrentPerson(db);
            Assert.NotNull(currentPerson);


            string commentText = "Example Commment";
            NewSessionComment newComment = new NewSessionComment
            {
                Content = commentText
            };

            ISessionCommentService comments = CommentService();

            // ACT & ASSERT
            await Assert.ThrowsAsync<ForbiddenException>(async () =>
            {
                // This isn't possible because the org disallows comments
                await CreateComment(comments, newComment);
            });
        }

        // Read

        [Fact]
        public async Task ReadAllAsync()
        {
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            SbDb db = Builder.DbContext.Value;

            PersonEntity currentPerson = CurrentPerson(db);
            Assert.NotNull(currentPerson);

            ISessionCommentService comments = CommentService();

            await CreateComment(comments, new NewSessionComment { Content = "Example 1" });
            await CreateComment(comments, new NewSessionComment { Content = "Example 2" });
            await CreateComment(comments, new NewSessionComment { Content = "Example 3" });

            // Disallowing comments on the session should not matter to a read action
            await DisallowCommmentsOnSession(db);

            // ACT
            IndexPageResponse<SessionComment> page = await comments.ReadAllAsync(
                IdentityDbSeed.OrgPrimary.Route,
                SbDbSeed.SessionRoute);

            // ASSERT
            Assert.NotNull(page);
            Assert.NotNull(page.Request);
            Assert.Equal(3, page.Result.Count());
        }

        // Update

        [Fact]
        public async Task UpdateAsync()
        {
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            SbDb db = Builder.DbContext.Value;

            PersonEntity currentPerson = CurrentPerson(db);
            Assert.NotNull(currentPerson);

            ISessionCommentService comments = CommentService();

            string originalText = "Example 1";
            string updatedText = "New Example Content";
            SessionComment comment = await CreateComment(comments, new NewSessionComment { Content = originalText });
            Assert.NotNull(comment);
            Assert.Equal(originalText, comment.Content);

            // ACT
            SessionComment updatedComment = await comments.UpdateAsync(
                IdentityDbSeed.OrgPrimary.Route,
                SbDbSeed.SessionRoute,
                comment.Route,
                new NewSessionComment { Content = updatedText });

            // ASSERT
            Assert.NotNull(updatedComment);
            Assert.Equal(updatedText, updatedComment.Content);
        }

        // Delete

        [Fact]
        public async Task DeleteAsync()
        {
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            SbDb db = Builder.DbContext.Value;

            PersonEntity currentPerson = CurrentPerson(db);
            Assert.NotNull(currentPerson);

            ISessionCommentService comments = CommentService();
            SessionComment comment = await CreateComment(comments, new NewSessionComment { Content = "Example 1" });
            Assert.NotNull(comment);
            Assert.NotNull(comment.Content);

            // Ensure single comment reads
            IndexPageResponse<SessionComment> page = await comments.ReadAllAsync(
               IdentityDbSeed.OrgPrimary.Route,
               SbDbSeed.SessionRoute);
            Assert.Single(page.Result);

            // ACT
            await comments.DeleteAsync(
                IdentityDbSeed.OrgPrimary.Route,
                SbDbSeed.SessionRoute,
                comment.Route);

            // ASSERT

            // Read should now have nothing to see
            page = await comments.ReadAllAsync(
               IdentityDbSeed.OrgPrimary.Route,
               SbDbSeed.SessionRoute);
            Assert.Empty(page.Result);

            // Cannot update it
            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                SessionComment updatedComment = await comments.UpdateAsync(
               IdentityDbSeed.OrgPrimary.Route,
               SbDbSeed.SessionRoute,
               comment.Route,
               new NewSessionComment { Content = "New Text Here" });
            });

            // Cannot delete it again
            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await comments.DeleteAsync(
                   IdentityDbSeed.OrgPrimary.Route,
                   SbDbSeed.SessionRoute,
                   comment.Route);
            });
        }

        // Mixed

        [Fact]
        public async Task UpdateAsyncAndDeleteAsync_DisallowedOnSession()
        {
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            SbDb db = Builder.DbContext.Value;

            PersonEntity currentPerson = CurrentPerson(db);
            Assert.NotNull(currentPerson);

            ISessionCommentService comments = CommentService();

            string originalText = "Example 1";
            string updatedText = "New Example Content";
            SessionComment comment = await CreateComment(comments, new NewSessionComment { Content = originalText });
            Assert.NotNull(comment);
            Assert.Equal(originalText, comment.Content);

            await DisallowCommmentsOnSession(db);

            // ACT & ASSERT
            await Assert.ThrowsAsync<ConflictException>(async () =>
            {
                // This isn't possible because the session itself disallows comments
                await comments.UpdateAsync(
                   IdentityDbSeed.OrgPrimary.Route,
                   SbDbSeed.SessionRoute,
                   comment.Route,
                   new NewSessionComment { Content = updatedText });
            });

            await Assert.ThrowsAsync<ConflictException>(async () =>
            {
                // This isn't possible because the session itself disallows comments
                await comments.DeleteAsync(
                   IdentityDbSeed.OrgPrimary.Route,
                   SbDbSeed.SessionRoute,
                   comment.Route);
            });
        }

        [Fact]
        public async Task UpdateAsyncAndDeleteAsync_DisallowedOnOrg()
        {
            await Builder.SetCurrentUserContext(TestUserContext.OrgMember);
            SbDb db = Builder.DbContext.Value;

            PersonEntity currentPerson = CurrentPerson(db);
            Assert.NotNull(currentPerson);

            ISessionCommentService comments = CommentService();

            string originalText = "Example 1";
            string updatedText = "New Example Content";
            SessionComment comment = await CreateComment(comments, new NewSessionComment { Content = originalText });
            Assert.NotNull(comment);
            Assert.Equal(originalText, comment.Content);

            await DisallowCommmentsOnOrg(db);

            // ACT & ASSERT
            await Assert.ThrowsAsync<ForbiddenException>(async () =>
            {
                // This isn't possible because the session itself disallows comments
                await comments.UpdateAsync(
                   IdentityDbSeed.OrgPrimary.Route,
                   SbDbSeed.SessionRoute,
                   comment.Route,
                   new NewSessionComment { Content = updatedText });
            });

            await Assert.ThrowsAsync<ForbiddenException>(async () =>
            {
                // This isn't possible because the session itself disallows comments
                await comments.DeleteAsync(
                   IdentityDbSeed.OrgPrimary.Route,
                   SbDbSeed.SessionRoute,
                   comment.Route);
            });
        }
    }
}
