using CRUDExample.Filters.ActionFilters;
using Entities;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using Serilog;
using ServiceContracts;
using Services;

namespace CRUDExample
{
    public static class ConfigureServicesExtension
    {
        public static void ConfigureServices(this WebApplicationBuilder? builder)
        {
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

            //Add services for Controllers (Temporarily disabling client-side validation)
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<GlobalActionFilter>();
            }).AddViewOptions(vo => vo.HtmlHelperOptions.ClientValidationEnabled = true);

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
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            //Routing configuration for consistency
            builder.Services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });
        }
    }
}
