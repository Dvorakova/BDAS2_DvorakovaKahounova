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

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View();
        }

        // Zpracování registrace
        [HttpPost]
        public IActionResult Register(Osoba novaOsoba)
        {
            novaOsoba.TYP_OSOBY = "U";

            if (ModelState.IsValid)
            {
                if (_dataAccess.EmailExists(novaOsoba.EMAIL))
                {
                    ModelState.AddModelError("EMAIL", "Tento email již byl použit.");

					var originalllRole = HttpContext.Session.GetString("OriginalRole");
					ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalllRole == "A" || User.IsInRole("A")));

					return View(novaOsoba);
                }

                var (hashedPassword, salt) = HashPassword(novaOsoba.HESLO);
                novaOsoba.HESLO = hashedPassword;
                novaOsoba.SALT = salt;
                
                if (_dataAccess.RegisterUser(novaOsoba))
                {
                    if (User.Identity.IsAuthenticated && User.IsInRole("C"))
                    {
                        //v případě, že osobu registroval chovatel, je chovatel vrácen na jeho hlavní stranu
						return RedirectToAction("Index", "Chovatele");
					}
					//když se registrace podaří, zobrazí se uživateli stránka s přihlášením
					return RedirectToAction("Login", new { email = novaOsoba.EMAIL });
                }
                ModelState.AddModelError("", "Registrace se nezdařila.");
            }

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View(novaOsoba);
        }

        // Zobrazení přihlašovacího formuláře
        [HttpGet]
        public IActionResult Login(string email)
        {
            ViewBag.Email = email;
            ViewBag.IsLoginAttempted = false;

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View();
        }

        // Zpracování přihlášení
        [HttpPost]
        public async Task<IActionResult> Login(string email, string heslo)
        {
            var osoba = _dataAccess.LoginUser(email, heslo);
            if (osoba != null)
            {
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

                // pokud se přihlášení povede, uživateli se zobrazí stránka
                if (claimsPrincipal.IsInRole("C"))
                {
					return RedirectToAction("Index", "Chovatele");
				}
				if (claimsPrincipal.IsInRole("A"))
				{
					return RedirectToAction("Index", "Home");
				}

				return RedirectToAction("PsiKAdopci", "Pes");
            }
            ModelState.AddModelError("email", "Nesprávné přihlašovací údaje.");
            ViewBag.Email = email;
            ViewBag.IsLoginAttempted = true; // Informace o pokusu o přihlášení

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

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
            HttpContext.SignOutAsync(); // Zruší autentifikaci

			// Přesměrovat  na přihlášení
			return RedirectToAction("Login");
        }

        // Metoda pro zobrazení profilu přihlášeného uživatele
        [HttpGet]
        public IActionResult Profile(int? userId = null)
        {
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            Osoba osoba;

			if (userId.HasValue)
			{
				// Načtení profilu na základě ID uživatele
				osoba = _dataAccess.GetUserProfileById(userId.Value);
				if (osoba == null)
				{
					return NotFound(); // Uživatelský profil nenalezen
				}

				var currentUserId = GetLoggedInUserId();
				ViewBag.IsOwner = currentUserId == userId.Value; // Kontrola, zda jde o vlastní účet
				

				ViewBag.CanEditNotChovatel = false; // Zakázat úpravy
			} else { 

			// Získat email přihlášeného uživatele z claims
			var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
					// Pokud není uživatel přihlášený, přesměrujeme na login
					return RedirectToAction("Login");
            }

            osoba = _dataAccess.GetUserProfile(email);

            if (osoba == null)
            {
					return RedirectToAction("Login");
				}
				ViewBag.InvalidPassword = false;
				ViewBag.CanEditNotChovatel = true; // Umožnit úpravy

				var currentUserId = GetLoggedInUserId();
				ViewBag.IsOwner = true; // Když uživatel načítá svůj vlastní účet, vždy je owner
				
			}

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View(osoba);
		}

		private int GetLoggedInUserId()
		{
			var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
			return userIdClaim != null ? int.Parse(userIdClaim) : 0;
		}


		// Pro kontrolu hesla
		[HttpPost]
        public IActionResult CheckPassword(string password)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
				return RedirectToAction("Login");
            }

            var osoba = _dataAccess.LoginUser(email, password);
            if (osoba != null)
            {
                ViewBag.CanEdit = true;
                ViewBag.InvalidPassword = false;
                ViewBag.IsOwner = true;
                var originalllRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalllRole == "A" || User.IsInRole("A")));

				return View("Profile", osoba); // Zobrazí profil s možností úprav
            }

            // Nastavíme chybovou hlášku a zůstaneme na stránce
            ViewBag.ErrorMessage = "Nesprávné heslo. Zkuste to znovu.";
            ViewBag.InvalidPassword = true;
            ViewBag.CanEdit = false;
            osoba = _dataAccess.GetUserProfile(email); // Znovu načteme data o uživateli

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View("Profile", osoba);
        }

        // Pro aktualizaci profilu
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Osoba updatedOsoba)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }
			
            // Získáme ID aktuálně přihlášeného uživatele z claims
			var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null)
            {
                updatedOsoba.ID_OSOBA = int.Parse(userIdClaim.Value); // Předáme ID do updatedOsoba
            }
            else
            {
				// Pokud se nepodaří najít ID, můžeme uživatele přesměrovat zpět na login
				return RedirectToAction("Login");
            }

            // Načteme uživatelský profil podle emailu
            var currentUser = _dataAccess.GetUserProfile(email);
            if (currentUser != null)
            {
                // Pokud uživatel existuje, nastavíme jeho stávající heslo do updatedOsoba
                updatedOsoba.HESLO = currentUser.HESLO; // Předáme původní heslo
            }

            if (_dataAccess.UpdateUserProfile(updatedOsoba))
            {
                // Pokud byla aktualizace úspěšná, nastavíme nové údaje do claims
                var identity = (ClaimsIdentity)User.Identity;

                // Odstraníme staré hodnoty z claims
                identity.RemoveClaim(identity.FindFirst(ClaimTypes.Name));
                identity.RemoveClaim(identity.FindFirst(ClaimTypes.Email));

                // Přidáme nové hodnoty
                identity.AddClaim(new Claim(ClaimTypes.Name, updatedOsoba.JMENO));
                identity.AddClaim(new Claim(ClaimTypes.Email, updatedOsoba.EMAIL));

                // Vytvoříme nový ClaimsPrincipal s aktuálními claims
                var principal = new ClaimsPrincipal(identity);

                // Uložíme nový ClaimsPrincipal do cookies pro autentizaci
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

				TempData["SuccessMessage"] = "Profil úspěšně aktualizován.";
                return RedirectToAction("Profile");
            }

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

			ModelState.AddModelError("", "Aktualizace se nezdařila.");
            ViewBag.CanEdit = true;
            return View("Profile", updatedOsoba);
        }



        public IActionResult Index()
        {

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View();
        }

    }
}
