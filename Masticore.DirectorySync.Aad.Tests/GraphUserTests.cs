using Masticore.Ad;
using Xunit;

namespace Masticore.DirectorySync.Aad.Tests
{
    public class GraphUserTests
    {
        /// <summary>
        /// Test the parsing of a GraphUser from JSON that has no alt emails
        /// </summary>
        [Fact]
        public void ParseBasicUser()
        {
            // JSON for a user with regular email and not alt emails
            string userJson1 = $"{{ \"surname\": \"User\", \"givenName\": \"Guest Gmail\", \"mail\": \"bob.smith@soundbite.ai\", \"displayName\": \"bob smith\", \"userPrincipalName\": \"bob.smith_gmail.com#EXT#@sounbite.onmicrosoft.com\", \"id\": \"1ce2c3fb-a919-4cf8-8a20-41aea11bc2bc\" }}";
            // JSON parse userJson1 into GraphUser object using newtonsoft json
            GraphUser user = JsonUtils.FromJson<GraphUser>(userJson1);
            Assert.Equal("User", user.FamilyName);
            Assert.Equal("Guest Gmail", user.GivenName);
            Assert.Equal("bob.smith@soundbite.ai", user.Email);
            Assert.Null(user.ProxyEmails);
            // The e-mails should only have unique e-mail addresses with no fluff in them
            string[] emails = user.AliasEmails;
            Assert.Single(emails);
            Assert.Equal(new string[] { "bob.smith@soundbite.ai" }, emails);
        }

        /// <summary>
        /// Test the parsing of a GraphUser from JSON that has all fields
        /// </summary>
        [Fact]
        public void ParseCompleteUser()
        {
            // JSON for a user with regular email, plus another alternative email and a repeat in the alt
            string userJson1 = $"{{ \"surname\": \"User\", \"givenName\": \"Guest Gmail\", \"mail\": \"bob.smith@soundbite.ai\", \"displayName\": \"bob smith\", \"userPrincipalName\": \"bob.smith_gmail.com#EXT#@sounbite.onmicrosoft.com\", \"proxyAddresses\": [\"SMTP:bob.smith@gmail.com\",\"smtp:bob.smith@soundbite.ai\"], \"id\": \"1ce2c3fb-a919-4cf8-8a20-41aea11bc2bc\" }}";
            // JSON parse userJson1 into GraphUser object using newtonsoft json
            GraphUser user = JsonUtils.FromJson<GraphUser>(userJson1);
            Assert.Equal("User", user.FamilyName);
            Assert.Equal("Guest Gmail", user.GivenName);
            Assert.Equal("bob.smith@soundbite.ai", user.Email);
            Assert.Equal(new string[] { "SMTP:bob.smith@gmail.com", "smtp:bob.smith@soundbite.ai" }, user.ProxyEmails);
            // The e-mails should only have unique e-mail addresses with no fluff in them
            string[] emails = user.AliasEmails;
            Assert.Equal(2, emails.Length);
            Assert.Equal(new string[] { "bob.smith@soundbite.ai", "bob.smith@gmail.com" }, emails);
        }

        /// <summary>
        /// Test the parsing of a GraphUser from JSON that represents too many aliases from an old scummy AD instance
        /// </summary>
        [Fact]
        public void ParseOvergrownUser()
        {
            // JSON for a user with regular email, plus another alternative email and a repeat in the alt
            string userJson1 = $"{{\"businessPhones\":[\"+1234567\"],\"givenName\":\"Who\",\"surname\":\"Dis\",\"mail\":\"Who_Dis@cable.Acme.com\",\"jobTitle\":\"Executive Director, Market & Growth Strategist\",\"mobilePhone\":\"+1 (123) 316-7890\",\"userPrincipalName\":\"wDis200@cable.Acme.com\",\"proxyAddresses\":[\"X500:/O=Acme/ou=External (FYD123456)/cn=Recipients/cn=123abc456def789xyz\",\"x500:/O=Acme/OU=CABLE-CDC/cn=Recipients/cn=rDis200\",\"X500:/o=ExchangeLabs/ou=Exchange Administrative Group (FYD123456)/cn=Recipients/cn=8bbc654a321-Dis, Who\",\"smtp:wDis200@Acmecorp.mail.onmicrosoft.com\",\"smtp:Who_Dis@c.Acme.com\",\"smtp:wDis200@cable.Acme.com\",\"smtp:Who_Dis@Acme.com\",\"smtp:wDis200@Acmecorp.onmicrosoft.com\",\"SMTP:Who_Dis@cable.Acme.com\"],\"id\":\"aad12f46-591c-46d1-b968-955a5549ce94\",\"@odata.type\":\"#microsoft.graph.user\",\"displayName\":\"Dis, Who\",\"@removed\":null,\"deletedDateTime\":null}}";
            // JSON parse userJson1 into GraphUser object using newtonsoft json
            GraphUser user = JsonUtils.FromJson<GraphUser>(userJson1);
            Assert.Equal("Dis", user.FamilyName);
            Assert.Equal("Who", user.GivenName);
            Assert.Equal("Who_Dis@cable.Acme.com", user.Email);
            string[] expectedAlts = new string[] { "X500:/O=Acme/ou=External (FYD123456)/cn=Recipients/cn=123abc456def789xyz", "x500:/O=Acme/OU=CABLE-CDC/cn=Recipients/cn=rDis200", "X500:/o=ExchangeLabs/ou=Exchange Administrative Group (FYD123456)/cn=Recipients/cn=8bbc654a321-Dis, Who", "smtp:wDis200@Acmecorp.mail.onmicrosoft.com", "smtp:Who_Dis@c.Acme.com", "smtp:wDis200@cable.Acme.com", "smtp:Who_Dis@Acme.com", "smtp:wDis200@Acmecorp.onmicrosoft.com", "SMTP:Who_Dis@cable.Acme.com" };
            Assert.Equal(expectedAlts, user.ProxyEmails);
            // The e-mails should only have unique e-mail addresses with no fluff in them
            string[] emails = user.AliasEmails;
            Assert.Equal(6, emails.Length);
            string[] expectedEmails =
                new string[] {
                    "who_dis@cable.acme.com",
                    "wdis200@acmecorp.mail.onmicrosoft.com",
                    "who_dis@c.acme.com",
                    "wdis200@cable.acme.com",
                    "who_dis@acme.com",
                    "wdis200@acmecorp.onmicrosoft.com"
                 };
            Assert.Equal(expectedEmails, emails);
        }
    }
}
