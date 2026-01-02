using System;
using Soundbite.TokenGen;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateToken();
        }


        static void CreateToken()
        {
            string encKey = "F5zNU2lprVtl27orvdJBpClfggSqAQ4zgip884J5k332hTBYrHYGQGt7OG6zUJss";
            encKey = "SQDF9A34mIyHSVM2t51ePfgxXQVthaH9MSarsC9ryVSop8KTnfC4odvj6yqvZa0q";
            string orgRoute = "dkxsT7lE";
            string userEmail = "admin@ltapac.onmicrosoft.com";

            TokenServiceSettings settings = new TokenServiceSettings()
            {
                Config = encKey,                
                TokenTimeoutInMinutes = 60 * 24 * 365
            };
            ITokenService service = new TokenServiceSecretKey(settings);
            string token = service.GenerateToken("SomeIssuer", orgRoute, userEmail);
            Console.WriteLine(token);
            Console.ReadLine();


        }
    }
}
