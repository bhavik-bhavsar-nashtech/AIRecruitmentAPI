
namespace AIRecruitmentAPI.Core;
public interface IModelProvider
{
    Task<string> GenerateText(string prompt);
}
