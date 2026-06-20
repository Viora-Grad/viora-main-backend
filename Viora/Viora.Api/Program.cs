using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Viora.Api.Middleware;
using Viora.Application;
using Viora.Application.Abstractions.Mail;
using Viora.Application.Abstractions.Media;
using Viora.Domain.Organizations.OnBoardings;
using Viora.Domain.Organizations.Suspensions;
using Viora.Domain.Scheduling;
using Viora.Domain.Services;
using Viora.Infrastructure;
using Viora.Infrastructure.AiRag;
using Viora.Infrastructure.RealTime.Hubs;
using Viora.Infrastructure.Seeding;
using Viora.Infrastructure.Settings;


// Load .env first
var cwd = Directory.GetCurrentDirectory();
var envPath = Path.Combine(cwd, ".env");
Env.Load(envPath);


var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAiRagServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

#region Settings
builder.Services.AddInterfacedOptions<ISchedulingSettings, SchedulingSettings>(
    builder.Configuration, "Scheduling");
builder.Services.AddInterfacedOptions<IStorageSettings, StorageConfigurations>(
    builder.Configuration, "Storage");
builder.Services.AddInterfacedOptions<IOnboardingSettings, OnboardingSettings>(
    builder.Configuration, "Onboarding");
builder.Services.AddInterfacedOptions<ISuspensionSettings, SuspensionSettings>(
    builder.Configuration, "Suspension");
builder.Services.AddInterfacedOptions<IServiceSettings, ServiceSettings>(
    builder.Configuration, "Service");
builder.Services.AddInterfacedOptions<IEmailSettings, EmailSettings>(
    builder.Configuration, "Email");
builder.Services.AddSignalR();
#endregion Settings

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    using var scope = app.Services.CreateScope(); // apply pending migrations on startup
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    await seeder.SeedAsync();
}

// Skipped in Docker — no dev cert available
// app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ScheduleHub>("/realtime-scheduling");

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? throw new InvalidOperationException("AllowedOrigins configuration is missing.");
app.UseCors(builder =>
    builder.WithOrigins(allowedOrigins)
           .AllowAnyHeader()
           .AllowAnyMethod()
           .AllowCredentials());
app.Run();
