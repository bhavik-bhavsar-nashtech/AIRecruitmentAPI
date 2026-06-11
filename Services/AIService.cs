using System.Text.Json;
using AIRecruitmentAPI.Core;

namespace AIRecruitmentAPI.Services;

public class AIService : IAIService
{
    private readonly IModelProvider _provider;

    public AIService(IModelProvider provider)
    {
        _provider = provider;
    }

    public async Task<ScreeningResult> ScreenResume(string resume, string jd)
    {
        var prompt = $@"
            You are an AI resume evaluator.

            TASK:
            Evaluate how well the candidate's resume matches the given Job Description.

            SCORING GUIDELINES:
            - Score MUST be an integer between 0 and 100
            - 100 = Perfect match (skills, experience, responsibilities aligned)
            - 70–90 = Strong match
            - 50–69 = Moderate match
            - 30–49 = Weak match
            - 0–29 = Poor match

            RULES:
            - Always score out of 100 (NOT out of 10)
            - Be consistent and objective
            - Base score strictly on:
              • Skill match
              • Experience relevance
              • Role alignment
            - Do NOT randomly assign scores
            - Do NOT be overly generous or overly strict

            DECISION RULE:
            - Score >= 60 → Shortlisted
            - Score < 60 → Rejected

            IMPORTANT:
            - Return ONLY valid JSON
            - Do NOT include markdown or explanation
            - Output must start with {{ and end with }}

            OUTPUT FORMAT:
            {{
              ""Score"": number,
              ""Summary"": string,
              ""Decision"": ""Shortlisted"" or ""Rejected""
            }}

            JD:
            {jd}

            Resume:
            {resume}
            ";

        var aiResponse = await _provider.GenerateText(prompt);

        var cleanJson = ExtractJson(aiResponse);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var result = JsonSerializer.Deserialize<ScreeningResult>(cleanJson, options);

            if (result == null)
                throw new Exception("Null result");

            // ✅ Normalize score (safety)
            if (result.Score > 100) result.Score = 100;
            if (result.Score < 0) result.Score = 0;

            // ✅ Enforce decision consistency
            result.Decision = result.Score >= 60 ? "Shortlisted" : "Rejected";

            return result;
        }
        catch
        {
            return new ScreeningResult
            {
                Score = 0,
                Summary = "Parsing failed",
                Decision = "Rejected"
            };
        }
    }

    string ExtractJson(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // ✅ Remove markdown wrappers
        input = input.Replace("```json", "")
                     .Replace("```", "")
                     .Trim();

        // ✅ Extract JSON block
        var start = input.IndexOf("{");
        var end = input.LastIndexOf("}");

        if (start >= 0 && end > start)
        {
            return input.Substring(start, end - start + 1);
        }

        return input;
    }
}