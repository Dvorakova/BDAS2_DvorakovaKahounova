using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Reflection;

namespace BDAS2_DvorakovaKahounova.Controllers
{
	public class ChovateleController : Controller
	{
		private readonly ChovateleDataAccess _dataAccess;

		public ChovateleController(IConfiguration configuration)
		{
			string connectionString = configuration.GetConnectionString("OracleConnection");
			_dataAccess = new ChovateleDataAccess(connectionString);
		}

		public IActionResult Index()
		{
			int uzivatelID = GetLoggedInUserId();
			if (uzivatelID == 0)
			{
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
			}
			List<Pes> psi = new List<Pes>();

			try
			{
				psi = _dataAccess.GetAllPsiProChovatele();
			}
			catch (Exception)
			{
				TempData["Message"] = "chyba při načítání psů.";
			}

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View(psi);

		}
		public IActionResult PridatPsa()
		{
			int uzivatelID = GetLoggedInUserId();
			if (uzivatelID == 0)
			{
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
			}

			if (User.Identity.IsAuthenticated && User.IsInRole("C"))
			{

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(); // Zobrazí stránku rezervací
			}

			// Pokud podmínky nejsou splněny, přesměruje uživatele na přihlášení
			return RedirectToAction("Login", "Osoba");
		}

		public IActionResult VyriditAdopci(int pesId)
		{
			int uzivatelID = GetLoggedInUserId();
			if (uzivatelID == 0)
			{
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
			}

			if (User.Identity.IsAuthenticated && User.IsInRole("C"))
			{
				PesMajitelModel model = new PesMajitelModel();
				model.Pes = new Pes();
				model.Pes.ID_PSA = pesId;
				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(model); // Zobrazí stránku 
			}

			// Pokud podmínky nejsou splněny, přesměruje uživatele na přihlášení
			return RedirectToAction("Login", "Osoba");
		}

