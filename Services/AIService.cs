using System.Text;
using System.Text.Json;

namespace AIRecruitmentAPI.Services;

public class AIService : IAIService
{
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public AIService(IConfiguration config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }

    private async Task<string> CallOpenAI(string systemPrompt, string userPrompt)
    {
        var endpoint = $"{_config["AzureOpenAI:Endpoint"]}openai/deployments/{_config["AzureOpenAI:Model"]}/chat/completions?api-version=2025-01-01-preview";

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("api-key", _config["AzureOpenAI:ApiKey"]);

        var body = new
        {
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        var response = await _httpClient.PostAsync(
            endpoint,
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        );

        var json = await response.Content.ReadAsStringAsync();

        var doc = JsonDocument.Parse(json);
        return doc.RootElement
                  .GetProperty("choices")[0]
                  .GetProperty("message")
                  .GetProperty("content")
                  .GetString() ?? string.Empty;
    }

    public async Task<string> ScreenResume(string resume, string jobDescription)
    {
        var prompt = $@"You are an expert recruiter.

Job Description:
{jobDescription}

Candidate Resume:
{resume}

Evaluate the candidate and provide:
- Score (0-100)
- Short summary (2-3 lines)
- Final decision (Shortlisted / Rejected)";

        return await CallOpenAI("You are a smart recruitment assistant.", prompt);
    }

    public async Task<string> GenerateOffer(string candidateName)
    {
        var prompt = $@"Generate a professional job offer letter for:
Candidate Name: {candidateName}
Role: Software Engineer
Salary: 10 LPA
Keep it formal and concise.";

        return await CallOpenAI("You are an HR assistant.", prompt);
    }
}
