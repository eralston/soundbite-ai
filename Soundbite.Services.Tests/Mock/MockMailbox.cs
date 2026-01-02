using Masticore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    public class MockMailbox : IMailbox
    {
        public List<MailMessage> Mail { get; set; } = new List<MailMessage>();

        public int Count { get; protected set; }
        public Task SendAsync(MailMessage mail)
        {
            Mail.Add(mail);
            ++Count;
            return Task.CompletedTask;
        }

        public async Task SendForOrgAsync(MailMessage mail, string orgRoute)
        {
            await SendAsync(mail);
        }
    }
}
