using BDAS2_DvorakovaKahounova.DataAcessLayer;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

/////

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Osoba/Login";       // Cesta k pøihlašovací stránce, pokud je vyžadováno pøihlášení
        options.AccessDeniedPath = "/Home/Denied"; // Cesta k chybové stránce pøi odmítnutém pøístupu (volitelné)
        options.ExpireTimeSpan = TimeSpan.FromHours(1); // Platnost cookies na jednu hodinu (lze upravit podle potøeby)
        options.SlidingExpiration = true;         // Posouvání platnosti pøi aktivitì uživatele
    });

/////

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Zapnutí autentizace a autorizace v middleware pipeline
app.UseAuthentication(); // Pøidání autentizace
app.UseAuthorization();  // Zapnutí autorizace
//app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
