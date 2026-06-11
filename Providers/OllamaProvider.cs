
using System.Text.Json;
using System.Text;
using AIRecruitmentAPI.Core;

namespace AIRecruitmentAPI.Providers;
public class OllamaProvider:IModelProvider
{
    private readonly HttpClient _http=new();
    public async Task<string> GenerateText(string prompt)
    {
        var body=new{model="llama3",prompt=prompt,stream=false};
        var res=await _http.PostAsync("http://localhost:11434/api/generate",
            new StringContent(JsonSerializer.Serialize(body),Encoding.UTF8,"application/json"));
        var json=await res.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.GetProperty("response").GetString();
    }
}
