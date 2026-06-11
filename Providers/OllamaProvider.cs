using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using AIRecruitmentAPI.Core;

namespace AIRecruitmentAPI.Providers;

public class OllamaProvider : IModelProvider
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;
    private readonly string _model;

    public OllamaProvider(HttpClient httpClient, IConfiguration config)
    {
        _http = httpClient;
        _baseUrl = config["AI:BaseUrl"];
        _model = config["AI:Model"];
    }

    public async Task<string> GenerateText(string prompt)
    {
        var body = new
        {
            model = _model,
            prompt = prompt,
            stream = false
        };

        var response = await _http.PostAsync(
            $"{_baseUrl}/api/generate",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ollama API failed: {error}");
        }

        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("response").GetString();
    }
}