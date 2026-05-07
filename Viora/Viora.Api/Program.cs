using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Viora.Api.Middleware;
using Viora.Application;
using Viora.Application.Abstractions.Media;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.Suspensions;
using Viora.Domain.Scheduling;
using Viora.Infrastructure;
using Viora.Infrastructure.Settings;


// Load .env first
var cwd = Directory.GetCurrentDirectory();
var envPath = Path.Combine(cwd, ".env");
Env.Load(envPath);


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Settings
builder.Services.AddInterfacedOptions<ISchedulingSettings, SchedulingSettings>(
    builder.Configuration, "Scheduling");
builder.Services.AddInterfacedOptions<IStorageConfiguration, StorageConfigurations>(
    builder.Configuration, "Storage");
builder.Services.AddInterfacedOptions<IOnboardingSettings, OnboardingSettings>(
    builder.Configuration, "Onboarding");
builder.Services.AddInterfacedOptions<ISuspensionSettings, SuspensionSettings>(
    builder.Configuration, "Suspension");
#endregion Settings

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();

    using var scope = app.Services.CreateScope(); // apply pending migrations on startup
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "welcome in Viora API");
app.Run();
