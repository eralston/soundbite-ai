using Masticore;
using Masticore.Mail;
using Masticore.Models;
using Soundbite.Resources;
using System.Reflection;
using System.Threading.Tasks;

namespace Soundbite.Messaging
{
    /// <summary>
    /// A collection of templates that can render on-demand
    /// </summary>
    public class EmailTemplates : IEmailTemplates
    {
        public static void InitWithRazorLight(Assembly entryAssembly)
        {
            RazorLightTextTemplate.Init(entryAssembly, typeof(EmailTemplates).Assembly, "Soundbite.Messaging.Templates");
        }

        protected ITextTemplate TextTemplate { get; }

        public EmailTemplates(ITextTemplate textTemplate)
        {
            TextTemplate = textTemplate;
        }

        private async Task<Body> RenderAsync<TModel>(string viewName, TModel model)
        {
            return new Body
            {
                Html = await TextTemplate.RenderAsync($"{viewName}.Html", model),
                Text = await TextTemplate.RenderAsync($"{viewName}.Text", model),
            };
        }

        public async Task<Body> WelcomeAsync(User user)
        {
            return await RenderAsync("UserWelcome", user);
        }

        public async Task<Body> UserInviteAsync(UserInviteModel model)
        {
            return await RenderAsync("UserInvite", model);
        }

        public async Task<Body> PersonInviteAsync(PersonInviteModel model)
        {
            return await RenderAsync("PersonInvite", model);
        }

        public async Task<Body> MemberInviteAsync(MemberInviteModel model)
        {
            return await RenderAsync("MemberInvite", model);
        }

        public async Task<Body> SessionReminderAsync(SessionModel model)
        {
            if (model.Preview == null)
            {
                string sessionTypeName = model.Session.TypeName();
                model.Preview = $"Please contributor to {sessionTypeName}:{model.Session.Name} in {model.Org.Name}";
            }

            return await RenderAsync("SessionReminder", model);
        }

        public async Task<Body> SessionPublishAsync(SessionPublishModel model)
        {
            if (model.Preview == null)
            {
                string sessionTypeName = model.Session.TypeName();
                model.Preview = $"{model.Session.Name} from {model.Sender.DisplayName}";
            }

            return await RenderAsync("SessionPublish", model);
        }

        public async Task<Body> SessionHostPublishAsync(SessionPublishModel model)
        {
            if (model.Preview == null)
            {
                string sessionTypeName = model.Session.TypeName();
                model.Preview = $"You successfully published {sessionTypeName}:{model.Session.Name} in {model.Org.Name}";
            }

            return await RenderAsync("SessionHostPublish", model);
        }
    }
}