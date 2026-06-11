namespace AIRecruitmentAPI.Core;

public interface IFileService
{
    Task<string> ExtractText(Stream fileStream, string extension);
}