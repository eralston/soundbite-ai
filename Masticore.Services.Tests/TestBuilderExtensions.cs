using Masticore.Entity;
using Masticore.Entity.Tests;
using Microsoft.EntityFrameworkCore;

namespace Masticore.Services.Tests
{
    public static class TestBuilderExtensions
    {
        public static MockNotifications GetMockNotificationService<TInfrastructure, TDbContext>(this TestBuilder<TInfrastructure, TDbContext> testBuilder)
            where TDbContext : DbContext
            where TInfrastructure : class, IDbInfrastructure<TDbContext>, new()
        {
            return testBuilder.NotificationService.Value as MockNotifications;
        }

        public static MockImageService GetMockImageService<TInfrastructure, TDbContext>(this TestBuilder<TInfrastructure, TDbContext> testBuilder)
            where TDbContext : DbContext
            where TInfrastructure : class, IDbInfrastructure<TDbContext>, new()
        {
            return testBuilder.ImageService.Value as MockImageService;
        }
    }
}
