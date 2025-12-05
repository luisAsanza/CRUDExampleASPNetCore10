using CRUDExample.Middleware;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using Rotativa.AspNetCore;
using ServiceContracts;
using Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// app.UseHttpLogging(); logs every http request. With AddHttpLogging I can overwrite what information
// of the HttpRequest is logged.
//builder.Services.AddHttpLogging(options =>
//{
//    options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
//                            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
//                            Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestQuery;
//                            //Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestHeaders |
//                            //Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode |
//                            //Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseHeaders;
//});

//Select built-in Logging Providers (Commented out to use Serilog instead)
//builder.Logging.ClearProviders().AddConsole().AddDebug();
//if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
//{
//    builder.Logging.AddEventLog();
//}

//Instead of selecting built-in Logging Providers, you can also use third-party logging providers such as Serilog, NLog, etc.
builder.Host.UseSerilog((context, sp, loggerConfiguration) => {
    loggerConfiguration
    .ReadFrom.Services(sp)
    .Enrich.WithMachineName();

    if (!context.HostingEnvironment.IsEnvironment("Testing"))
    {
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    }
});

//Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Add services for Controllers (Temporarily disabling client-side validation)
builder.Services.AddControllersWithViews()
    .AddViewOptions(vo => vo.HtmlHelperOptions.ClientValidationEnabled = true);

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

// Use Serilog custom middleware to enrich logs with Username
app.Use(async (ctx, next) =>
{
    var isAuthenticated = ctx.User.Identity?.IsAuthenticated == true;
    var userName = isAuthenticated ? ctx.User.Identity?.Name : "Anonymous";
    var diagnosticContext = ctx.RequestServices.GetRequiredService<IDiagnosticContext>();
    diagnosticContext.Set("Username", userName!);

    await next();
});

// Log every http request. I'm commented out this line to use Serilog request logging instead.
//app.UseHttpLogging();

// Log every request using Serilog. app.UseHttpLogging(); can be removed so it won't generate http requests logs twice
app.UseSerilogRequestLogging();


app.Logger.LogDebug("Adding Csp configuration");

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

app.Logger.LogDebug("End of Csp configuration");

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.UseRotativa();

app.Run();

public partial class Program { } //Make the Program class public for integration testing