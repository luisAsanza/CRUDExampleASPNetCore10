using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);
//Add services for Controllers
builder.Services.AddControllersWithViews();
//Add other services
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonService, PersonService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
