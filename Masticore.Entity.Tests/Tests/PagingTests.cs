using Masticore.Models;
using Masticore.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Masticore.Entity.Tests
{
    public class PagingTests : ResourceTestBase<OrganizationEntity, MockIdentityDb, MockSeedInfrastructure>
    {
        private const int usersToCreate = 100;
        private const int take = 32;

        [Fact]
        public async Task Paging_LimitedPageTake()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Equal(IdentityDbSeed.SeededUserCount, db.Users.Count());
            for (int i = 0; i < usersToCreate; i++)
            {
                db.MockUser();
            }
            db.SaveChanges();
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, db.Users.Count());

            // ACT
            IndexPageResponse<UserEntity> response = await db.Users.ReadPageAsync(new IndexPageRequest { Take = take });

            Assert.Equal(take, response.Request.Take);
            Assert.Equal(take, response.Result.Count());
            Assert.Equal(IndexPageRequest.DefaultMinSkip, response.Request.Skip);
            Assert.Equal(IndexPageRequest.DefaultMaxTotalResults, response.MaxTotalResults);
            Assert.Equal(IndexPageRequest.DefaultMaxTake, response.MaxTake);
            Assert.Null(response.TotalCount);
            Assert.Null(response.TotalPageCount);
        }

        [Fact]
        public async Task Paging_CustomFirstPage()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Equal(IdentityDbSeed.SeededUserCount, db.Users.Count());
            for (int i = 0; i < usersToCreate; i++)
            {
                db.MockUser();
            }
            db.SaveChanges();
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, db.Users.Count());
            int take = 10;
            int skip = 0;
            IndexPageRequest request = new IndexPageRequest
            {
                Skip = skip,
                Take = take,
                IncludesCounts = true,
            };

            // ACT
            IndexPageResponse<UserEntity> response = await db.Users.ReadPageAsync(request);

            Assert.Equal(take, response.Request.Take);
            Assert.Equal(take, response.Result.Count());
            Assert.Equal(skip, response.Request.Skip);
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, response.TotalCount);
            Assert.Equal(11, response.TotalPageCount);
        }

        [Fact]
        public async Task Paging_CustomThirdPage()
        {
            // ARRANGE            
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Equal(IdentityDbSeed.SeededUserCount, db.Users.Count());
            for (int i = 0; i < usersToCreate; i++)
            {
                db.MockUser();
            }
            db.SaveChanges();
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, db.Users.Count());
            int take = 10;
            int skip = 20;
            IndexPageRequest request = new IndexPageRequest
            {
                Skip = skip,
                Take = take,
                IncludesCounts = true,
            };

            // ACT
            IndexPageResponse<UserEntity> response = await db.Users.ReadPageAsync(request);
            int expectedRequests = TestUtils.GetExpectedPageCount(usersToCreate + IdentityDbSeed.SeededUserCount, request.Take.Value);
            Assert.Equal(take, response.Request.Take);
            Assert.Equal(take, response.Result.Count());
            Assert.Equal(skip, response.Request.Skip);
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, response.TotalCount);
            Assert.Equal(expectedRequests, response.TotalPageCount);
        }

        [Fact]
        public async Task Paging_LimitedEach()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Equal(IdentityDbSeed.SeededUserCount, db.Users.Count());
            for (int i = 0; i < usersToCreate; i++)
            {
                db.MockUser();
            }
            db.SaveChanges();
            HashSet<string> existingRoutes = db.Users.Select(u => u.Route).ToHashSet();
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, db.Users.Count());
            int numberOfRequests = 0;
            int inLoopCount = 0;
            IndexPageResponse<UserEntity> response = null;
            List<UserEntity> fullResult = new List<UserEntity>();
            IndexPageRequest request = new IndexPageRequest { Take = take };
            // ACT
            do
            {
                // Accumulate info on each page
                response = await db.Users.ReadPageAsync(request);
                request = response.Request;
                fullResult.AddRange(response.Result);
                inLoopCount += response.Result.Count();
                // ASSERT - Each page should report certain values the same
                Assert.Null(response.TotalCount);
                Assert.Null(response.TotalPageCount);

                // To the next page
                numberOfRequests++;
            }
            while (response != null && response.HasMorePages() && request.NextPage());

            HashSet<string> fullResultRoutes = fullResult.Select(u => u.Route).ToHashSet();
            IEnumerable<string> routeIntersection = existingRoutes.Intersect(fullResultRoutes);

            // ASSERT
            int expectedLastPageCount = (usersToCreate + IdentityDbSeed.SeededUserCount) % take;
            expectedLastPageCount = expectedLastPageCount == 0 ? take : expectedLastPageCount;
            int expectedRequests = ((usersToCreate + IdentityDbSeed.SeededUserCount) / take) + (expectedLastPageCount == 0 ? 0 : 1);

            Assert.Equal(expectedLastPageCount, response.Result.Count()); // Verifies the last page item count
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, inLoopCount);
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, fullResult.Count);
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, routeIntersection.Count());
            Assert.Equal(expectedRequests, numberOfRequests); // Verifies the number of requests
        }

        [Fact]
        public async Task Paging_PerfectPageEach()
        {
            const int totalUsers = 160;
            int usersToCreate = totalUsers - IdentityDbSeed.SeededUserCount;

            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Equal(IdentityDbSeed.SeededUserCount, db.Users.Count());
            // Make a perfect power of MaxTake so we have an extra empty page
            for (int i = 0; i < usersToCreate; i++)
            {
                db.MockUser();
            }
            db.SaveChanges();
            HashSet<string> existingRoutes = db.Users.Select(u => u.Route).ToHashSet();
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, db.Users.Count());
            // Make take/skip to make sure that approach works
            int? take = 32;
            int? skip = null;
            int numberOfRequests = 0;
            int inLoopCount = 0;
            IndexPageResponse<UserEntity> response = null;
            List<UserEntity> fullResult = new List<UserEntity>();

            // ACT
            do
            {
                IndexPageRequest request = new IndexPageRequest
                {
                    Skip = skip,
                    Take = take,
                };

                // Accumulate info on each page
                response = await db.Users.ReadPageAsync(request);
                fullResult.AddRange(response.Result);
                inLoopCount += response.Result.Count();
                take = response.Request.Take;
                skip = response.Request.Skip;
                // ASSERT - Each page should report certain values the same
                Assert.Equal(take, response.Request.Take);
                Assert.Null(response.TotalCount);
                Assert.Null(response.TotalPageCount);

                // To the next page
                skip += response.Request.Take;
                numberOfRequests++;
            }
            while (response != null && response.HasMorePages());

            HashSet<string> fullResultRoutes = fullResult.Select(u => u.Route).ToHashSet();
            IEnumerable<string> routeIntersection = existingRoutes.Intersect(fullResultRoutes);

            // ASSERT
            // How many remainder records did we have?
            Assert.Empty(response.Result);
            Assert.Equal(totalUsers, inLoopCount);
            Assert.Equal(totalUsers, fullResult.Count);
            Assert.Equal(totalUsers, routeIntersection.Count());
            // How many requests did we make for the total pages?
            Assert.Equal(TestUtils.GetExpectedPageCount(totalUsers, take.Value), numberOfRequests);
        }

        [Fact]
        public async Task Paging_ExactlyTooMany()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Equal(IdentityDbSeed.SeededUserCount, db.Users.Count());

            // Pull out the max and double-check this unit test isn't out of date
            int maxItemsPaged = IndexPageRequest.DefaultMaxTotalResults;
            Assert.Equal(1024, maxItemsPaged);
            // Add in at least as many as the max page size
            for (int i = 0; i < maxItemsPaged; i++)
            {
                db.MockUser();
            }
            db.SaveChanges();
            Assert.Equal(1024 + IdentityDbSeed.SeededUserCount, db.Users.Count());

            int numberOfRequests = 0;
            int inLoopCount = 0;
            IndexPageRequest request = null;
            IndexPageResponse<UserEntity> response = null;
            List<UserEntity> fullResult = new List<UserEntity>();

            // ACT
            do
            {
                // Accumulate info on each page
                response = await db.Users.ReadPageAsync(request);
                request = response.Request;
                fullResult.AddRange(response.Result);
                inLoopCount += response.Result.Count();
                // ASSERT - Each page should report certain values the same
                Assert.Null(response.TotalCount);
                Assert.Null(response.TotalPageCount);

                // To the next page
                numberOfRequests++;
            }
            while (response != null && response.HasMorePages() && request.NextPage());

            HashSet<string> existingRoutes = db.Users.Select(u => u.Route).ToHashSet();
            HashSet<string> fullResultRoutes = fullResult.Select(u => u.Route).ToHashSet();
            IEnumerable<string> routeIntersection = existingRoutes.Intersect(fullResultRoutes);

            // ASSERT
            // How many MaxTake pages did we make?
            int maxPages = IndexPageRequest.DefaultMaxTotalResults / (response.Request.Take ?? IndexPageRequest.DefaultMaxTake);
            // How many remainder records did we have?
            Assert.Equal(IndexPageRequest.DefaultMaxTake, response.Result.Count());
            Assert.Equal(maxItemsPaged, inLoopCount);
            Assert.Equal(maxItemsPaged, fullResult.Count);
            Assert.Equal(maxItemsPaged, routeIntersection.Count());
            // How many requests did we make for the total pages?
            Assert.Equal(maxPages, numberOfRequests);
        }

        [Fact]
        public async Task Paging_TooManyWithShortLastPage()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Equal(IdentityDbSeed.SeededUserCount, db.Users.Count());

            // Pull out the max and double-check this unit test isn't out of date
            int maxItemsPaged = IndexPageRequest.DefaultMaxTotalResults;
            // Add in at least as many as the max page size
            for (int i = 0; i < maxItemsPaged; i++)
            {
                db.MockUser();
            }
            db.SaveChanges();
            Assert.Equal(IndexPageRequest.DefaultMaxTotalResults + IdentityDbSeed.SeededUserCount, db.Users.Count());

            int numberOfRequests = 0;
            int inLoopCount = 0;
            int take = 30;
            IndexPageRequest request = new IndexPageRequest { Take = take };
            IndexPageResponse<UserEntity> response = null;
            List<UserEntity> fullResult = new List<UserEntity>();

            // ACT
            do
            {
                // Accumulate info on each page
                response = await db.Users.ReadPageAsync(request);
                request = response.Request;
                fullResult.AddRange(response.Result);
                inLoopCount += response.Result.Count();
                // ASSERT - Each page should report certain values the same
                Assert.Null(response.TotalCount);
                Assert.Null(response.TotalPageCount);

                // To the next page
                numberOfRequests++;
            }
            while (response != null && response.HasMorePages() && request.NextPage());

            HashSet<string> existingRoutes = db.Users.Select(u => u.Route).ToHashSet();
            HashSet<string> fullResultRoutes = fullResult.Select(u => u.Route).ToHashSet();
            IEnumerable<string> routeIntersection = existingRoutes.Intersect(fullResultRoutes);

            // ASSERT
            // How many remainder records did we have?
            Assert.Equal(TestUtils.GetExpectedLastPageItemCount(IndexPageRequest.DefaultMaxTotalResults, request.Take.Value), response.Result.Count());
            // How many MaxTake pages did we make?
            int maxPages = (int)Math.Ceiling(IndexPageRequest.DefaultMaxTotalResults / (decimal)take);
            Assert.Equal(maxPages, numberOfRequests);
            Assert.Equal(maxItemsPaged, inLoopCount);
            Assert.Equal(maxItemsPaged, fullResult.Count);
            Assert.Equal(maxItemsPaged, routeIntersection.Count());
        }


        [Fact]
        public async Task Paging_CustomEach()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            Assert.Equal(IdentityDbSeed.SeededUserCount, db.Users.Count());
            for (int i = 0; i < usersToCreate; i++)
            {
                db.MockUser();
            }
            db.SaveChanges();
            HashSet<string> existingRoutes = db.Users.Select(u => u.Route).ToHashSet();
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, db.Users.Count());
            int take = 10;
            int skip = 0;
            int numberOfRequests = 0;
            int inLoopCount = 0;
            IndexPageResponse<UserEntity> usersPage = null;
            List<UserEntity> fullResult = new List<UserEntity>();

            // ACT
            do
            {
                IndexPageRequest pageRequest = new IndexPageRequest
                {
                    Skip = skip,
                    Take = take,
                    IncludesCounts = true,
                };

                // Accumulate info on each page
                usersPage = await db.Users.ReadPageAsync(pageRequest);
                fullResult.AddRange(usersPage.Result);
                inLoopCount += usersPage.Result.Count();

                // ASSERT - Each page should report certain values the same
                int expectedReq = TestUtils.GetExpectedPageCount(usersToCreate + IdentityDbSeed.SeededUserCount, usersPage.Request.Take.Value);
                Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, usersPage.TotalCount);
                Assert.Equal(take, usersPage.Request.Take);
                Assert.Equal(expectedReq, usersPage.TotalPageCount);

                // To the next page
                skip += take;
                numberOfRequests++;
            }
            while (usersPage != null && usersPage.HasMorePages());

            HashSet<string> fullResultRoutes = fullResult.Select(u => u.Route).ToHashSet();
            IEnumerable<string> routeIntersection = existingRoutes.Intersect(fullResultRoutes);

            // ASSERT
            int expectedRequests = TestUtils.GetExpectedPageCount(usersToCreate + IdentityDbSeed.SeededUserCount, usersPage.Request.Take.Value);
            int expectedLastPageCount = TestUtils.GetExpectedLastPageItemCount(usersToCreate + IdentityDbSeed.SeededUserCount, usersPage.Request.Take.Value);
            Assert.Equal(expectedLastPageCount, usersPage.Result.Count());
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, inLoopCount);
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, fullResult.Count);
            Assert.Equal(usersToCreate + IdentityDbSeed.SeededUserCount, routeIntersection.Count());
            Assert.Equal(expectedRequests, numberOfRequests);
        }

        [Fact]
        public async Task Paging_Zero()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            int take = 10;
            int skip = 0;
            int numberOfRequests = 0;
            int inLoopCount = 0;
            IndexPageResponse<UserEntity> response = null;
            List<UserEntity> fullResult = new List<UserEntity>();

            // ACT
            do
            {
                IndexPageRequest pageRequest = new IndexPageRequest
                {
                    Skip = skip,
                    Take = take,
                    IncludesCounts = true,
                };

                // Accumulate info on each page
                response = await db.Users.Where(u => u.Route == "IMPOSSIBLE_VALUE").ReadPageAsync(pageRequest);
                fullResult.AddRange(response.Result);
                inLoopCount += response.Result.Count();

                // ASSERT - Each page should report certain values the same
                Assert.Equal(0, response.TotalCount);
                Assert.Equal(take, response.Request.Take);
                Assert.Equal(1, response.TotalPageCount);

                // To the next page
                skip += take;
                numberOfRequests++;
            }
            while (response != null && response.HasMorePages());

            // ASSERT
            Assert.Empty(response.Result);
            Assert.Equal(0, inLoopCount);
            Assert.Empty(fullResult);
            Assert.Equal(1, numberOfRequests);
        }

        [Fact]
        public async Task Paging_WithFilter()
        {
            // ARRANGE
            MockIdentityDb db = Builder.DbContext.Value;
            int take = 10;
            int skip = 0;
            int numberOfRequests = 0;
            int inLoopCount = 0;
            IndexPageResponse<UserEntity> response = null;
            List<UserEntity> fullResult = new List<UserEntity>();

            // ACT
            do
            {
                IndexPageRequest request = new IndexPageRequest
                {
                    Skip = skip,
                    Take = take,
                    IncludesCounts = true,
                    // Got to be a piece of an e-mail loaded by MockIdentityDb
                    Filter = "ada"
                };

                // Accumulate info on each page
                response = await db.Users.ReadPageAsync(request, (u) => u.Email.Contains(request.Filter), true);
                fullResult.AddRange(response.Result);
                inLoopCount += response.Result.Count();

                // ASSERT - Each page should report certain values the same
                Assert.Equal(1, response.TotalCount);
                Assert.Equal(take, response.Request.Take);
                Assert.Equal(1, response.TotalPageCount);

                // To the next page
                skip += take;
                numberOfRequests++;
            }
            while (response != null && response.HasMorePages());

            // ASSERT
            Assert.Single(response.Result);
            Assert.Equal("ada@masticore", response.Result.First().Email);
            Assert.Equal(1, inLoopCount);
            Assert.Single(fullResult);
            Assert.Equal(1, numberOfRequests);
        }
    }
}
