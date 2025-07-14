using PanchangaApi.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Panchanga API",
        Version = "v1",
        Description = "API for calculating Hindu Panchanga (traditional calendar) information",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Panchanga API",
            Email = "reachus@arisecraft.com"
        }
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register services
builder.Services.AddScoped<IAstronomicalService, AstronomicalService>();
builder.Services.AddScoped<ISanskritNamesService, SanskritNamesService>();
builder.Services.AddScoped<IPanchangaService, PanchangaService>();

// Add memory cache
builder.Services.AddMemoryCache();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Panchanga API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    c.DocumentTitle = "Panchanga API Documentation";
});

app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Add a simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Add API info endpoint
app.MapGet("/api", () => Results.Ok(new { 
    name = "Panchanga API",
    version = "v1",
    description = "API for calculating Hindu Panchanga (traditional calendar) information",
    documentation = "/swagger",
    endpoints = new {
        panchanga = "/api/panchanga",
        tithi = "/api/elements/tithi",
        nakshatra = "/api/elements/nakshatra",
        yoga = "/api/elements/yoga",
        karana = "/api/elements/karana",
        vara = "/api/elements/vara",
        health = "/health"
    }
}));

app.Run();
