using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

namespace sd.Application.Services.Gemini;
/// <summary>
/// A service that uses the Gemini API to process text.
/// </summary>
public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<GeminiSettings> _geminiSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="GeminiService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="apiKey">The Gemini API key.</param>
    public GeminiService(HttpClient httpClient, IOptions<GeminiSettings> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _geminiSettings = options;
    }

    /// <summary>
    /// Processes the input string using the Gemini API.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The response from the Gemini API.</returns>
    public async Task<string> ProcessStringAsync(string input, CancellationToken cancellationToken)
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
            string url = $"{_geminiSettings.Value.ApiUrl}?key={_geminiSettings.Value.ApiKey}";

            // Send the request.
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            // Ensure the operation was not canceled during processing
            cancellationToken.ThrowIfCancellationRequested();

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
        catch (OperationCanceledException)
        {
            // This exception is thrown if the client disconnects.
            // You can log this event if needed.
            return "Request canceled by the client: {ex.Message}"; // 499 is a common status code for Client Closed Request
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

