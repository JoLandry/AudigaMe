using AudioObjects;
using AudioUtils;
using AudioPersistenceService;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

var metadataFilePath = Path.Combine(Directory.GetCurrentDirectory(),"src","resources","audioMetadata.json");

builder.Services.AddSingleton<IAudioPersistence>(_ => new AudioPersistence(metadataFilePath));
builder.Services.AddSingleton<IAudioService, AudioServices>();

builder.Services.AddControllers();
builder.Services.AddHttpClient();

var app = builder.Build();

try
{
    var audioServices = app.Services.GetRequiredService<IAudioService>();
    if(audioServices is AudioServices audioServicesInstance)
    {
        await audioServicesInstance.InitializeAsync();
    }
}
catch(Exception e)
{
    Console.WriteLine($"Error initializing audio services: {e.Message}");
}

// Serve static files from wwwroot folder
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// If no API endpoint matches, serve the Angular app's index.html
app.MapFallbackToFile("index.html");

app.Run();