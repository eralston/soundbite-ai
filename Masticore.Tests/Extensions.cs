using Masticore.Models;
using Masticore.Resources;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Xunit;

namespace Masticore.Tests
{
    /// <summary>
    /// The modes for comparing <see cref="IUniversal"/> objects
    /// </summary>
    public enum UniversalIdCompareMode
    {
        /// <summary>
        /// Completely skip
        /// </summary>
        Ignore,

        /// <summary>
        /// String values must be the same
        /// Groups should usually have the same
        /// </summary>
        Equal,

        /// <summary>
        /// String values must NOT be the same
        /// Users should usually NOT have the same
        /// </summary>
        NotEqual
    }

    /// <summary>
    /// Extension methods for Masticore that help with testing
    /// </summary>
    public static class Extensions
    {
        public static void AssertUserEqual(this IUserFields expected, IUserFields actual, UniversalIdCompareMode uidCompare = UniversalIdCompareMode.NotEqual)
        {
            Assert.Equal(expected.Email.ToEmail(), actual.Email);
            Assert.Equal(expected.FamilyName, actual.FamilyName);
            Assert.Equal(expected.GivenName, actual.GivenName);
            Assert.Equal(expected.Phone, actual.Phone);
            Assert.Equal(expected.Title, actual.Title);

            AssertUidEqual(expected, actual, uidCompare);
        }

        public static void AssertUidEqual(IUniversal expected, IUniversal actual, UniversalIdCompareMode uidCompare)
        {
            if (uidCompare == UniversalIdCompareMode.Equal)
            {
                Assert.Equal(expected.UniversalId, actual.UniversalId);
            }
            else if (uidCompare == UniversalIdCompareMode.NotEqual)
            {
                Assert.NotEqual(expected.UniversalId, actual.UniversalId);
            }
        }

        public static void AssertGroupEqual(this IGroupFields expected, IGroupFields actual, UniversalIdCompareMode uidCompare = UniversalIdCompareMode.Equal)
        {
            Assert.NotNull(expected.UniversalId);
            Assert.NotNull(actual.UniversalId);

            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Description, actual.Description);

            AssertUidEqual(expected, actual, uidCompare);
        }

        /// <summary>
        /// Creates a new <see cref="IHttpClientFactory"/> for testing
        /// </summary>
        /// <returns></returns>
        public static IHttpClientFactory CreateHttpClientFactory()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddHttpClient();

            IHttpClientFactory factory = services
                .BuildServiceProvider()
                .GetRequiredService<IHttpClientFactory>();
            return factory;
        }
    }
}
