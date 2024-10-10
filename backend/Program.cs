var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient();

var app = builder.Build();

// Serve static files from wwwroot folder
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure routing for the API
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// If no API endpoint matches, serve the Angular app's index.html
app.MapFallbackToFile("index.html");

app.Run();