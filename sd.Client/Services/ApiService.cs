using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;

namespace sd.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _log;

        public ApiService(CurrentUserService currentUser, ILogger<ApiService> log)
        {
            _httpClient = currentUser.HttpClient;
            _log = log;
        }

        public async Task<string> GetStringAsync(string url)
        {
            try
            {
                var httpResponse = await _httpClient.GetAsync(url);
                httpResponse.EnsureSuccessStatusCode();
                return await httpResponse.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _log.LogError($"GET string request failed: {url}, Error: {ex}");
                throw;
            }

        }

        public async Task<T> GetAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                _log.LogError($"GET request failed: {url}, Error: {ex}");
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string url, object data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, data);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                _log.LogError($"POST request failed: {url}, Error: {ex}");
                throw;
            }
        }

        public async Task<T> PutAsync<T>(string url, object data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(url, data);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                _log.LogError($"PUT request failed: {url}, Error: {ex}");
                throw;
            }
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    _log.LogError($"DELETE API Error: {errorMessage}");
                }
                return response;
            }
            catch (Exception ex)
            {
                _log.LogError($"DELETE request failed: {url}, Error: {ex}");
                throw;
            }
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Found)
            {
                if (typeof(T) == typeof(HttpResponseMessage))
                {
                    return (T)(object)response;
                }

                return await response.Content?.ReadFromJsonAsync<T>();
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _log.LogError($"API Error: {errorMessage}");
                // throw new Exception($"API Error: {errorMessage}");
                return default;
            }
        }
    }
}