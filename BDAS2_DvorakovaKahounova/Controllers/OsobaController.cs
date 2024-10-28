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
            if (ModelState.IsValid)
            {
                if (_dataAccess.RegisterUser(novaOsoba))
                {
                    return RedirectToAction("Login");
                }
                ModelState.AddModelError("", "Registrace se nezdařila.");
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
