using Masticore.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

//NOTE - cert based auth - https://medium.com/dev-genius/jwt-authentication-in-asp-net-core-e67dca9ae3e8

namespace Masticore.Tests
{
    public class TokenServiceTests : TestBase
    {
        #region Methods

        private Claim[] BuildClaims()
        {
            return new Claim[] {
                new Claim("id", "someId")
            };
        }

        private ITokenServiceSettings GetTokenSecurityConfig_SecretKey()
        {
            return new TokenServiceSettings()
            {
                SecurityType = TokenSecurityType.SecretKey,
                Config = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("uiRMaXwtz5lGrcxzooncnnzDYhzzBrXAoNJGKOIsix2ZclCqGAJLQERXQNR5JI0l")),
                ClockSkewInSeconds = 0,
                TokenTimeoutInMinutes = 60 * 24 * 365 * 15
            };
        }

        #endregion

        #region ValidateToken

        [Fact]
        public void GenerateToken_SecretKey()
        {
            // ARRANGE
            TokenServiceSettings.TokenIssuerClaimValue = "https://soundbite";
            TokenServiceFactory factory = new TokenServiceFactory(null);
            ITokenService service = factory.GetTokenService(GetTokenSecurityConfig_SecretKey());
            Claim[] claims = BuildClaims();

            // ACT
            string token = service.GenerateToken(claims);
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken tokenInfo = service.GetTokenValidated(token);

            // ASSERT
            Assert.Equal(3, token.Split('.').Length);
            Assert.NotNull(tokenInfo);
            claims.ForEach(claim =>
            {
                Assert.NotNull(tokenInfo.Claims.First(i => i.Type == claim.Type && i.Value == claim.Value));
            });
        }

        [Fact]
        public void ValidateToken_SecretKey_GetTokenValidated()
        {
            // ARRANGE
            TokenServiceSettings.TokenIssuerClaimValue = "https://soundbite";
            TokenServiceFactory factory = new TokenServiceFactory(null);
            ITokenService service = factory.GetTokenService(GetTokenSecurityConfig_SecretKey());
            Claim[] claims = BuildClaims();

            // NOTE - expiration on this token is set to 2031 so if it's failing due to expiration you need to regenerate a new token
            string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6InNvbWVJZCIsIm5iZiI6MTYzNDQ5MjUxMywiZXhwIjoxOTUwMDI1Mjg1LCJpYXQiOjE2MzQ0OTI1MTMsImlzcyI6Imh0dHBzOi8vc291bmRiaXRlIn0.ld_eD5HAjIyJtc6wZvQng7FHZLLsM764yKGxyW20cq4";

            // ACT
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken tokenInfo = service.GetTokenValidated(token);

            // ASSERT
            Assert.NotNull(token);
            Assert.Equal(3, token.Split('.').Length);
            claims.ForEach(claim =>
            {
                Assert.NotNull(tokenInfo.Claims.First(i => i.Type == claim.Type && i.Value == claim.Value));
            });

        }

        [Fact]
        public void ValidateToken_SecretKey_GetTokenValidated_Timeout()
        {
            // ARRANGE
            TokenServiceFactory factory = new TokenServiceFactory(null);
            ITokenService service = factory.GetTokenService(GetTokenSecurityConfig_SecretKey());
            string expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpZCI6InNvbWVJZCIsIm5iZiI6MTYwNTIzODE3OSwiZXhwIjoxNjA1MjM4MjM4LCJpYXQiOjE2MDUyMzgxNzl9.o25QMopONIeZ-3ge8WFlg6m6HV56vsMGHDlHAf2ahyc";

            // ACT
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken tokenInfo = service.GetTokenValidated(expiredToken);
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken tokenInfoNotValidated = service.GetToken(expiredToken);

            // ASSERT
            Assert.Null(tokenInfo);
            Assert.NotNull(tokenInfoNotValidated);
        }

        [Fact]
        public void ValidateToken_SecretKey_GetTokenValidated_HackedClaims()
        {
            // ARRANGE
            TokenServiceFactory factory = new TokenServiceFactory(null);
            ITokenService service = factory.GetTokenService(GetTokenSecurityConfig_SecretKey());
            Claim[] claims = BuildClaims();
            List<Claim> hackedClaims = new List<Claim>(claims)
            {
                new Claim("role", "superUser")
            };

            // ACT - simulate a hack by taking two good tokens and split out the claims from one and put them in another one
            string token = service.GenerateToken(claims);
            string token2 = service.GenerateToken(hackedClaims.ToArray());
            string[] tokenParts = token.Split('.');
            string hackedToken = $"{tokenParts[0]}.{token2.Split('.')[1]}.{tokenParts[2]}";
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken hackedTokenInfo = service.GetTokenValidated(hackedToken);
            System.IdentityModel.Tokens.Jwt.JwtSecurityToken hackedTokenInfoRead = service.GetToken(hackedToken);

            // ASSERT
            Assert.Equal(3, hackedToken.Split('.').Length);
            Assert.Null(hackedTokenInfo);
            Assert.NotNull(hackedTokenInfoRead);
            hackedClaims.ForEach(claim =>
            {
                Assert.NotNull(hackedTokenInfoRead.Claims.First(i => i.Type == claim.Type && i.Value == claim.Value));
            });
        }

        #endregion

    }
}