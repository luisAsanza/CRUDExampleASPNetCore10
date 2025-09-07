var builder = WebApplication.CreateBuilder(args);
//Add services for Controllers
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
