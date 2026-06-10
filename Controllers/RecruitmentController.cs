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


    // ✅ UPDATED METHOD (supports txt, pdf, docx)
    [HttpPost("upload-resume")]
    public async Task<IActionResult> UploadResume(IFormFile resume, string jobDescription, string name)
    {
        try
        {
            if (resume == null || resume.Length == 0)
                return BadRequest("No file uploaded.");

            // ✅ Validate extension
            var allowedExtensions = new[] { ".txt", ".pdf", ".docx" };
            var extension = Path.GetExtension(resume.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Only txt, pdf, docx files are allowed.");

            // ✅ Optional size limit (5MB)
            if (resume.Length > 5 * 1024 * 1024)
                return BadRequest("File too large. Max 5MB allowed.");

            string resumeText;

            using (var stream = resume.OpenReadStream())
            {
                resumeText = await _aiService.ExtractTextFromFile(stream, extension);
            }

            if (string.IsNullOrWhiteSpace(resumeText))
                return BadRequest("Could not extract text from file.");

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
