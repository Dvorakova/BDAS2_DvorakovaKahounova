using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BDAS2_DvorakovaKahounova.Controllers
{
    public class OsobaController : Controller
    {
        private readonly OsobaDataAccess _dataAccess;
        public OsobaController(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("OracleConnection");
            _dataAccess = new OsobaDataAccess(connectionString);
        }

        // Zobrazení registračního formuláře
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Zpracování registrace
        [HttpPost]
        public IActionResult Register(Osoba novaOsoba)
        {
            novaOsoba.TYP_OSOBY = "U";

            if (ModelState.IsValid)
            {
                Console.WriteLine("kontrola mailu v controolleer");
                if (_dataAccess.EmailExists(novaOsoba.EMAIL))
                {
                    ModelState.AddModelError("EMAIL", "Tento email již byl použit.");
                    return View(novaOsoba);
                }

                var (hashedPassword, salt) = HashPassword(novaOsoba.HESLO);
                novaOsoba.HESLO = hashedPassword;
                novaOsoba.SALT = salt;

                if (_dataAccess.RegisterUser(novaOsoba))
                {
                    //když se registrace podaří, zobrazí se uživateli stránka s přihlášením
                    return RedirectToAction("Login", new { email = novaOsoba.EMAIL });
                }
                ModelState.AddModelError("", "Registrace se nezdařila.");
            }
            return View(novaOsoba);
        }

        // Zobrazení přihlašovacího formuláře
        [HttpGet]
        public IActionResult Login(string email)
        {
            ViewBag.Email = email;
            ViewBag.IsLoginAttempted = false;
            return View();
        }

        // Zpracování přihlášení
        [HttpPost]
        //public IActionResult Login(string email, string heslo)
        public async Task<IActionResult> Login(string email, string heslo)
        {
            var osoba = _dataAccess.LoginUser(email, heslo);
            if (osoba != null)
            {

                /////
                //PRIDANO
                // Pokud přihlášení uspěje, nastavíme identity
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, osoba.JMENO),         // Uloží jméno uživatele
                        new Claim(ClaimTypes.Email, osoba.EMAIL),        // Uloží email uživatele
                        new Claim(ClaimTypes.Role, osoba.TYP_OSOBY),     // Uloží typ osoby jako roli
                        new Claim("UserId", osoba.ID_OSOBA.ToString())   // ID uživatele
                    };

                // Vytvoření identity a autentizace
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Uložení identity do cookies pro přetrvání přihlášení
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                /////


                // pokud se přihlášení povede, uživateli se zobrazí stránka (zatím home)
                return RedirectToAction("Index", "Pes");
            }
            ModelState.AddModelError("email", "Nesprávné přihlašovací údaje.");
            ViewBag.Email = email;
            ViewBag.IsLoginAttempted = true; // Informace o pokusu o přihlášení
            return View();
        }

        // Metoda pro hashování hesla
        private (string hashedPassword, string salt) HashPassword(string password)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                // Generování salt
                byte[] saltBytes = new byte[16];
                rng.GetBytes(saltBytes);
                string salt = Convert.ToBase64String(saltBytes);

                // Použití PBKDF2 k hashování hesla se salt
                var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000); // 10,000 iterací
                byte[] hash = pbkdf2.GetBytes(32); // Délka hashe

                // Vrácení hashe a salt jako řetězců
                return (Convert.ToBase64String(hash), salt);
            }
        }

        // Metoda pro odhlášení
        [HttpPost]
        public IActionResult Logout()
        {
            // Odhlásit uživatele
            HttpContext.SignOutAsync(); // Zruší autentifikaci

            // Přesměrovat na domovskou stránku nebo na přihlášení
            return RedirectToAction("Login");
        }

        // Metoda pro zobrazení profilu přihlášeného uživatele
        [HttpGet]
        public IActionResult Profile()
        {
            // Získat email přihlášeného uživatele z claims
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                // Pokud není přihlášený uživatel, přesměrujeme na login
                return RedirectToAction("Login");
            }

            var osoba = _dataAccess.GetUserProfile(email);

            if (osoba != null)
            {
                // Předat model do View
                return View(osoba);
            }

            return RedirectToAction("Login");
        }



        public IActionResult Index()
        {
            return View();
        }
    }
}
