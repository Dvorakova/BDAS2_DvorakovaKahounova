﻿using BDAS2_DvorakovaKahounova.DataAcessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [Authorize(Roles = "R")]
        public IActionResult Index()
        {
            // Zkontroluj, zda je uživatel přihlášen a má typ osoby "R"
            if (User.Identity.IsAuthenticated && User.IsInRole("R"))
            {
                return View(); // Zobrazí stránku rezervací
            }

            // Pokud podmínky nejsou splněny, přesměruj na přihlášení nebo jinou stránku
            return RedirectToAction("Login", "Osoba");
        }

		public IActionResult VyriditRezervaci()
		{
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
                if (!string.IsNullOrEmpty(rezervaceKod))
                {
                    // Zavolej metodu ZiskejIdPsa
                    int? idPsa = _dataAccess.ZiskejIdPsa(rezervaceKod);

                    if (idPsa.HasValue)
                    {
                        ViewBag.Message = $"ID psa: {idPsa.Value}";
                    }
                    else
                    {
                        ViewBag.Message = "Pes nebyl nalezen.";
                    }
                }
                else
                {
                    ViewBag.Message = "Rezervační kód je prázdný.";
                }

                return View();
            }

            // Pokud podmínky nejsou splněny, přesměruj na přihlášení nebo jinou stránku
            return RedirectToAction("Login", "Osoba");
        }

    }
}
