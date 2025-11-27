using CRUDExample.Middleware;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using Rotativa.AspNetCore;
using ServiceContracts;
using Services;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//Logging
builder.Logging.ClearProviders().AddConsole().AddDebug();
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Logging.AddEventLog();
}

//Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Add services for Controllers
builder.Services.AddControllersWithViews();

//Add memory cache
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

//Add other services
builder.Services.AddScoped<ICountriesRepository, CountriesRepository>();
builder.Services.AddScoped<IPersonsRepository, PersonsRepository>();
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonService, PersonService>();

//DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

//Routing configuration for consistency
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
});

var app = builder.Build();

//Add csp to responses
if (app.Environment.IsDevelopment())
{
    // Option A: no CSP
    // Option B: very relaxed CSP or Report-Only
    app.UseCspReportOnly();
}
else if (app.Environment.IsStaging())
{
    // Real policy, but report-only to see violations
    app.UseCspReportOnly();
}
else if (app.Environment.IsProduction())
{
    // Real, enforced CSP
    app.UseCsp();
}

app.Logger.LogDebug("debug-message xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
app.Logger.LogInformation("information-message xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
app.Logger.LogWarning("warning-message xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
app.Logger.LogError("error-message xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
app.Logger.LogCritical("critical-message xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.UseRotativa();

app.Run();

public partial class Program { } //Make the Program class public for integration testing