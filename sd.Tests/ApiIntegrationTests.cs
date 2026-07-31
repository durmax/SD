using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using sd.Application.Services;
using sd.Shared;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace sd.Tests;

public class ApiIntegrationTests : IClassFixture<ApiTestWebApplicationFactory>
{
    private readonly ApiTestWebApplicationFactory _factory;

    public ApiIntegrationTests(ApiTestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHealthz_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/healthz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCurrentUser_WithTestAuthentication_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new System.Net.Http.HttpRequestMessage(HttpMethod.Get, "/api/User/GetCurrentUser");
        request.Headers.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName, "test-token");

        // Act
        var response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class ApiTestWebApplicationFactory : WebApplicationFactory<sd.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTests");

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            services.AddAuthorization();

            services.AddSingleton<IUserService, FakeUserService>();
            services.AddSingleton<IRelationshipService, FakeRelationshipService>();
        });
    }
}

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                           ILoggerFactory logger,
                           UrlEncoder encoder,
                           ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User")
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public sealed class FakeUserService : IUserService
{
    public Task<bool> Create(UserModel entity) => Task.FromResult(true);

    public Task<UserModel?> Create(string userEmail) => Task.FromResult<UserModel?>(new UserModel { UserId = "test-user", Email = userEmail, Name = "Test User" });

    public Task<bool> Delete(string id) => Task.FromResult(true);

    public Task<IEnumerable<UserModel>> GetByCondition(System.Linq.Expressions.Expression<System.Func<UserModel, bool>> expression)
        => Task.FromResult<IEnumerable<UserModel>>(new List<UserModel>());

    public Task<UserModel> GetById(string id) => Task.FromResult(new UserModel { UserId = id, Email = "test@example.com", Name = "Test User" });

    public Task<UserModel?> GetCurrentUser(ClaimsPrincipal user)
        => Task.FromResult<UserModel?>(new UserModel { UserId = "test-user", Email = user.FindFirst(ClaimTypes.Email)?.Value ?? "test@example.com", Name = user.Identity?.Name ?? "Test User" });

    public Task<bool> Update(UserModel entity) => Task.FromResult(true);
}

public sealed class FakeRelationshipService : IRelationshipService
{
    public Task<bool> AddRelationship(RelationshipModel relationship) => Task.FromResult(true);

    public Task<Dictionary<string, string>> FriendRequestsToUser(string userId)
        => Task.FromResult(new Dictionary<string, string>());

    public Task<Dictionary<string, string>> GetAllFriends(string userId)
        => Task.FromResult(new Dictionary<string, string>());

    public Task<IEnumerable<RelationshipModel>> GetByCondition(System.Linq.Expressions.Expression<System.Func<RelationshipModel, bool>> expression)
        => Task.FromResult<IEnumerable<RelationshipModel>>(new List<RelationshipModel>());

    public Task<RelationshipModel> GetRelationship(string UserId1, Relation relation, string UserId2)
        => Task.FromResult<RelationshipModel?>(null);

    public Task<List<UserRelationshipsWithOneUserDto>> GetRelationships(string currentUserId, List<UserModel> users)
        => Task.FromResult(new List<UserRelationshipsWithOneUserDto>());

    public Task<bool> Create(RelationshipModel entity) => Task.FromResult(true);

    public Task<bool> Delete(string id) => Task.FromResult(true);

    public Task<bool> Update(RelationshipModel entity) => Task.FromResult(true);
}
