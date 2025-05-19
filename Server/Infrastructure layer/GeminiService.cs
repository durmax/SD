
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json; // Use Newtonsoft.Json

namespace sd.Api.Infrastructure;
    /// <summary>
    /// A service that uses the Gemini API to process text.
    /// </summary>
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent"; // Corrected API URL

        /// <summary>
        /// Initializes a new instance of the <see cref="GeminiService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="apiKey">The Gemini API key.</param>
        public GeminiService(HttpClient httpClient)
        {
        string apiKey = "AIzaSyCGzeZgqpRVNs1nDFwKHVE0PUQTMPjkIfU";

            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
            }
        }

        /// <summary>
        /// Processes the input string using the Gemini API.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The response from the Gemini API.</returns>
        public async Task<string> ProcessStringAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return "Input was null or empty.";
            }

            try
            {
                // Construct the request payload.  Use the classes
                var requestData = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = input
                                }
                            }
                        }
                    }
                };
                // Serialize the request.
                string jsonRequest = JsonConvert.SerializeObject(requestData);
                StringContent content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // Add the API key to the request URL.
                string url = $"{GeminiApiUrl}?key={_apiKey}";

                // Send the request.
                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode(); // Ensure a successful response.

                // Deserialize the response.
                string jsonResponse = await response.Content.ReadAsStringAsync();
                // Use the classes to deserialize.
                var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(jsonResponse);

                // Extract the relevant information from the response.
                if (geminiResponse?.candidates != null && geminiResponse.candidates.Length > 0 &&
                    geminiResponse.candidates[0]?.content?.parts != null && geminiResponse.candidates[0].content.parts.Length > 0)
                {
                    return geminiResponse.candidates[0].content.parts[0].text;
                }
                else
                {
                    return "No response from Gemini API."; //Or, you can throw an exception
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP request errors.
                return $"Error communicating with Gemini API: {ex.Message}";
            }
            catch (JsonException ex)
            {
                // Handle JSON serialization/deserialization errors.
                return $"Error processing Gemini API response: {ex.Message}";
            }
            catch (Exception ex)
            {
                // Handle other errors.
                return $"An error occurred: {ex.Message}";
            }
        }
    }
    // Add these classes to represent the structure.
    public class Part
    {
        public string text { get; set; }
    }

    public class Content
    {
        public Part[] parts { get; set; }
    }

    public class Candidate
    {
        public Content content { get; set; }
    }

    public class GeminiResponse
    {
        public Candidate[] candidates { get; set; }
    }

