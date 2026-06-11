using AIRecruitmentAPI.Core;
using AIRecruitmentAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIRecruitmentAPI.Controllers;

[ApiController]
[Route("")]
public class RecruitmentController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly IFileService _fileService;

    public RecruitmentController(IAIService aiService, IFileService fileService)
    {
        _aiService = aiService;
        _fileService = fileService;
    }

    // ✅ JSON-based API (existing)
    [HttpPost("recruit")]
    public async Task<IActionResult> Evaluate([FromBody] RequestModel req)
    {
        var result = await _aiService.ScreenResume(req.Resume, req.JobDescription);
        return Ok(result);
    }

    // ✅ ✅ FILE UPLOAD API (NEW - Swagger will show this)
    [HttpPost("upload-resume")]
    public async Task<IActionResult> UploadResume(
        IFormFile resume,
        string jobDescription,
        string name)
    {
        if (resume == null || resume.Length == 0)
            return BadRequest("No file uploaded");

        var extension = Path.GetExtension(resume.FileName).ToLower();

        var allowed = new[] { ".txt", ".pdf", ".docx" };
        if (!allowed.Contains(extension))
            return BadRequest("Only txt, pdf, docx allowed");

        string resumeText;

        using (var stream = resume.OpenReadStream())
        {
            resumeText = await _fileService.ExtractText(stream, extension);
        }

        var result = await _aiService.ScreenResume(resumeText, jobDescription);

        return Ok(new
        {
            candidate = name,
            result
        });
    }


    [HttpPost("upload-resume-text")]
    public async Task<IActionResult> UploadResumeText([FromBody] ResumeRequest req)
    {
        var result = await _aiService.ScreenResume(req.ResumeText, req.JobDescription);

        return Ok(new
        {
            candidate = req.Name,
            result
        });
    }
}

// ✅ Request model
public class RequestModel
{
    public string Resume { get; set; }
    public string JobDescription { get; set; }
}



public class ResumeRequest
{
    public string Name { get; set; }
    public string JobDescription { get; set; }
    public string ResumeText { get; set; }
}
