﻿using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Security.Claims;
using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace BDAS2_DvorakovaKahounova.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminDataAccess _dataAccess;
        public AdminController(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("OracleConnection");
            _dataAccess = new AdminDataAccess(connectionString);
        }

        public IActionResult Statistiky()
        {
            decimal celkovaDavkaKg = _dataAccess.SpoctiKrmneDavky();
            ViewData["CelkovaDavkaKg"] = celkovaDavkaKg;

            int pocetPsu = _dataAccess.SpoctiPocetPsuVUtulku();

            // Předáme počet psů do ViewData, aby se zobrazil na stránce
            ViewData["PocetPsuvUtulku"] = pocetPsu;

            int pocetPsuVKarantene = _dataAccess.SpoctiPocetPsuVKarantene();
            ViewData["PocetPsuVKarantene"] = pocetPsuVKarantene;

            decimal prumernyPobyt = _dataAccess.SpoctiPrumernouDobuPobytu();
            prumernyPobyt = Math.Round(prumernyPobyt, 2);
            ViewData["PrumernyPobyt"] = prumernyPobyt;

            var originallRole = HttpContext.Session.GetString("OriginalRole");
            ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

            return View();
        }

        public IActionResult Logovani()
        {
            var originallRole = HttpContext.Session.GetString("OriginalRole");
            ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));
            List<Log> logs = _dataAccess.GetLogs();
            return View(logs);
            //return View();
        }
        public IActionResult Emulace()
        {

            var users = _dataAccess.GetAllUsers();

            var originalRole = HttpContext.Session.GetString("OriginalRole");
            ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

            return View(users);
        }

        [HttpPost]
        public IActionResult Emulovat(int userId)
        {
            //uložení původní role, pokud emulujeme poprvé
            if (HttpContext.Session.GetString("OriginalRole") == null)
            {
                var originalRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                HttpContext.Session.SetString("OriginalRole", originalRole ?? "A"); // Defaultní původní role: Admin

                var adminId = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                HttpContext.Session.SetString("OriginalUserId", adminId ?? "0"); // původní id
                Console.WriteLine("Emulace poprvé. Id: " + adminId);
            }

            // Získání dat uživatele z databáze
            var user = _dataAccess.GetUserById(userId);
            if (user == null) return RedirectToAction("Emulace"); // Uživatel nenalezen

            // Vytvoření nových claims pro emulovaného uživatele
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.JMENO),
                    new Claim(ClaimTypes.Email, user.EMAIL),
                    new Claim(ClaimTypes.Role, user.TYP_OSOBY),
                    new Claim("UserId", user.ID_OSOBA.ToString())
                };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult ResetRole()
        {
            // Načtení původního ID administrátora ze session
            var originalUserId = HttpContext.Session.GetString("OriginalUserId");
            if (string.IsNullOrEmpty(originalUserId)) return RedirectToAction("Index", "Home");

            // Načtení dat administrátora z databáze
            var admin = _dataAccess.GetUserById(int.Parse(originalUserId));
            if (admin == null) return RedirectToAction("Index", "Home");

            // Obnovení identity administrátora
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, admin.JMENO),
                    new Claim(ClaimTypes.Email, admin.EMAIL),
                    new Claim(ClaimTypes.Role, admin.TYP_OSOBY),
                    new Claim("UserId", admin.ID_OSOBA.ToString())
                };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

            // Vyčištění session
            HttpContext.Session.Remove("OriginalUserId");
            HttpContext.Session.Remove("OriginalRole");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            var model = new DatabaseViewModel
            {
                TablesAndColumns = _dataAccess.VypisTabulkyASloupce()
            };

            // Pro každou tabulku načíst její obsah
            var tableNames = model.TablesAndColumns.Select(t => t.TableName).Distinct();
            foreach (var tableName in tableNames)
            {
                model.TableContents[tableName] = _dataAccess.TableContent(tableName);
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult AddRecord(string tableName, Dictionary<string, string> values)
        {
            // Zde zavoláme metodu pro přidání záznamu
            _dataAccess.InsertRecord(tableName, values);

            // Po úspěšném přidání záznamu přesměrujeme zpět na Index stránku
            return RedirectToAction("Index");
        }
        //[HttpPost]
        //public IActionResult DeleteRecord(string tableName, Dictionary<string, string> keys)
        //{
        //    _dataAccess.DeleteRecord(tableName, keys);
        //    return RedirectToAction(nameof(Index));
        //}



    }
}
