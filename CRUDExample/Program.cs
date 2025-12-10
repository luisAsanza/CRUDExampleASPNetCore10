using CRUDExample.Middleware;
using Rotativa.AspNetCore;
using Serilog;
using CRUDExample;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureServices();

var app = builder.Build();

// Configure the error handling middleware using the built-in ExceptionHandler middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");

    // Use HSTS in production
    //app.UseHsts();
}

//Configure the error handling middleware using custom middleware for demostration purposes
//(Not recommended, use the built-in ExceptionHandler middleware instead)
//app.UseCustomExceptionHandlingMiddleware();

// Use custom middleware to log request bodies for debugging (Not recommended for production)
app.UseCustomExceptionLoggerMiddleware();

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
//app.MapRazorPages();
app.UseRotativa();

app.Run();

public partial class Program { } //Make the Program class public for integration testing