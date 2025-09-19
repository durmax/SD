using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace sd.Client.Services
{
    public sealed class CurrentUserService : IDisposable
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CurrentUserService> _log;
        private readonly TaskCompletionSource _initializationTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private bool _disposed;

        public bool IsAuthenticated { get; private set; }
        public HttpClient HttpClient { get; private set; } = default!;

        // Optional: let consumers react when the client flips between authed/anon
        public event Action<bool, HttpClient>? OnClientChanged;

        public CurrentUserService(
            AuthenticationStateProvider authenticationStateProvider,
            IHttpClientFactory httpClientFactory, ILogger<CurrentUserService> log)
        {
            Console.WriteLine("sttttart");

            _authenticationStateProvider = authenticationStateProvider;
            _httpClientFactory = httpClientFactory;
            _log = log;

            // 1) Subscribe first to avoid any race with fast state changes
            _authenticationStateProvider.AuthenticationStateChanged += HandleAuthenticationStateChanged;

            // 2) Kick off the initial fetch
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                await ProcessAuthStateAsync(_authenticationStateProvider.GetAuthenticationStateAsync());
            }
            finally
            {
                _initializationTcs.TrySetResult(); // let GetClientAsync() continue
            }
        }

        private void CreateHttpClient(bool isAuthenticated)
        {
            var name = isAuthenticated ? "forAuthenticatedUser" : "forNotAuthenticatedUser";

            // Dispose the previous instance to avoid leaks
            var old = HttpClient;
            HttpClient = _httpClientFactory.CreateClient(name);
            old?.Dispose();
        }

        private async void HandleAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            try
            {
                await ProcessAuthStateAsync(task); // delegate to Task-returning method
            }
            catch (ObjectDisposedException) { /* ignore during dispose */ }
            catch (Exception)
            {
                _log.LogError("Error processing authentication state change");
            }
        }

        // 2) New Task-returning method with the actual logic
        private async Task ProcessAuthStateAsync(Task<AuthenticationState> task)
        {
            var authState = await task.ConfigureAwait(false);
            var newIsAuthenticated = authState.User.Identity?.IsAuthenticated == true;

            var shouldSwap = HttpClient == null || newIsAuthenticated != IsAuthenticated;

            IsAuthenticated = newIsAuthenticated;

            if (shouldSwap)
            {
                CreateHttpClient(IsAuthenticated);
                OnClientChanged?.Invoke(IsAuthenticated, HttpClient); // if you expose this event
            }
        }

        /// <summary>
        /// Await this to guarantee HttpClient/IsAuthenticated are ready.
        /// </summary>
        public async ValueTask<HttpClient> GetClientAsync(CancellationToken cancellationToken = default)
        {
            // If initialization already finished, this is a fast path.
            if (!_initializationTcs.Task.IsCompleted)
                await _initializationTcs.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

            return HttpClient;
        }

        public void Dispose()
        {
            _log.LogInformation("CurrentUserService disposing...");
            if (_disposed) return;
            _disposed = true;

            _authenticationStateProvider.AuthenticationStateChanged -= HandleAuthenticationStateChanged;
            HttpClient?.Dispose();
        }
    }
}