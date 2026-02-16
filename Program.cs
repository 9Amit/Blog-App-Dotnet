using BlogApp.Data;
using Microsoft.EntityFrameworkCore;
using System; // Required for Uri

var builder = WebApplication.CreateBuilder(args);

// --- Connection String Configuration Start ---
string? connectionString = null; // Declare as nullable string initially

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrEmpty(databaseUrl))
{
    // Convert Render PostgreSQL URL to Npgsql format
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');

    connectionString =
        $"Host={uri.Host};" +
        $"Port={uri.Port};" +
        $"Database={uri.AbsolutePath.TrimStart('/')};" +
        $"Username={userInfo[0]};" +
        $"Password={userInfo[1]};" + // Make sure userInfo[1] exists, it should if Split(':') worked
        $"SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    // Local development
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

// ✅ IMPORTANT: Ensure connectionString is not null before use, or throw if critical
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' or 'DATABASE_URL' is missing.");
}

// ✅ Add PostgreSQL with the now guaranteed non-null connectionString
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(connectionString));
// --- Connection String Configuration End ---


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ CORS (Allow all for now)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// ✅ Use CORS
app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

// ✅ Auto-run migrations (important for Render DB)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
    // Check if the database exists before applying migrations
    if (db.Database.GetPendingMigrations().Any())
    {
        db.Database.Migrate();
    }
}

// ✅ Render requires listening on dynamic PORT (This part needs to be below app.MapControllers())
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://*:{port}");

app.Run();