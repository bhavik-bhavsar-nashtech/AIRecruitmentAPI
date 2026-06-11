using AIRecruitmentAPI.Core;
using AIRecruitmentAPI.Services;
using AIRecruitmentAPI.Providers;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add Controllers
builder.Services.AddControllers();

// ✅ Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Provider selection
var provider = builder.Configuration["Provider"];

if (provider == "Ollama")
    builder.Services.AddScoped<IModelProvider, OllamaProvider>();
else
    builder.Services.AddScoped<IModelProvider, OpenAICompatibleProvider>();

builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

// ✅ Enable Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();