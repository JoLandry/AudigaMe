using AudioUtils;
using AudioPersistenceService;
using PlaylistService;


var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

var host = Environment.GetEnvironmentVariable("DB_HOST");
var user = Environment.GetEnvironmentVariable("DB_USER");
var pass = Environment.GetEnvironmentVariable("DB_PASSWORD");
var db = Environment.GetEnvironmentVariable("DB_NAME");
var port = Environment.GetEnvironmentVariable("DB_PORT");

var connectionString = $"Host={host};Username={user};Password={pass};Database={db};Port={port}";


builder.Services.AddSingleton<IAudioPersistence>(_ => new AudioPersistence(connectionString));
builder.Services.AddSingleton<IAudioService, AudioServices>();
builder.Services.AddSingleton<IPlaylistManager>(_ => new PlaylistManager(connectionString));


builder.Services.AddControllers();
builder.Services.AddHttpClient();

// For CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5174")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

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

app.UseCors("AllowFrontend");

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// If no API endpoint matches, serve the Angular app's index.html
app.MapFallbackToFile("index.html");

app.Run();