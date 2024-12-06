using BDAS2_DvorakovaKahounova.DataAcessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BDAS2_DvorakovaKahounova.Models;
using System;
using System.Security.Cryptography;

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
            ViewBag.Message = TempData["Message"];
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            // Zkontroluje, zda je uživatel přihlášen a má typ osoby "R"
            if (User.Identity.IsAuthenticated && (User.IsInRole("R") || User.IsInRole("P")))
            {
                List<Rezervace> rezervaceList = new List<Rezervace>();
                try
                {
                    // Získání rezervací pro přihlášeného uživatele
                    rezervaceList = _dataAccess.GetRezervace(uzivatelID);
                }
                catch (Exception)
                {
					ViewBag.Message = "Chyba při načítání rezervací uživatele.";
				}                
                var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(rezervaceList);  // Předáme seznam rezervací do View
            }
			// Pokud podmínky nejsou splněny, je uživatel přesměrován na přihlášení
			return RedirectToAction("Login", "Osoba");
        }

        public IActionResult VyriditRezervaci()
        {
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            // Zkontroluje, zda je uživatel přihlášen a má typ osoby "R"
            if (User.Identity.IsAuthenticated && User.IsInRole("C"))
            {

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(); // Zobrazí stránku rezervací
            }
			// Pokud podmínky nejsou splněny, přesměruje uživatele na přihlášení
			return RedirectToAction("Login", "Osoba");
        }

        [HttpPost]
        [Authorize(Roles = "C")]
        public IActionResult VyriditRezervaci(string rezervaceKod, string action, int idPes)
        {

            // Zkontroluje, zda je uživatel přihlášen a má typ osoby "C"
            if (User.Identity.IsAuthenticated && User.IsInRole("C"))
            {
                if (action == "vyhledatPsa")
                {


                    Pes pes = new Pes();
                    if (!string.IsNullOrEmpty(rezervaceKod))
                    {
                        //nalezení id psa
                        int? idPsa = _dataAccess.ZiskejIdPsa(rezervaceKod);

                        if (idPsa.HasValue)
                        {
                            //vypsání informací o nalezeném psovi
                            ViewBag.Message = "Pes nalezen";
                            pes = _dataAccess.ZobrazInfoOPsovi(idPsa.Value);

                            // Zjištění stavu karantény
                            string karantenaStatus = _dataAccess.ZjistiKarantenu(idPsa.Value);
                            ViewBag.KarantenaStatus = karantenaStatus;
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

					var originallRole = HttpContext.Session.GetString("OriginalRole");
					ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

					return View(pes);
                }
                else if (action == "vyresitRezervaci")
                {
                    try
                    {
                        int majitelIdOsoba = _dataAccess.ZiskejIdRezervatora(idPes);
                        _dataAccess.ZpracujRezervaciVDatabazi(majitelIdOsoba, idPes);
					}
                    catch (Exception ex)
                    {
                        TempData["Message"] = "Rezervace se nezdařila."; ;
					}

					return RedirectToAction("Index", "Chovatele");

				}
            }
			// Pokud podmínky nejsou splněny, přesměruje uživatele na přihlášení
			return RedirectToAction("Login", "Osoba");
        }

        //metoda pro získání id přihlášeného uživatele
        private int GetLoggedInUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : 0;
        }

    }

}
