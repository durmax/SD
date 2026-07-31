using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using sd.Application.Helper;
using sd.Application.Interfaces.Repositories;
using sd.Application.Services;
using sd.Shared;
using System.Linq.Expressions;
using System.Security.Claims;

namespace sd.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task Create_ShouldReturnExistingUser_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new UserModel
        {
            UserId = "user-1",
            Email = "test@example.com",
            Name = "Test User"
        };

        var repository = new FakeUserRepository(existingUser);
        var cacheHelper = new CachingHelper(new MemoryCache(new MemoryCacheOptions()));
        var service = new UserService(repository, cacheHelper);

        // Act
        var result = await service.Create(existingUser.Email);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(existingUser.UserId);
        result.Email.Should().Be(existingUser.Email);
    }

    [Fact]
    public async Task Create_ShouldCreateNewUser_WhenEmailDoesNotExist()
    {
        // Arrange
        var repository = new FakeUserRepository();
        var cacheHelper = new CachingHelper(new MemoryCache(new MemoryCacheOptions()));
        var service = new UserService(repository, cacheHelper);

        // Act
        var result = await service.Create("new@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("new@example.com");
        repository.CreatedUsers.Should().ContainSingle();
    }

    [Fact]
    public async Task Update_ShouldReturnFalse_WhenExistingUserIsMissing()
    {
        // Arrange
        var repository = new FakeUserRepository();
        var cacheHelper = new CachingHelper(new MemoryCache(new MemoryCacheOptions()));
        var service = new UserService(repository, cacheHelper);

        var user = new UserModel { UserId = "missing", Email = "new@example.com" };

        // Act
        var result = await service.Update(user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ShouldReturnFalse_WhenEmailIsAlreadyUsedByAnotherUser()
    {
        // Arrange
        var existingUser = new UserModel { UserId = "user-1", Email = "existing@example.com", Name = "Existing" };
        var otherUser = new UserModel { UserId = "user-2", Email = "taken@example.com", Name = "Taken" };
        var repository = new FakeUserRepository(existingUser, otherUser);
        var cacheHelper = new CachingHelper(new MemoryCache(new MemoryCacheOptions()));
        var service = new UserService(repository, cacheHelper);

        var updatedUser = new UserModel { UserId = "user-1", Email = "taken@example.com", Name = "Existing" };

        // Act
        var result = await service.Update(updatedUser);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Update_ShouldReturnTrue_WhenEmailIsUnchanged()
    {
        // Arrange
        var existingUser = new UserModel { UserId = "user-1", Email = "same@example.com", Name = "Existing" };
        var repository = new FakeUserRepository(existingUser);
        var cacheHelper = new CachingHelper(new MemoryCache(new MemoryCacheOptions()));
        var service = new UserService(repository, cacheHelper);

        var updatedUser = new UserModel { UserId = "user-1", Email = "same@example.com", Name = "Updated Name" };

        // Act
        var result = await service.Update(updatedUser);

        // Assert
        result.Should().BeTrue();
        repository.UpdatedUsers.Should().ContainSingle();
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnNull_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var repository = new FakeUserRepository();
        var cacheHelper = new CachingHelper(new MemoryCache(new MemoryCacheOptions()));
        var service = new UserService(repository, cacheHelper);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = await service.GetCurrentUser(principal);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUser_WhenAuthenticatedUserHasEmailClaim()
    {
        // Arrange
        var repository = new FakeUserRepository(new UserModel { UserId = "user-1", Email = "auth@example.com", Name = "Auth User" });
        var cacheHelper = new CachingHelper(new MemoryCache(new MemoryCacheOptions()));
        var service = new UserService(repository, cacheHelper);
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Email, "auth@example.com") }, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = await service.GetCurrentUser(principal);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("auth@example.com");
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<UserModel> _users;
        public List<UserModel> CreatedUsers { get; } = new();
        public List<UserModel> UpdatedUsers { get; } = new();

        public FakeUserRepository(params UserModel[] users)
        {
            _users = users.ToList();
        }

        public Task<bool> Create(UserModel user)
        {
            CreatedUsers.Add(user);
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                user.Email = $"generated-{Guid.NewGuid():N}@example.com";
            }

            _users.Add(user);
            return Task.FromResult(true);
        }

        public Task<bool> Delete(string id)
        {
            _users.RemoveAll(x => x.UserId == id);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<UserModel>> GetByCondition(Expression<Func<UserModel, bool>> expression)
        {
            var predicate = expression.Compile();
            return Task.FromResult<IEnumerable<UserModel>>(_users.Where(predicate).ToList());
        }

        public Task<UserModel> GetById(string id)
        {
            return Task.FromResult(_users.FirstOrDefault(x => x.UserId == id)!);
        }

        public Task<bool> Update(UserModel newVer)
        {
            UpdatedUsers.Add(newVer);
            var index = _users.FindIndex(x => x.UserId == newVer.UserId);
            if (index < 0) return Task.FromResult(false);

            _users[index] = newVer;
            return Task.FromResult(true);
        }
    }
}
