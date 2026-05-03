using DotNetEnv;
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
//builder.Services.AddOpenApi();

builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
/*
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
    using var scope = app.Services.CreateScope(); // apply pending migrations on startup
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

}
*/


app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "welcome in Viora API");
app.Run();
