using Masticore.Sms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Soundbite.Services.Tests.Mock
{
    public class MockSmsGateway : ISmsGateway
    {
        public int Count { get; protected set; }
        public List<Message> Messages = new List<Message>();
        public Task<string> SendAsync(Message sms)
        {
            Messages.Add(sms);
            ++Count;
            return Task.FromResult("SID GOES HERE");
        }
    }
}
