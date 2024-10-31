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
            novaOsoba.TYP_OSOBY = "U";
            //Console.WriteLine($"Jméno: {novaOsoba.JMENO}, Příjmení: {novaOsoba.PRIJMENI}, Typ osoby: {novaOsoba.TYP_OSOBY} Telefon: {novaOsoba.TELEFON}, Email: {novaOsoba.EMAIL}, Heslo: {novaOsoba.HESLO}");

            if (ModelState.IsValid)
            {
                Console.WriteLine("kontrola mailu v controolleer");
                if (_dataAccess.EmailExists(novaOsoba.EMAIL))
                {
                    ModelState.AddModelError("EMAIL", "Tento email již byl použit.");
                    return View(novaOsoba);
                }

                if (_dataAccess.RegisterUser(novaOsoba))
                {
                    //když se registrace podaří, zobrazí se uživateli stránka s přihlášením
                    return RedirectToAction("Login", new { email = novaOsoba.EMAIL });
                    return RedirectToAction("Login");
                }
                Console.WriteLine("Registrace se nezdařila.");
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
        public IActionResult Login(string email, string heslo)
        {
            var osoba = _dataAccess.LoginUser(email, heslo);
            if (osoba != null)
            {
                // pokud se přihlášení povede, uživateli se zobrazí stránka (zatím home)
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("email", "Nesprávné přihlašovací údaje.");
            ViewBag.Email = email;
            ViewBag.IsLoginAttempted = true; // Informace o pokusu o přihlášení
            return View();
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
