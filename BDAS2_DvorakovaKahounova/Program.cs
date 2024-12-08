using BDAS2_DvorakovaKahounova.DataAcessLayer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddControllersWithViews();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Osoba/Login";       // Cesta k p�ihla�ovac� str�nce, pokud je vy�adov�no p�ihl�en�
        options.AccessDeniedPath = "/Home/Denied"; // Cesta k chybov� str�nce p�i odm�tnut�m p��stupu (voliteln�)
        options.ExpireTimeSpan = TimeSpan.FromHours(1); // Platnost cookies na jednu hodinu (lze upravit podle pot�eby)
        options.SlidingExpiration = true;         // Posouv�n� platnosti p�i aktivit� u�ivatele
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Doba platnosti session
    options.Cookie.HttpOnly = true;                // Zv��en� bezpe�nosti cookies
    options.Cookie.IsEssential = true;             // Pro zaji�t�n� fungov�n� i p�i omezen� cookies
});

var app = builder.Build();

app.UseSession(); // Povolen� session middleware

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Zapnut� autentizace a autorizace v middleware pipeline
app.UseAuthentication(); // P�id�n� autentizace
app.UseAuthorization();  // Zapnut� autorizace


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
