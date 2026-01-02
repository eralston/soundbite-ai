using Masticore.Resources;
using Xunit;

namespace Masticore.Tests
{
    /// <summary>
    /// Base class for asserting resources
    /// </summary>
    public abstract class TestBase
    {
        public void AssertResource(IResource resource)
        {
            Assert.NotNull(resource);
            Assert.NotNull(resource.Route);
            Assert.Equal(ResourceExtensions.RouteLength, resource.Route.Length);
            Assert.NotEqual(default, resource.CreatedUtc);
            Assert.NotEqual(default, resource.UpdatedUtc);
        }
    }
}
