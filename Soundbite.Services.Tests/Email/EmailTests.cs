// TODO: FIGURE OUT WHY THESE UNIT TEST CAN'T RUN RAZOR WHEN BOTH AZFUN AND API PROJECTS CAN

//Message:
//System.TypeLoadException : Could not load type 'Microsoft.AspNetCore.Razor.Language.Extensions.NamespaceDirective' from assembly 'Microsoft.AspNetCore.Razor.Language, Version=2.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'.
//  Stack Trace: 
//    <> c__DisplayClass1_0.< get_Instance > b__0(RazorProjectEngineBuilder builder)
//    RazorProjectEngine.Create(RazorConfiguration configuration, RazorProjectFileSystem fileSystem, Action`1 configure)
//    DefaultRazorEngine.get_Instance() line 16
//    RazorLightEngineBuilder.Build() line 355
//    RazorLightTemplate.Init(Assembly entryAssembly, Assembly viewAssembly, String rootNamespace) line 25
//    EmailTemplates.InitWithRazorLight(Assembly entryAssembly) line 17
//    EmailTests.TemplatesTest() line 30
//    -- - End of stack trace from previous location where exception was thrown ---

//using Masticore.Entity;
//using Masticore.Mail;
//using Masticore.Models;
//using Masticore.Services.Tests.Mock;
//using Soundbite.Entity;
//using Soundbite.Messaging;
//using Soundbite.Models;
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;

//namespace Soundbite.Services.Tests
//{
//    public class EmailTests : ServiceTestBase
//    {
//        private async Task RenderAndAssert(Func<Task<Body>> renderFunc)
//        {
//            Body body = await renderFunc();
//            Assert.NotNull(body);
//            Assert.NotNull(body.Html);
//            Assert.NotNull(body.Text);
//        }

//        [Fact]
//        public async Task TemplatesTest()
//        {
//            // ARRANGE
//            EmailTemplates.InitWithRazorLight(typeof(EmailTests).Assembly);
//            EmailTemplates templates = new EmailTemplates(new RazorLightTemplate());

//            SbDb db = await Infrastructure.DbAsync();
//            User user1 = MockMapper.Instance.Map<User>(db.Users.First());
//            User user2 = MockMapper.Instance.Map<User>(db.Users.Skip(1).First());
//            OrganizationEntity orgEntity = db.Organizations.First();
//            Organization org = MockMapper.Instance.Map<Organization>(orgEntity);
//            GroupEntity groupEntity = db.Groups.First();
//            Group group = MockMapper.Instance.Map<Group>(groupEntity);
//            SessionEntity sessionEntity = db.Sessions.First();
//            Session session = MockMapper.Instance.Map<Session>(sessionEntity);

//            // ACT & ASSERT
//            await RenderAndAssert(async () => { return await templates.MemberInviteAsync(user1, user2, org, group, true); });
//            await RenderAndAssert(async () => { return await templates.PersonInviteAsync(user1, user2, org, true); });
//            await RenderAndAssert(async () => { return await templates.SessionPublishAsync(user1, org, session); });
//            await RenderAndAssert(async () => { return await templates.SessionReminderAsync(user1, org, session); });
//            await RenderAndAssert(async () => { return await templates.UserInvite(user1, user2); });
//            await RenderAndAssert(async () => { return await templates.UserJoinAsync(user1); });
//            await RenderAndAssert(async () => { return await templates.WelcomeAsync(user1); });
//        }
//    }
//}
