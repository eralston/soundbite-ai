using Masticore;
using Masticore.Models;
using Soundbite.Models;
using System.Threading.Tasks;

namespace Soundbite
{
    public interface IOrgEmailOptions
    {
        string OrgImageUrl { get; set; }
    }

    public class MessageModel
    {
        public User Receiver { get; set; }
        public string Preview { get; set; }
        public IOrgEmailOptions Options { get; set; }
    }

    public class UserInviteModel : MessageModel
    {
        public User Sender { get; set; }
    }

    public class PersonInviteModel : UserInviteModel
    {
        public Organization Org { get; set; }
        public bool IsAppInvite { get; set; }
        public bool IsMsTeamsEnabled { get; set; }
        public string MsTeamsInfoLink { get; set; }
    }

    public class MemberInviteModel : PersonInviteModel
    {
        public Group Group { get; set; }
    }

    public class SessionModel : MessageModel
    {
        public Organization Org { get; set; }
        public Session Session { get; set; }
        public User Sender { get; set; }
        public bool IsMsTeamsEnabled { get; set; }
        public string MsTeamsActionLink { get; set; }
        public string MsTeamsInfoLink { get; set; }
    }

    public class SessionPublishModel : SessionModel
    {
        public string PublicLink { get; set; }
    }

    /// <summary>
    /// Interface that defines the various email body content generation method required in the application.
    /// </summary>
    public interface IEmailTemplates
    {
        Task<Body> WelcomeAsync(User user);
        Task<Body> UserInviteAsync(UserInviteModel invite);
        Task<Body> PersonInviteAsync(PersonInviteModel model);
        Task<Body> MemberInviteAsync(MemberInviteModel model);
        Task<Body> SessionReminderAsync(SessionModel model);
        Task<Body> SessionPublishAsync(SessionPublishModel model);
        Task<Body> SessionHostPublishAsync(SessionPublishModel model);
    }
}