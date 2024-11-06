using BDAS2_DvorakovaKahounova.DataAcessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDAS2_DvorakovaKahounova.Controllers
{
    public class RezervaceController : Controller
    {
        private readonly OsobaDataAccess _dataAccess;

        public RezervaceController(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("OracleConnection");
            _dataAccess = new OsobaDataAccess(connectionString);
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
            return View();
        }
    }
}
