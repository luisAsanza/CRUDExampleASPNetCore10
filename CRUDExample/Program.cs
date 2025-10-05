using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

//Add services for Controllers
builder.Services.AddControllersWithViews();
//Add other services
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonService, PersonService>();

//DB
builder.Services.AddDbContext<Entities.PersonsDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
