﻿using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Org.BouncyCastle.Crypto;
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
		}

		public IActionResult Katalog()
		{
			List<KatalogItem> katalog = _dataAccess.GetKatalogViewAll();
			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));			
			return View(katalog);
		}

		public IActionResult Emulace()
		{

			var users = _dataAccess.GetAllUsers();

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

			return View(users);
		}
		public IActionResult Vyhledavani()
		{
			var tableNames = _dataAccess.VypisTabulkyASloupce()
				.Select(t => t.TableName)
				.Distinct()
				.ToList();

			try
			{
				// Načteme seznam zaměstnanců
				var zamestnanci = _dataAccess.GetZamestnanci();
				// Předáme seznam zaměstnanců do ViewBag pro použití v ComboBoxu
				ViewBag.Zamestnanci = zamestnanci;

			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = $"Chyba při načítání zaměstnanců: {ex.Message}";
				return RedirectToAction("Error");
			}

			ViewBag.TableNames = tableNames;
			ViewBag.HasSearched = false;

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

			return View();
		}

		[HttpPost]
		public IActionResult Search(string selectedTable, string searchQuery)
		{
			ViewBag.HasSearched = true;
			if (string.IsNullOrEmpty(selectedTable) || string.IsNullOrEmpty(searchQuery))
			{
				TempData["ErrorMessage"] = "Musíte vybrat tabulku a zadat hledaný text.";
			}
			else
			{
				try
				{
					// Prohledání tabulky
					var results = _dataAccess.SearchTableProcedure(selectedTable, searchQuery);
					// Kontrola, zda jsou výsledky prázdné
					if (results == null || !results.Any())
					{
						ViewBag.NoResultsMessage = "Žádné výsledky nebyly nalezeny pro zadaný text.";
					}
					// Předej výsledky do pohledu
					ViewBag.SearchResults = results;
					ViewBag.SelectedTable = selectedTable;
				}
				catch (Exception ex)
				{

					TempData["ErrorMessage"] = "Došlo k chybě při vyhledávání: " + ex.Message;
				}
			}
			try
			{
				// Načteme seznam zaměstnanců
				var zamestnanci = _dataAccess.GetZamestnanci();
				// Předáme seznam zaměstnanců do ViewBag pro použití v ComboBoxu
				ViewBag.Zamestnanci = zamestnanci;

			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = $"Chyba při načítání zaměstnanců: {ex.Message}";
				return RedirectToAction("Error");
			}

			var tableNames = _dataAccess.VypisTabulkyASloupce()
				.Select(t => t.TableName)
				.Distinct()
				.ToList();

			ViewBag.TableNames = tableNames;

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

			return View("Vyhledavani");
		}


		[HttpPost]
		public IActionResult SearchHierarchy(string idOsoba)
		{
			ViewBag.HasSearchedHierarchy = true;

			if (string.IsNullOrEmpty(idOsoba) || !int.TryParse(idOsoba, out int parsedId))
			{
				TempData["ErrorMessage"] = "Musíte zadat platné ID osoby.";
			}
			else
			{
				try
				{
					// Získání hierarchie
					var hierarchyResults = _dataAccess.GetHierarchy(parsedId);

					if (hierarchyResults == null || !hierarchyResults.Any())
					{
						ViewBag.NoResultsMessage = "Žádní podřízení nebyli nalezeni pro zadané ID.";
					}
					else
					{
						ViewBag.HierarchyResults = hierarchyResults;
						ViewBag.SearchedId = parsedId;
					}
				}
				catch (Exception ex)
				{
					TempData["ErrorMessage"] = "Došlo k chybě při zobrazení hierarchie: " + ex.Message;
				}
			}

			try
			{
				// Načteme seznam zaměstnanců
				var zamestnanci = _dataAccess.GetZamestnanci();
				// Předáme seznam zaměstnanců do ViewBag pro použití v ComboBoxu
				ViewBag.Zamestnanci = zamestnanci;

			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = $"Chyba při načítání zaměstnanců: {ex.Message}";
				return RedirectToAction("Error");
			}

			var tableNames = _dataAccess.VypisTabulkyASloupce()
				.Select(t => t.TableName)
				.Distinct()
				.ToList();

			ViewBag.TableNames = tableNames;

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

			return View("Vyhledavani");
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

			//comboboxy:
			ViewBag.Zamestnanci = _dataAccess.GetZamestnanci();
			ViewBag.Psi = _dataAccess.GetPsi();
			ViewBag.Majitele = _dataAccess.GetMajitele();
			ViewBag.Rezervatori = _dataAccess.GetRezervatori();
			ViewBag.Osoby = _dataAccess.GetOsoby();
			ViewBag.Duvody = _dataAccess.GetDuvody();
			ViewBag.Barvy = _dataAccess.GetBarvy();
			ViewBag.Plemena = _dataAccess.GetPlemena();
			ViewBag.Predpisy = _dataAccess.GetPredpisy();
			ViewBag.Pohlavi = _dataAccess.GetPohlavi();
			ViewBag.Vlastnosti = _dataAccess.GetVlastnosti();
			ViewBag.Fotografie = _dataAccess.GetFotografie();
			ViewBag.Pobyty = _dataAccess.GetPobyty();
			ViewBag.Zaznamy = _dataAccess.GetZaznamy();

			ViewBag.IsEditMode = TempData["IsEditMode"];
			ViewBag.NazevTabulky = TempData["NazevTabulky"];
			ViewBag.EditValues = TempData["EditValues"];
			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));


			return View(model);
		}

		[HttpPost]
		public IActionResult AddRecord(string tableName, Dictionary<string, string> values)
		{
			try
			{
				//přidáno - potřeba upravit a pak přidat do editace stejný foreach
				foreach (var key in values.Keys.ToList())
				{
					if (key.StartsWith("ID_NADRIZENEHO") && int.TryParse(values[key], out var result))
					{
						values[key] = result.ToString();
					}
					else if (key.StartsWith("MAJITEL_ID_OSOBA") && int.TryParse(values[key], out var result1))
					{
						values[key] = result1.ToString();
					}
					else if (key.StartsWith("REZERVATOR_ID_OSOBA") && int.TryParse(values[key], out var result2))
					{
						values[key] = result2.ToString();
					}
					else if (key.StartsWith("ID_PSA") && int.TryParse(values[key], out var result3))
					{
						values[key] = result3.ToString();
					}
					else if ((key.StartsWith("ID_OSOBA")|| key.StartsWith("NAHRANO_ID_OSOBA")) && int.TryParse(values[key], out var result4))
					{
						values[key] = result4.ToString();
					}
					else if (key.StartsWith("ID_DUVOD") && int.TryParse(values[key], out var result5))
					{
						values[key] = result5.ToString();
					}
					else if (key.StartsWith("ID_BARVA") && int.TryParse(values[key], out var result6))
					{
						values[key] = result6.ToString();
					}
					else if (key.StartsWith("ID_PLEMENO") && int.TryParse(values[key], out var result7))
					{
						values[key] = result7.ToString();
					}
					else if (key.StartsWith("ID_PREDPIS") && int.TryParse(values[key], out var result8))
					{
						values[key] = result8.ToString();
					}
					else if (key.StartsWith("ID_POHLAVI") && int.TryParse(values[key], out var result9))
					{
						values[key] = result9.ToString();
					}
					else if (key.StartsWith("ID_VLASTNOST") && int.TryParse(values[key], out var result10))
					{
						values[key] = result10.ToString();
					}
					else if (key.StartsWith("ID_FOTOGRAFIE") && int.TryParse(values[key], out var result11))
					{
						values[key] = result11.ToString();
					}
					else if (key.StartsWith("ID_POBYT") && int.TryParse(values[key], out var result12))
					{
						values[key] = result12.ToString();
					}
					else if (key.StartsWith("ID_ZAZNAM_POBYT") && int.TryParse(values[key], out var result13))
					{
						values[key] = result13.ToString();
					}
				}
				//


				_dataAccess.InsertRecord(tableName, values);
			}
			catch (Exception)
			{
				TempData["ErrorMessage"] = "Záznam se nepovedlo přidat";
			}
			// Zde zavoláme metodu pro přidání záznamu


			// Po úspěšném přidání záznamu přesměrujeme zpět na Index stránku
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult DeleteRecord(string tableName, Dictionary<string, string> values)
		{
			foreach (var pair in values)
			{
				Console.WriteLine($"{pair.Key} = {pair.Value}");
			}
			try
			{
				// Volání DataAccess metody pro smazání
				_dataAccess.DeleteRecord(tableName, values);
			}
			catch (Exception)
			{
				TempData["ErrorMessage"] = "Záznam se nepovedlo smazat";
			}


			// Přesměrování zpět na Index stránku
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult UpdateRecord(string tableName, Dictionary<string, string> values, Dictionary<string, string> oldValues)
		{
			//try
			//{
			// Zavolejte DataAccess metodu pro aktualizaci
			_dataAccess.UpdateRecord(tableName, values, oldValues);
			//}
			//catch (Exception)
			//{
			//	TempData["ErrorMessage"] = "Záznam se nepovedlo přidat";
			//}


			// Přesměrování zpět na Index stránku
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult EditRecord(string tableName, Dictionary<string, string> values)
		{
			TempData["IsEditMode"] = true;
			TempData["NazevTabulky"] = tableName.ToUpper();
			TempData["EditValues"] = values;

			return RedirectToAction("Index");
		}

		//[HttpPost]
		//public IActionResult Search(string tableName, Dictionary<string, string> values)
		//{
		//	TempData["IsSearchMode"] = true;

		//	return RedirectToAction("Index");
		//}
	}
}
