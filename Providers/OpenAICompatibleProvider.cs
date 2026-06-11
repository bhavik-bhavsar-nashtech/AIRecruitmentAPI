
using System.Text;
using System.Text.Json;
using AIRecruitmentAPI.Core;

namespace AIRecruitmentAPI.Providers;
public class OpenAICompatibleProvider:IModelProvider
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http=new();
    public OpenAICompatibleProvider(IConfiguration config){_config=config;}

    public async Task<string> GenerateText(string prompt)
    {
        var body = new
        {
            model = _config["AI:Model"],
            input = prompt
        };

        _http.DefaultRequestHeaders.Clear();
        _http.DefaultRequestHeaders.Add("api-key", _config["AI:Key"]);

        Console.WriteLine("Request JSON:");
        Console.WriteLine(JsonSerializer.Serialize(body));

        var res = await _http.PostAsync(
            _config["AI:Endpoint"],
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        );

        var json = await res.Content.ReadAsStringAsync();

        // ✅ Add this block
        if (!res.IsSuccessStatusCode)
        {
            throw new Exception($"AI API Error: {res.StatusCode}\nResponse: {json}");
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new Exception("AI API returned empty response");
        }


        

        Console.WriteLine("Response:");
        Console.WriteLine(json);


        var doc = JsonDocument.Parse(json);

        var output = doc.RootElement
            .GetProperty("output")[0]
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();
        return output!;

        //return JsonDocument.Parse(json)
        //    .RootElement.GetProperty("choices")[0]
        //    .GetProperty("message")
        //    .GetProperty("content")
        //    .GetString();
    }
}
