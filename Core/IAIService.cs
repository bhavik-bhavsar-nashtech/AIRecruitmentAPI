
namespace AIRecruitmentAPI.Core;
public interface IAIService
{
    Task<ScreeningResult> ScreenResume(string resume,string jd);
}
