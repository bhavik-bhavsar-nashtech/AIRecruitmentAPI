
using System.Text.Json;
using AIRecruitmentAPI.Core;

namespace AIRecruitmentAPI.Services;
public class AIService : IAIService
{
    private readonly IModelProvider _provider;
    public AIService(IModelProvider provider){_provider=provider;}

    public async Task<ScreeningResult> ScreenResume(string resume,string jd)
    {
        var prompt = $@"Return JSON only:
        {{
          ""Score"": number,
          ""Summary"": string,
          ""Decision"": ""Shortlisted"" or ""Rejected""
        }}
        JD: {jd}
        Resume: {resume}";

        var aiResponse = await _provider.GenerateText(prompt);

        var cleanJson = ExtractJson(aiResponse);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var result = JsonSerializer.Deserialize<ScreeningResult>(cleanJson, options);
            return result!;
        }
        catch
        {
            return new ScreeningResult{Score=0,Summary="Parsing failed",Decision="Rejected"};
        }
    }

    string ExtractJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // ✅ Remove markdown code block if present
        input = input.Replace("```json", "")
                     .Replace("```", "")
                     .Trim();

        // ✅ Extract only JSON part
        var start = input.IndexOf("{");
        var end = input.LastIndexOf("}");

        if (start >= 0 && end > start)
        {
            return input.Substring(start, end - start + 1);
        }

        return input;
    }
}
