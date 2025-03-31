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
        private readonly LoggingService _logger;

        public ApiService(CurrentUserService currentUser, LoggingService logger)
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
                _logger.Log("ApiService", LogLevel.Error, $"GET request failed: {url}, Error: {ex}");
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
                _logger.Log("ApiService", LogLevel.Error, $"POST request failed: {url}, Error: {ex}");
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
                _logger.Log("ApiService", LogLevel.Error, $"PUT request failed: {url}, Error: {ex}");
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
                    _logger.Log("ApiService", LogLevel.Error, $"DELETE API Error: {errorMessage}");
                }
                return response;
            }
            catch (Exception ex)
            {
                _logger.Log("ApiService", LogLevel.Error, $"DELETE request failed: {url}, Error: {ex}");
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
                _logger.Log("ApiService", LogLevel.Error, $"API Error: {errorMessage}");
                throw new Exception($"API Error: {errorMessage}");
            }
        }
    }
}