using AutoMapper;
using FluentAssertions;
using sd.Application.Interfaces.Repositories;
using sd.Application.Services;
using sd.Application.Services.Gemini;
using sd.Shared;
using System.Linq.Expressions;
using Microsoft.Extensions.Options;

namespace sd.Tests;

public class WordServiceTests
{
    [Fact]
    public async Task AddWord_ShouldReturnNull_WhenTitleOrUserIdIsMissing()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.AddWord(new WordDto { Title = " ", UserId = "user-1" });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddWord_ShouldCreateWordWithGeneratedId_WhenInputIsValid()
    {
        // Arrange
        var wordRepo = new FakeWordRepository();
        var service = CreateService(wordRepo);

        // Act
        var result = await service.AddWord(new WordDto { Title = "Hello", UserId = "user-1", WordLang = "en", ToLang = "ar" });

        // Assert
        result.Should().NotBeNullOrWhiteSpace();
        wordRepo.Words.Should().ContainSingle();
        wordRepo.Words[0].WordId.Should().Be(result);
        wordRepo.Words[0].Title.Should().Be("Hello");
    }

    [Fact]
    public async Task Like_ShouldReturnMinusOne_WhenWordDoesNotExist()
    {
        // Arrange
        var wordRepo = new FakeWordRepository();
        var service = CreateService(wordRepo);

        // Act
        var result = await service.Like("user-1", "missing-word");

        // Assert
        result.Should().Be(-1);
    }

    [Fact]
    public async Task SaveComment_ShouldReturnFalse_WhenWordIsMissing()
    {
        // Arrange
        var wordRepo = new FakeWordRepository();
        var service = CreateService(wordRepo);

        // Act
        var result = await service.SaveComment("missing-word", new CommentModel { CommentId = "c1", UserId = "u1", CommentText = "hello" });

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetWordDtoById_ShouldReturnNull_WhenWordIsNotVisibleToViewer()
    {
        // Arrange
        var word = new WordModel
        {
            WordId = "word-1",
            UserId = "owner",
            Title = "Hello",
            ShareWith = (int)ShareWith.Friends,
            Likes = new List<string>(),
            Comments = new List<CommentModel>()
        };

        var wordRepo = new FakeWordRepository(word);
        var relationshipRepo = new FakeRelationshipRepository();
        var userRepo = new FakeUserRepository();
        var service = CreateService(wordRepo, relationshipRepo, userRepo);

        // Act
        var result = await service.GetWordDtoById("word-1", "viewer");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWordDtoById_ShouldReturnWord_WhenOwnerViewsTheirOwnFriendsOnlyWord()
    {
        // Arrange
        var word = new WordModel
        {
            WordId = "word-1",
            UserId = "owner",
            Title = "Hello",
            ShareWith = (int)ShareWith.Friends,
            Likes = new List<string>(),
            Comments = new List<CommentModel>()
        };

        var service = CreateService(new FakeWordRepository(word));

        // Act
        var result = await service.GetWordDtoById("word-1", "owner");

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Hello");
    }

    [Fact]
    public async Task GetWordDtoById_ShouldReturnWord_WhenViewerIsFriendAndWordIsFriendsOnly()
    {
        // Arrange
        var word = new WordModel
        {
            WordId = "word-2",
            UserId = "owner",
            Title = "Bonjour",
            ShareWith = (int)ShareWith.Friends,
            Likes = new List<string>(),
            Comments = new List<CommentModel>()
        };

        var relationshipRepo = new FakeRelationshipRepository(new RelationshipModel
        {
            UserId1 = "viewer",
            UserId2 = "owner",
            Relation = Relation.Friend
        });

        var service = CreateService(new FakeWordRepository(word), relationshipRepo, new FakeUserRepository());

        // Act
        var result = await service.GetWordDtoById("word-2", "viewer");

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Bonjour");
    }

    [Fact]
    public async Task Like_ShouldAddUserId_WhenWordExistsAndUserHasNotLikedIt()
    {
        // Arrange
        var word = new WordModel
        {
            WordId = "word-1",
            UserId = "owner",
            Title = "Hello",
            Likes = new List<string>(),
            Comments = new List<CommentModel>()
        };

        var repo = new FakeWordRepository(word);
        var service = CreateService(repo);

        // Act
        var result = await service.Like("viewer", "word-1");

        // Assert
        result.Should().Be(1);
        repo.Words.Single().Likes.Should().ContainSingle().Which.Should().Be("viewer");
    }

    [Fact]
    public async Task Like_ShouldRemoveUserId_WhenUserAlreadyLikedWord()
    {
        // Arrange
        var word = new WordModel
        {
            WordId = "word-2",
            UserId = "owner",
            Title = "Hello",
            Likes = new List<string> { "viewer" },
            Comments = new List<CommentModel>()
        };

        var repo = new FakeWordRepository(word);
        var service = CreateService(repo);

        // Act
        var result = await service.Like("viewer", "word-2");

        // Assert
        result.Should().Be(0);
        repo.Words.Single().Likes.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveComment_ShouldAddComment_WhenWordExists()
    {
        // Arrange
        var word = new WordModel
        {
            WordId = "word-3",
            UserId = "owner",
            Title = "Hello",
            Likes = new List<string>(),
            Comments = new List<CommentModel>()
        };

        var repo = new FakeWordRepository(word);
        var service = CreateService(repo);
        var comment = new CommentModel { CommentId = "c1", UserId = "viewer", CommentText = "Nice word" };

        // Act
        var result = await service.SaveComment("word-3", comment);

        // Assert
        result.Should().BeTrue();
        repo.Words.Single().Comments.Should().ContainSingle();
        repo.Words.Single().Comments[0].CommentText.Should().Be("Nice word");
    }

    [Fact]
    public async Task SaveComment_ShouldReplaceExistingComment_WhenSameCommentIdAlreadyExists()
    {
        // Arrange
        var existingComment = new CommentModel
        {
            CommentId = "c1",
            UserId = "viewer",
            CommentText = "Old text"
        };

        var word = new WordModel
        {
            WordId = "word-4",
            UserId = "owner",
            Title = "Hello",
            Likes = new List<string>(),
            Comments = new List<CommentModel> { existingComment }
        };

        var repo = new FakeWordRepository(word);
        var service = CreateService(repo);

        // Act
        var result = await service.SaveComment("word-4", new CommentModel { CommentId = "c1", UserId = "viewer", CommentText = "Updated text" });

        // Assert
        result.Should().BeTrue();
        repo.Words.Single().Comments.Should().ContainSingle();
        repo.Words.Single().Comments[0].CommentText.Should().Be("Updated text");
    }

    private static WordService CreateService(
        IWordRepository? wordRepo = null,
        IRelationshipRepository? relationshipRepo = null,
        IUserRepository? userRepo = null)
    {
        var mapper = new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<WordModel, WordDto>()
                .ForMember(dto => dto.CommentsCount, exp => exp.MapFrom(w => w.Comments != null ? w.Comments.Count : 0))
                .ForMember(dto => dto.LikesCount, exp => exp.MapFrom(w => w.Likes != null ? w.Likes.Count : 0));
            cfg.CreateMap<WordDto, WordModel>();
        }));

        var geminiService = new GeminiService(new HttpClient(), Options.Create(new GeminiSettings()));

        return new WordService(
            wordRepo ?? new FakeWordRepository(),
            mapper,
            userRepo ?? new FakeUserRepository(),
            relationshipRepo ?? new FakeRelationshipRepository(),
            geminiService);
    }

