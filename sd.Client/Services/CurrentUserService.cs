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
        private Exception? _initException;
        private bool _initialized;

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
                await ProcessAuthStateAsync(_authenticationStateProvider.GetAuthenticationStateAsync())
                        .ConfigureAwait(false);
                _initialized = true;
                _initializationTcs.TrySetResult();
            }
            catch (Exception ex)
            {
                _initException = ex;
                _initializationTcs.TrySetException(ex);
            }
        }

        private void CreateHttpClient(bool isAuthenticated)
        {
            var name = isAuthenticated ? "forAuthenticatedUser" : "forNotAuthenticatedUser";

            // Dispose the previous instance to avoid leaks
            var old = HttpClient;
            HttpClient = _httpClientFactory.CreateClient(name);
        }

        private async void HandleAuthenticationStateChanged(Task<AuthenticationState> task)
        {
            try
            {
                await ProcessAuthStateAsync(task); // delegate to Task-returning method
            }
            catch (ObjectDisposedException) { /* ignore during dispose */ }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error processing authentication state change");
            }
        }

        // 2) New Task-returning method with the actual logic
        private async Task ProcessAuthStateAsync(Task<AuthenticationState> task)
        {
            var authState = await task.ConfigureAwait(false);
            var newIsAuthenticated = authState.User.Identity?.IsAuthenticated == true;

            var shouldSwap = !_initialized || newIsAuthenticated != IsAuthenticated;

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
            await _initializationTcs.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

            if (_initException is not null) throw _initException;

            return HttpClient;
        }

        public void Dispose()
        {
            _log.LogInformation("CurrentUserService disposing...");
            if (_disposed) return;
            _disposed = true;

            _authenticationStateProvider.AuthenticationStateChanged -= HandleAuthenticationStateChanged;
        }
    }
}