using Masticore.Sms;
using Xunit;

namespace Soundbite.Messaging.Tests
{
    public class PhoneNumberValidationTests
    {
        [Fact]
        public void CheckAllFormats()
        {
            // Example formats for phone numbers and their expected behavior

            // Valid
            Assert.Equal("+15714178464", PhoneNumber.ToE164Format("(571) 417-8464"));
            Assert.Equal("+15714178464", PhoneNumber.ToE164Format("    (571) 417-8464     "));
            Assert.Equal("+19738700726", PhoneNumber.ToE164Format("973-870-0726"));
            Assert.Equal("+18573669931", PhoneNumber.ToE164Format("18573669931"));
            Assert.Equal("+19172846432", PhoneNumber.ToE164Format("917.284.6432"));
            Assert.Equal("+16468438351", PhoneNumber.ToE164Format("+1 (646) 843-8351"));
            Assert.Equal("+12027280044", PhoneNumber.ToE164Format("+1 202-728-0044 x145"));

            // Invalid
            Assert.Null(PhoneNumber.ToE164Format(""));
            Assert.Null(PhoneNumber.ToE164Format(null));
            Assert.Throws<PhoneNumberParseException>(() => PhoneNumber.ToE164Format(" "));
            Assert.Throws<PhoneNumberParseException>(() => PhoneNumber.ToE164Format("HELLO"));
            Assert.Throws<PhoneNumberParseException>(() => PhoneNumber.ToE164Format("441737378310"));
            Assert.Throws<PhoneNumberParseException>(() => PhoneNumber.ToE164Format("1-973-321-36XX"));
            Assert.Throws<PhoneNumberParseException>(() => PhoneNumber.ToE164Format("-8822"));
        }
    }
}
