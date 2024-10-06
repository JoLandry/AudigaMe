var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();  // <-- This line is essential to add controller services

var app = builder.Build();

// Serve static files from wwwroot folder
app.UseDefaultFiles();
app.UseStaticFiles();

// Configure routing for the API
app.UseRouting();

app.UseAuthorization();

app.MapControllers();  // <-- This line ensures that the controllers are mapped to endpoints

// If no API endpoint matches, serve the Angular app's index.html
app.MapFallbackToFile("index.html");

app.Run();
