using DotNetEnv;
using Microsoft.OpenApi;
using Viora.Api.Middleware;
using Viora.Application;
using Viora.Infrastructure;



// Load .env first
var cwd = Directory.GetCurrentDirectory();
var projectEnvPath = Path.Combine(cwd, ".env");
var solutionEnvPath = Path.GetFullPath(Path.Combine(cwd, "..", ".env"));

if (File.Exists(projectEnvPath))
    Env.Load(projectEnvPath);
else if (File.Exists(solutionEnvPath))
    Env.Load(solutionEnvPath);


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}



app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "welcome in Viora API");
app.Run();
