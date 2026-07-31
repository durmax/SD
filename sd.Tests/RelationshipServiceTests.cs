using FluentAssertions;
using sd.Application.Interfaces.Repositories;
using sd.Application.Services;
using sd.Shared;
using System.Linq.Expressions;

namespace sd.Tests;

public class RelationshipServiceTests
{
    [Fact]
    public async Task AddRelationship_ShouldReturnFalse_WhenRelationshipAlreadyExistsWithFriendRelation()
    {
        // Arrange
        var existing = new RelationshipModel
        {
            UserId1 = "u1",
            UserId2 = "u2",
            Relation = Relation.Friend
        };

        var repo = new FakeRelationshipRepository(existing);
        var userRepo = new FakeUserRepository();
        var service = new RelationshipService(repo, userRepo);
        var relationship = new RelationshipModel { UserId1 = "u1", UserId2 = "u2", Relation = Relation.Friend };

        // Act
        var result = await service.AddRelationship(relationship);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AddRelationship_ShouldReturnTrue_WhenExistingRelationIsNone()
    {
        // Arrange
        var existing = new RelationshipModel
        {
            UserId1 = "u1",
            UserId2 = "u2",
            Relation = Relation.None
        };

        var repo = new FakeRelationshipRepository(existing);
        var userRepo = new FakeUserRepository();
        var service = new RelationshipService(repo, userRepo);
        var relationship = new RelationshipModel { UserId1 = "u1", UserId2 = "u2", Relation = Relation.Friend };

        // Act
        var result = await service.AddRelationship(relationship);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetRelationship_ShouldReturnRelationship_WhenMatchingPairExists()
    {
        // Arrange
        var existing = new RelationshipModel
        {
            RelationshipId = "r1",
            UserId1 = "u1",
            UserId2 = "u2",
            Relation = Relation.FriendRequestTo
        };

        var repo = new FakeRelationshipRepository(existing);
        var service = new RelationshipService(repo, new FakeUserRepository());

        // Act
        var result = await service.GetRelationship("u1", Relation.FriendRequestTo, "u2");

        // Assert
        result.Should().NotBeNull();
        result.Relation.Should().Be(Relation.FriendRequestTo);
    }

    [Fact]
    public async Task GetAllFriends_ShouldReturnFriendNames_WhenRelationshipsExist()
    {
        // Arrange
        var relationships = new[]
        {
            new RelationshipModel { UserId1 = "u1", UserId2 = "u2", Relation = Relation.Friend },
            new RelationshipModel { UserId1 = "u1", UserId2 = "u3", Relation = Relation.Friend }
        };

        var repo = new FakeRelationshipRepository(relationships);
        var userRepo = new FakeUserRepository(
            new UserModel { UserId = "u1", Name = "Me", Email = "me@example.com" },
            new UserModel { UserId = "u2", Name = "Alice", Email = "alice@example.com" },
            new UserModel { UserId = "u3", Name = "Bob", Email = "bob@example.com" });
        var service = new RelationshipService(repo, userRepo);

        // Act
        var result = await service.GetAllFriends("u1");

        // Assert
        result.Should().ContainKey("u2").WhoseValue.Should().Be("Alice");
        result.Should().ContainKey("u3").WhoseValue.Should().Be("Bob");
    }

    private sealed class FakeRelationshipRepository : IRelationshipRepository
    {
        private readonly List<RelationshipModel> _relationships;

        public FakeRelationshipRepository(params RelationshipModel[] relationships)
        {
            _relationships = relationships.ToList();
        }

        public Task<bool> Create(RelationshipModel entity)
        {
            _relationships.Add(entity);
            return Task.FromResult(true);
        }

        public Task<bool> Delete(string id)
        {
            _relationships.RemoveAll(x => x.RelationshipId == id);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<RelationshipModel>> GetByCondition(Expression<Func<RelationshipModel, bool>> expression)
        {
            var predicate = expression.Compile();
            return Task.FromResult<IEnumerable<RelationshipModel>>(_relationships.Where(predicate).ToList());
        }

        public Task<RelationshipModel> GetById(string id)
            => Task.FromResult(_relationships.FirstOrDefault(x => x.RelationshipId == id)!);

        public Task<bool> Update(RelationshipModel entity)
            => Task.FromResult(true);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly List<UserModel> _users;

        public FakeUserRepository(params UserModel[] users)
        {
            _users = users.ToList();
        }

        public Task<bool> Create(UserModel user)
            => Task.FromResult(true);

        public Task<bool> Delete(string id)
            => Task.FromResult(true);

        public Task<IEnumerable<UserModel>> GetByCondition(Expression<Func<UserModel, bool>> expression)
        {
            var predicate = expression.Compile();
            return Task.FromResult<IEnumerable<UserModel>>(_users.Where(predicate).ToList());
        }

        public Task<UserModel> GetById(string id)
            => Task.FromResult(_users.FirstOrDefault(x => x.UserId == id)!);

        public Task<bool> Update(UserModel newVer)
            => Task.FromResult(true);
    }
}