    private sealed class FakeWordRepository : IWordRepository
    {
        public List<WordModel> Words { get; } = new();

        public FakeWordRepository(params WordModel[] words)
        {
            Words.AddRange(words);
        }

        public Task<bool> Create(WordModel entity)
        {
            entity.WordId ??= Guid.NewGuid().ToString();
            Words.Add(entity);
            return Task.FromResult(true);
        }

        public Task<bool> Delete(string id)
        {
            Words.RemoveAll(x => x.WordId == id);
            return Task.FromResult(true);
        }

        public Task<IEnumerable<WordModel>> GetByCondition(Expression<Func<WordModel, bool>> expression)
        {
            var predicate = expression.Compile();
            return Task.FromResult<IEnumerable<WordModel>>(Words.Where(predicate).ToList());
        }

        public Task<long> GetDocCount(string userId, string lang)
        {
            return Task.FromResult<long>(Words.Count);
        }

        public Task<WordModel> GetById(string id)
        {
            return Task.FromResult(Words.FirstOrDefault(x => x.WordId == id)!);
        }

        public Task<WordModel?> GetWord(string userId, string lang, int currentPage, int limit)
        {
            return Task.FromResult<WordModel?>(Words.FirstOrDefault());
        }

        public Task<List<WordModel>> GetWords(string userId, string lang, int skip, int limit)
        {
            return Task.FromResult(Words.Skip(skip).Take(limit).ToList());
        }

        public Task<bool> Update(WordModel entity)
        {
            var index = Words.FindIndex(x => x.WordId == entity.WordId);
            if (index < 0) return Task.FromResult(false);
            Words[index] = entity;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeRelationshipRepository : IRelationshipRepository
    {
        private readonly List<RelationshipModel> _relationships;

        public FakeRelationshipRepository(params RelationshipModel[] relationships)
        {
            _relationships = relationships.ToList();
        }

        public Task<bool> Create(RelationshipModel entity)
            => Task.FromResult(true);

        public Task<bool> Delete(string id)
            => Task.FromResult(true);

        public Task<IEnumerable<RelationshipModel>> GetByCondition(Expression<Func<RelationshipModel, bool>> expression)
        {
            var predicate = expression.Compile();
            return Task.FromResult<IEnumerable<RelationshipModel>>(_relationships.Where(predicate).ToList());
        }

        public Task<RelationshipModel> GetById(string id)
            => Task.FromResult(new RelationshipModel());

        public Task<bool> Update(RelationshipModel entity)
            => Task.FromResult(true);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public Task<bool> Create(UserModel user)
            => Task.FromResult(true);

        public Task<bool> Delete(string id)
            => Task.FromResult(true);

        public Task<IEnumerable<UserModel>> GetByCondition(Expression<Func<UserModel, bool>> expression)
        {
            var predicate = expression.Compile();
            return Task.FromResult<IEnumerable<UserModel>>(Array.Empty<UserModel>());
        }

        public Task<UserModel> GetById(string id)
            => Task.FromResult(new UserModel { UserId = id, Name = "Test User", Email = "test@example.com" });

        public Task<bool> Update(UserModel newVer)
            => Task.FromResult(true);
    }
}