		[HttpPost]
		public IActionResult PridatOdcerveniAkce(int pesId, DateTime datumOdcerveni)
		{
			int uzivatelID = GetLoggedInUserId();
			if (uzivatelID == 0)
			{
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
			}
			try
			{
				// Zavolání metody PridatOdcerveni, která přidá odčervení pro konkrétního psa
				_dataAccess.PridatOdcerveni(pesId, datumOdcerveni);
				TempData["Message"] = "Záznam o odčervení úspěšně přidán";
			}
			catch (Exception)
			{
				TempData["Message"] = "Záznam o odčervení se nepřidal";
			}
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult PridatOckovaniAkce(int pesId, DateTime datumOckovani)
		{
			int uzivatelID = GetLoggedInUserId();
			if (uzivatelID == 0)
			{
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
			}
			try
			{
				// Zavolání metody PridatOdckovani, která přidá očkování pro konkrétního psa
				_dataAccess.PridatOckovani(pesId, datumOckovani);
				TempData["Message"] = "Záznam o očkování úspěšně přidán";
			}
			catch (Exception)
			{
				TempData["Message"] = "Záznam o očkování se nepřidal";
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult ZaznamenatUmrti(int pesId)
		{
			try
			{
				_dataAccess.ZaznamenatUmrti(pesId);
				TempData["Message"] = "Úmrtí bylo úspěšně zaznamenáno.";
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "Chyba při zaznamenávání úmrtí: ";
			}

			return RedirectToAction("Index");
		}


		public IActionResult ShowOwnerProfile(int ownerId)
		{
			return RedirectToAction("Profile", "Osoba", new { userId = ownerId });
		}

		//Fotografie
		public IActionResult UploadImage(int pesId, IFormFile imageFile)
		{
			if (imageFile != null && imageFile.Length > 0)
			{
				using (var stream = new MemoryStream())
				{
					imageFile.CopyTo(stream);
					var fotografie = new Fotografie
					{
						nazev_souboru = imageFile.FileName,
						typ_souboru = imageFile.ContentType,
						pripona_souboru = Path.GetExtension(imageFile.FileName),
						datum_nahrani = DateTime.Now,
						nahrano_id_osoba = GetLoggedInUserId(), // metoda pro získání ID přihlášeného uživatele
						obsah_souboru = stream.ToArray()
					};

					try
					{
						// Zavolání metody s transakcí
						_dataAccess.UpravFotografiiTransakci(fotografie, pesId);
					}
					catch (Exception ex)
					{
						TempData["Message"] = "Došlo k chybě při nahrávání fotografie.";
					}
				}
			}

			return RedirectToAction("Index"); // Po nahrání nebo úpravě fotografie se vrátíme na seznam psů
		}

		private int GetLoggedInUserId()
		{
			var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
			if (userIdClaim != null)
			{
				return int.Parse(userIdClaim.Value);
			}
			return 0;
		}

		//Metody pro Pridat psa:


		[HttpPost]
		public IActionResult PridatPsa(string cisloCipu, string action, int[] vlastnosti, IFormFile fotografie)
		{
			int uzivatelID = GetLoggedInUserId();
			if (uzivatelID == 0)
			{
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
			}
			//vyhledání psa podle čísla čipu
			if (action == "search")
			{
				ViewBag.CisloCipu = cisloCipu;
				var idPsa = _dataAccess.GetPesIdByCisloCipu(cisloCipu);
				PesMajitelModel viewModel = new PesMajitelModel();
				if (idPsa.HasValue)
				{
					// Pokud je pes nalezen
					var pes = _dataAccess.ZobrazInfoOPsovi(idPsa.Value) ?? new Pes();
					var majitel = pes.ID_MAJITEL.HasValue
						? _dataAccess.GetUserProfileById(pes.ID_MAJITEL)
						: new Osoba();

					viewModel.Pes = pes;
					viewModel.Majitel = majitel;

					ViewData["Duvody"] = _dataAccess.GetDuvodyPobytu(); // Získání důvodů pobytu
				}
				else
				{
					// Pokud pes neexistuje, zobrazíme formulář pro přidání nového psa
					ViewBag.Message = "Pes nenalezen. Ujistěte se, že máte správně zadané číslo čipu. Pokud ano, zadejte údaje pro nového psa.";

					// Načítáme hodnoty pro comboboxy
					ViewData["Barvy"] = _dataAccess.GetBarvy();// Získání všech barev
					ViewData["Plemena"] = _dataAccess.GetPlemene();// Získání všech plemen
					ViewData["Duvody"] = _dataAccess.GetDuvodyPobytu(); // Získání důvodů pobytu
					ViewData["Vlastnosti"] = _dataAccess.GetVlastnosti(); // Získání všech vlastností
				}

				ViewBag.ShowForm = !idPsa.HasValue;

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(viewModel);
			}
			else if (action == "nochip")
			{
				// Logika pro "Pes bez čipu"
				ViewData["Barvy"] = _dataAccess.GetBarvy();
				ViewData["Plemena"] = _dataAccess.GetPlemene();
				ViewData["Duvody"] = _dataAccess.GetDuvodyPobytu();
				ViewData["Vlastnosti"] = _dataAccess.GetVlastnosti(); // Získání všech vlastností
				ViewBag.ShowForm = true;
				ViewBag.Message = null;

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(new PesMajitelModel());
			}
			//zadání nového psa do databáze
			else if (action == "createDog")
			{
				// Načtení hodnot z formuláře
				var jmeno = Request.Form["jmeno"];
				var cisloNovehoCipu = Request.Form["cisloCipu"];
				var datumNarozeni = string.IsNullOrEmpty(Request.Form["datumNarozeni"])
									? (DateTime?)null
									: DateTime.Parse(Request.Form["datumNarozeni"]);
				var barvaId = int.Parse(Request.Form["barva"]);
				var plemenoId = int.Parse(Request.Form["plemeno"]);
				var pohlaviId = int.Parse(Request.Form["pohlavi"]);
				var duvodPobytuId = int.Parse(Request.Form["duvodPobytu"]);
				var zacatekPobytu = DateTime.Parse(Request.Form["zacatekPobytu"]);
				var vaha = int.Parse(Request.Form["vaha"]);

				//////////////////////
				int? fotografieId = null;
				try
				{
					// Pokud je soubor fotografie přítomen, ulož ho

					if (fotografie != null && fotografie.Length > 0)
					{
						// Převeď soubor na byte[]
						using (var memoryStream = new MemoryStream())
						{
							fotografie.CopyTo(memoryStream);
							byte[] obsahFotografie = memoryStream.ToArray();

							// Vytvoř objekt Fotografie a nastav parametry
							var fotografieObj = new Fotografie
							{
								nazev_souboru = fotografie.FileName,
								typ_souboru = fotografie.ContentType,
								pripona_souboru = Path.GetExtension(fotografie.FileName),
								datum_nahrani = DateTime.Now,
								nahrano_id_osoba = GetLoggedInUserId(), // získání id přihlášeného uživatele
								obsah_souboru = obsahFotografie
							};

							// Ulož fotografii a získej ID
							fotografieId = _dataAccess.SaveFotografie(fotografieObj);
						}
					}
				}
				catch (Exception ex)
				{

					var originallllRole = HttpContext.Session.GetString("OriginalRole");
					ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallllRole == "A" || User.IsInRole("A")));

					// Ošetření chyby
					ViewBag.ErrorMessage = "Chyba při načítání fotografie.";
					return View();
				}
				////////////////////////

				try
				{
					_dataAccess.VlozNovehoPsa(jmeno, cisloNovehoCipu, datumNarozeni, plemenoId, barvaId, pohlaviId, fotografieId, vaha,
						zacatekPobytu, duvodPobytuId, vlastnosti);
				}
				catch (Exception ex) { ViewBag.ErrorMessage = "Chyba při vkládání psa do databáze."; }

				// Přesměrování nebo zobrazení potvrzení
				TempData["Message"] = "Pes byl úspěšně přidán!";
				ViewBag.Message = "Pes přidán";

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View();
			}
			else if (action == "addStay")
			{
				var pesId = int.Parse(Request.Form["pesId"]);
				var zacatekPobytu = DateTime.Parse(Request.Form["zacatekPobytu"]);
				var idDuvod = int.Parse(Request.Form["idDuvod"]);
				var vaha = double.Parse(Request.Form["vaha"]);

				try
				{
					//přidání nového pobytu psovi, který už v útulku kdysi byl
					_dataAccess.PridatPobytPsovi(pesId, zacatekPobytu, idDuvod, vaha);

					return RedirectToAction("Index");
				}
				catch (Exception ex)
				{
					// Ošetření chyby
					ViewBag.ErrorMessage = ex.Message;

					var originallRole = HttpContext.Session.GetString("OriginalRole");
					ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

					return View();
				}
			}

			var originalRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalRole == "A" || User.IsInRole("A")));

