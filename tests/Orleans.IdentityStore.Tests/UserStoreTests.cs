using Microsoft.AspNetCore.Identity;
using Orleans.IdentityStore.Stores;
using Orleans.TestingHost;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.IdentityStore.Tests
{
    [Collection(ClusterCollection.Name)]
    public class UserStoreTests
    {
        private readonly TestCluster _cluster;

        public UserStoreTests(ClusterFixture fixture)
        {
            _cluster = fixture.Cluster;
        }

        [Fact]
        public async Task CanCreateUserWithoutSettingId()
        {
            var username = $"{Guid.NewGuid()}";
            var user = new TestUser
            {
                NormalizedEmail = username + "@test.com",
                Email = username + "@test.com",
                UserName = username,
                NormalizedUserName = username
            };

            var store = GetSubject();

            await store.CreateAsync(user);
            Assert.NotEqual(default, user.Id);

            var userById = await store.FindByIdAsync(user.Id.ToString());
            var userByName = await store.FindByEmailAsync(username + "@test.com");

            Assert.NotNull(userById);
            Assert.NotNull(userByName);
            Assert.Equal(user.Id, userById.Id);
            Assert.Equal(user.Id, userByName.Id);
            Assert.Equal(user.Email, userById.Email);
            Assert.Equal(user.Email, userByName.Email);
        }

        [Fact]
        public async Task CanCreateUserWithSettingId()
        {
            var userId = Guid.NewGuid();
            var username = $"{userId}";
            var user = new TestUser
            {
                Id = userId,
                NormalizedEmail = username + "@test.com",
                Email = username + "@test.com",
                UserName = username,
                NormalizedUserName = username
            };

            var store = GetSubject();

            await store.CreateAsync(user);
            Assert.Equal(userId, user.Id);

            var userById = await store.FindByIdAsync(user.Id.ToString());
            var userByName = await store.FindByEmailAsync(username + "@test.com");

            Assert.NotNull(userById);
            Assert.NotNull(userByName);
            Assert.Equal(userId, userById.Id);
            Assert.Equal(userId, userByName.Id);
            Assert.Equal(user.Email, userById.Email);
            Assert.Equal(user.Email, userByName.Email);
        }

        [Fact]
        public async Task CanDeleteUser()
        {
            var userId = Guid.NewGuid();
            var username = $"{userId}";
            var user = new TestUser
            {
                Id = userId,
                NormalizedEmail = username + "@test.com",
                Email = username + "@test.com",
                UserName = username,
                NormalizedUserName = username
            };

            var store = GetSubject();

            await store.CreateAsync(user);
            await store.DeleteAsync(user);

            var userById = await store.FindByIdAsync(user.Id.ToString());
            var userByName = await store.FindByEmailAsync(username + "@test.com");

            Assert.Null(userById);
            Assert.Null(userByName);
        }

        private OrleansUserStore<TestUser, IdentityRole<Guid>> GetSubject()
        {
            var roleStore = new OrleansRoleStore<TestUser, IdentityRole<Guid>>(_cluster.Client);
            return new OrleansUserStore<TestUser, IdentityRole<Guid>>(_cluster.Client, roleStore);
        }
    }
}