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
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            // Zkontroluj, zda je uživatel přihlášen a má typ osoby "R"
            if (User.Identity.IsAuthenticated && (User.IsInRole("R") || User.IsInRole("P")))
            {
                // Získání rezervací pro přihlášeného uživatele
                List<Rezervace> rezervaceList = _dataAccess.GetRezervace(uzivatelID);

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(rezervaceList);  // Předáme seznam rezervací do View
            }

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

			// Pokud podmínky nejsou splněny, přesměruj na přihlášení nebo jinou stránku
			return RedirectToAction("Login", "Osoba");
        }

        public IActionResult VyriditRezervaci()
        {
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            // Zkontroluj, zda je uživatel přihlášen a má typ osoby "R"
            if (User.Identity.IsAuthenticated && User.IsInRole("C"))
            {

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(); // Zobrazí stránku rezervací
            }

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

			// Pokud podmínky nejsou splněny, přesměruj na přihlášení nebo jinou stránku
			return RedirectToAction("Login", "Osoba");
        }

        [HttpPost]
        [Authorize(Roles = "C")]
        public IActionResult VyriditRezervaci(string rezervaceKod, string action, int idPes/*, int majitelIdOsoba*/)
        {

            // Zkontroluj, zda je uživatel přihlášen a má typ osoby "C"
            if (User.Identity.IsAuthenticated && User.IsInRole("C"))
            {
                if (action == "vyhledatPsa")
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

                            // Zjistíme stav karantény
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
                    int majitelIdOsoba = _dataAccess.ZiskejIdRezervatora(idPes);
                    Console.WriteLine("id psa: ve VyriditRezervaci: " + idPes);
                    Console.WriteLine("id osoby: ve VyriditRezervaci: " + majitelIdOsoba);
                    //try
                    //{
                    // Zavolání metody z DataAccess
                    _dataAccess.ZpracujRezervatorZmena(majitelIdOsoba, idPes);
                    _dataAccess.PridejMajitele(majitelIdOsoba);
                    _dataAccess.PridejAdopci(idPes, majitelIdOsoba);
                    _dataAccess.AktualizujKonecPobytu(idPes);

					var originallRole = HttpContext.Session.GetString("OriginalRole");
					ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

					return RedirectToAction("Index", "Chovatele");
                }
            }

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

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
