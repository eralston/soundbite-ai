using Soundbite.ClientSdk;

namespace Sandbox
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ITokenServiceSettings settings = new TokenServiceSettings()
            {
                ClockSkewInSeconds = 5,
                RefreshTokenTimeoutInMinutes = 60 * 24 * 90, /* 90 Days */
                SecurityType = TokenSecurityType.SecretKey,
                TokenTimeoutInMinutes = 60 * 24 * 90 /* 90 Days */,
                Config = ""
            };
            ITokenService tokenService = new TokenServiceSecretKey(settings);
            string token = tokenService.GenerateUserToken("", "");
            string t = token;
        }
    }
}
