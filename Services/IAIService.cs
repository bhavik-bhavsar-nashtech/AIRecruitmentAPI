namespace AIRecruitmentAPI.Services;

public interface IAIService
{
    Task<string> ScreenResume(string resume, string jobDescription);
    Task<string> GenerateOffer(string candidateName);

    // ✅ NEW METHOD
    Task<string> ExtractTextFromFile(Stream fileStream, string extension);
}
