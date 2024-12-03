using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BDAS2_DvorakovaKahounova.Controllers
{
	public class AdminController : Controller
    {
        private readonly AdminDataAccess _dataAccess;
        public AdminController(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("OracleConnection");
            _dataAccess = new AdminDataAccess(connectionString);
        }

        // Slovník popisů rolí a jejich zkratek
        private readonly Dictionary<string, string> roleMap = new()
        {
            { "Majitel", "M" },
            { "Rezervátor", "R" },
            { "Chovatel", "C" },
            { "Přihlášený uživatel", "U" },
            { "Majitel i rezervátor", "P" }
        };


        public IActionResult Index()
		{
			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View();
        }
        public IActionResult Logovani()
        {
            var originallRole = HttpContext.Session.GetString("OriginalRole");
            ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));
			List<Log> logs = _dataAccess.GetLogs();
			return View(logs);
			//return View();
        }
        public IActionResult Emulace()
        {
            // Předání klíčů slovníku (popisů rolí) do pohledu
            var roles = roleMap.Keys.ToList();

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

			return View(roles);
        }

        [HttpPost]
        public IActionResult Emulovat(string selectedRole)
        {
            // Uložení původní role, pokud ještě není uložená
            if (HttpContext.Session.GetString("OriginalRole") == null)
            {
                var originalRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                HttpContext.Session.SetString("OriginalRole", originalRole ?? "A"); // Defaultní původní role: Admin
            }

            // Uložení nové emulované role do session
            HttpContext.Session.SetString("EmulatedRole", selectedRole);

            // Získání zkratky role ze slovníku
            if (roleMap.TryGetValue(selectedRole, out var roleCode))
            {
                UpdateUserClaims(roleCode); // Aktualizace claims na základě zkratky role
            }

			return RedirectToAction("Index", "Home");
        }

        public IActionResult ResetRole()
        {
            // Obnovení původní role
            var originalRole = HttpContext.Session.GetString("OriginalRole");
            if (originalRole != null)
            {
                HttpContext.Session.Remove("EmulatedRole");
                HttpContext.Session.Remove("OriginalRole");

                UpdateUserClaims(originalRole); // Aktualizace claims zpět na původní roli
            }

			return RedirectToAction("Index", "Home");
        }

        //private void UpdateUserClaims(string role)
        //{
        //    var claims = User.Claims.Where(c => c.Type != ClaimTypes.Role).ToList(); // Odebrání staré role
        //    claims.Add(new Claim(ClaimTypes.Role, role)); // Přidání nové role

        //    var identity = new ClaimsIdentity(claims, "Cookie");
        //    var principal = new ClaimsPrincipal(identity);

        //    HttpContext.SignInAsync("Cookie", principal).Wait(); // Přepis identity v cookies
        //}

        private void UpdateUserClaims(string role)
        {
            // Odstranění předchozí role
            var claims = User.Claims.Where(c => c.Type != ClaimTypes.Role).ToList();

            // Přidání nové role
            claims.Add(new Claim(ClaimTypes.Role, role));

            // Vytvoření nové identity s novými claims a schématem cookies
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Přiřazení nové identity uživatelskému kontextu
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait(); // Přepis identity v cookies
        }



    }
}
