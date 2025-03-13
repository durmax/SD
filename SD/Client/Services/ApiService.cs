using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using System.Net.Http.Json;

namespace SD.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;

        public ApiService(CurrentUserService currentUser, ILogger<ApiService> logger)
        {
            _httpClient = currentUser.HttpClient;
            _logger = logger;
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
                _logger.LogError($"GET request failed: {url}, Error: {ex}");
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
                _logger.LogError($"POST request failed: {url}, Error: {ex}");
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
                _logger.LogError($"PUT request failed: {url}, Error: {ex}");
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
                    _logger.LogError($"DELETE API Error: {errorMessage}");
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DELETE request failed: {url}, Error: {ex}");
                throw;
            }
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<T>();
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                _logger.LogError($"API Error: {errorMessage}");
                throw new Exception($"API Error: {errorMessage}");
            }
        }
    }
}