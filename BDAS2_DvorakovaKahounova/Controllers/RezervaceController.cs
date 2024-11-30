using BDAS2_DvorakovaKahounova.DataAcessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BDAS2_DvorakovaKahounova.Models;

namespace BDAS2_DvorakovaKahounova.Controllers
{
    public class RezervaceController : Controller
    {
        private readonly RezervaceDataAccess _dataAccess;

        public RezervaceController(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("OracleConnection");
            _dataAccess = new RezervaceDataAccess(connectionString);
        }

        // Akce pro zobrazení stránky rezervací
        [HttpGet]
        [Authorize(Roles = "R, P")]
        public IActionResult Index()
        {
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {
                return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            // Zkontroluj, zda je uživatel přihlášen a má typ osoby "R"
            if (User.Identity.IsAuthenticated && (User.IsInRole("R") || User.IsInRole("P")))
            {
                return View(); // Zobrazí stránku rezervací
            }

            // Pokud podmínky nejsou splněny, přesměruj na přihlášení nebo jinou stránku
            return RedirectToAction("Login", "Osoba");
        }

		public IActionResult VyriditRezervaci()
		{
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {
                return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            // Zkontroluj, zda je uživatel přihlášen a má typ osoby "R"
            if (User.Identity.IsAuthenticated && User.IsInRole("C"))
			{
				return View(); // Zobrazí stránku rezervací
			}

			// Pokud podmínky nejsou splněny, přesměruj na přihlášení nebo jinou stránku
			return RedirectToAction("Login", "Osoba");
		}

        [HttpPost]
        [Authorize(Roles = "C")]
        public IActionResult VyriditRezervaci(string rezervaceKod)
        {
            // Zkontroluj, zda je uživatel přihlášen a má typ osoby "C"
            if (User.Identity.IsAuthenticated && User.IsInRole("C"))
            {
                Pes pes = new Pes();
                if (!string.IsNullOrEmpty(rezervaceKod))
                {
                    // Zavolej metodu ZiskejIdPsa
                    int? idPsa = _dataAccess.ZiskejIdPsa(rezervaceKod);

                    if (idPsa.HasValue)
                    {
                        ViewBag.Message = "Pes nalezen";
                        pes = _dataAccess.ZobrazInfoOPsovi(idPsa.Value);

                    }
                    else
                    {
                        ViewBag.Message = "Pes nebyl nalezen.";
                    }
                }
                else
                {
                    ViewBag.Message = "Zadejte kód rezervace.";
                }

                return View(pes);
            }

            // Pokud podmínky nejsou splněny, přesměruj na přihlášení nebo jinou stránku
            return RedirectToAction("Login", "Osoba");
        }

        private int GetLoggedInUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }
    }
}