			return View();

		}


		[HttpPost]
		public IActionResult VyzvednoutMajitelem(int pesId)
		{
			_dataAccess.UkonciPobyt(pesId);
			return RedirectToAction("Index");
		}

		//metoda pro adoptování psa (bez rezervace)
		[HttpPost]
		public IActionResult Adoptovat(int pesId)
		{
			return RedirectToAction("VyriditAdopci", new { pesId = pesId });
		}

		[HttpPost]
		public IActionResult VyriditAdopci(string email, string action, int idPes, int majitelIdOsoba)
		{
			int uzivatelID = GetLoggedInUserId();
			if (uzivatelID == 0)
			{
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
			}

			if (User.Identity.IsAuthenticated && User.IsInRole("C"))
			{
				if (action == "search")
				{
					// Zavoláme metodu v data access vrstvě pro ověření emailu
					var osoba = _dataAccess.OveritEmail(email);

					if (osoba != null)
					{
						PesMajitelModel majitel = new PesMajitelModel();
						majitel.Pes = new Pes();
						majitel.Pes.ID_PSA = idPes;
						majitel.Majitel = osoba;

						var originalllRole = HttpContext.Session.GetString("OriginalRole");
						ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalllRole == "A" || User.IsInRole("A")));
						// Pokud email existuje, přesměrujeme uživatele na stránku pro potvrzení adopce
						return View(majitel);
					}
					else
					{
						// Pokud email není nalezen, zobrazíme chybovou zprávu
						ViewBag.ErrorMessage = "Osoba s tímto emailem není zaregistrována.";

						PesMajitelModel majitel = new PesMajitelModel();
						majitel.Pes = new Pes();
						majitel.Pes.ID_PSA = idPes;

						var originalllRole = HttpContext.Session.GetString("OriginalRole");
						ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalllRole == "A" || User.IsInRole("A")));

						return View(majitel);
					}

				}
				else if (action == "adoptovatPsa")
				{
					try
					{
						_dataAccess.AdopcePsa(majitelIdOsoba, idPes);
					}
					catch (Exception)
					{
						// Ošetření chyby
						ViewBag.ErrorMessage = "Psa se nepodařilo adoptovat.";
						return View();
					}


					return RedirectToAction("Index");
				}
				// Pokud podmínky nejsou splněny, zobrazíme formulář pro vyřízení adopce
				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(); // Zobrazí stránku s formulářem

			}
			// Pokud podmínky nejsou splněny, přesměrujeme uživatele na přihlášení
			return RedirectToAction("Login", "Osoba");
		}

	}
}
