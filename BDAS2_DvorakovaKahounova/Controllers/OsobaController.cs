using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Mvc;
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
            //novaOsoba.TYP_OSOBY = "O";
            Console.WriteLine($"Jméno: {novaOsoba.JMENO}, Příjmení: {novaOsoba.PRIJMENI}, Telefon: {novaOsoba.TELEFON}, Email: {novaOsoba.EMAIL}, Heslo: {novaOsoba.HESLO}");

            if (ModelState.IsValid)
            {
                Console.WriteLine("kontrola mailu v controolleer");
                if (_dataAccess.EmailExists(novaOsoba.EMAIL))
                {
                    ModelState.AddModelError("EMAIL", "Tento email již byl použit.");
                    return View(novaOsoba);
                }
                Console.WriteLine("po kontrole mailu v controller.");

                ////přidání typu osoby
                //novaOsoba.TYP_OSOBY = "U";
                //Console.WriteLine("Jsme u nastavení typu osoby.");

                if (_dataAccess.RegisterUser(novaOsoba))
                {
                    Console.WriteLine("Registrace úspěšná.");
                    //když se registrace podaří, zobrazí se uživateli stránka s přihlášením
                    return RedirectToAction("Login");
                }
                Console.WriteLine("Registrace se nezdařila.");
                ModelState.AddModelError("", "Registrace se nezdařila.");
            }
            else
            {
                // Výpis chyb
                foreach (var entry in ModelState)
                {
                    foreach (var error in entry.Value.Errors)
                    {
                        Console.WriteLine($"Chyba: {error.ErrorMessage}");
                    }
                }
            }
            return View(novaOsoba);
        }

        // Zobrazení přihlašovacího formuláře
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Zpracování přihlášení
        [HttpPost]
        public IActionResult Login(string email, string heslo)
        {
            var osoba = _dataAccess.LoginUser(email, heslo);
            if (osoba != null)
            {
                // Tady by obvykle následovalo nastavení session nebo cookies
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", "Nesprávné přihlašovací údaje.");
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
