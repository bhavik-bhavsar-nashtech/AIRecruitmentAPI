using Microsoft.AspNetCore.Mvc;
using AIRecruitmentAPI.Models;
using AIRecruitmentAPI.Services;

namespace AIRecruitmentAPI.Controllers;

[ApiController]
[Route("")]
public class RecruitmentController : ControllerBase
{
    private readonly IAIService _aiService;

    public RecruitmentController(IAIService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("recruit")]
    public async Task<IActionResult> Recruit([FromBody] RecruitRequest request)
    {
        try
        {
            var screening = await _aiService.ScreenResume(request.Resume, request.JobDescription);

            var response = new Dictionary<string, object>
            {
                ["name"] = request.Name,
                ["screening"] = screening
            };

            if (screening.ToLower().Contains("shortlisted"))
            {
                response["interview"] = ScheduleInterview(request.Name);
                response["offer"] = await _aiService.GenerateOffer(request.Name);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("upload-resume")]
    public async Task<IActionResult> UploadResume(IFormFile resume, string jobDescription, string name)
    {
        try
        {
            using var reader = new StreamReader(resume.OpenReadStream());
            var resumeText = await reader.ReadToEndAsync();

            var screening = await _aiService.ScreenResume(resumeText, jobDescription);

            var response = new Dictionary<string, object>
            {
                ["name"] = name,
                ["screening"] = screening
            };

            if (screening.ToLower().Contains("shortlisted"))
            {
                response["interview"] = ScheduleInterview(name);
                response["offer"] = await _aiService.GenerateOffer(name);
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private object ScheduleInterview(string candidateName)
    {
        return new
        {
            candidate = candidateName,
            date = "2026-06-01",
            time = "10:00 AM"
        };
    }
}
