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

        // Konstruktor controlleru, který přijímá instanci PesDataAccess
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

				var originalllRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalllRole == "A" || User.IsInRole("A")));

				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            List<Pes> psi = _dataAccess.GetAllPsiProChovatele();

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View(psi);
            
        }
		public IActionResult PridatPsa()
		{
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            if (User.Identity.IsAuthenticated && User.IsInRole("C"))
            {

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View(); // Zobrazí stránku rezervací
            }


			var originalllRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalllRole == "A" || User.IsInRole("A")));

			// Pokud podmínky nejsou splněny, přesměruj na přihlášení nebo jinou stránku
			return RedirectToAction("Login", "Osoba");
        }

		[HttpPost]
		public IActionResult PridatOdcerveniAkce(int pesId, DateTime datumOdcerveni)
		{
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            // Zavolání metody PridatOdcerveni, která přidá odčervení pro konkrétního psa
            _dataAccess.PridatOdcerveni(pesId, datumOdcerveni);


			var originalllRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalllRole == "A" || User.IsInRole("A")));

			// Po dokončení akce přesměrujeme na nějakou stránku nebo zobrazíme zprávu
			return RedirectToAction("Index"); // nebo nějaký jiný pohled
		}

		[HttpPost]
		public IActionResult PridatOckovaniAkce(int pesId, DateTime datumOckovani)
		{
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {

				var originalllRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originalllRole == "A" || User.IsInRole("A")));

				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            // Zavolání metody PridatOdcerveni, která přidá odčervení pro konkrétního psa
            _dataAccess.PridatOckovani(pesId, datumOckovani);


			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			// Po dokončení akce přesměrujeme na nějakou stránku nebo zobrazíme zprávu
			return RedirectToAction("Index"); // nebo nějaký jiný pohled
		}



		public IActionResult ShowOwnerProfile(int ownerId)
		{

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

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

					// Získání existující fotografie pro psa
					var existujiciFotografie = _dataAccess.GetFotografieByPesId(pesId);

					if (existujiciFotografie == null)
					{
						// Pokud pes ještě nemá fotografii, přidáme novou
						int fotografieId = _dataAccess.SaveFotografie(fotografie);
						_dataAccess.UpdatePesFotografie(pesId, fotografieId);
					}
					else
					{
						// Pokud pes již má fotografii, aktualizujeme stávající
						fotografie.id_fotografie = existujiciFotografie.id_fotografie;

						// Aktualizace existující fotografie v databázi
						_dataAccess.UpdateFotografie(fotografie);
					}
				}
			}

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

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

        //Metody rpo Pridat psa:


        [HttpPost]
        public IActionResult PridatPsa(string cisloCipu, string action, int[] vlastnosti, IFormFile fotografie)
        {
            int uzivatelID = GetLoggedInUserId();
            if (uzivatelID == 0)
            {

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }

            if (action == "search")
            {
                ViewBag.CisloCipu = cisloCipu;
                Console.WriteLine("cislo cipu v controlleru" + cisloCipu);
                var idPsa = _dataAccess.GetPesIdByCisloCipu(cisloCipu);
                //Pes pes = new Pes();
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

				//return View(pes);
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
            else if (action == "createDog")
            {
                // Načtení hodnot z formuláře
                var jmeno = Request.Form["jmeno"];
                var cisloNovehoCipu = Request.Form["cisloCipu"];
                Console.WriteLine("Číslo čipu: " + cisloNovehoCipu);
                var datumNarozeni = string.IsNullOrEmpty(Request.Form["datumNarozeni"])
                                    ? (DateTime?)null
                                    : DateTime.Parse(Request.Form["datumNarozeni"]);
                var barvaId = int.Parse(Request.Form["barva"]);
                var plemenoId = int.Parse(Request.Form["plemeno"]);
                var pohlaviId = int.Parse(Request.Form["pohlavi"]);
                var duvodPobytuId = int.Parse(Request.Form["duvodPobytu"]);
                var zacatekPobytu = DateTime.Parse(Request.Form["zacatekPobytu"]);
                //var vlastnostiIds = Request.Form["vlastnosti[]"].ToString().Split(',').Select(int.Parse).ToList();
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
                                nahrano_id_osoba = GetLoggedInUserId(),  // Pokud máš ID majitele, můžeš ho použít
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
					ViewBag.ErrorMessage = ex.Message;
                    return View();
                }
                ////////////////////////

                // 1. Vložení psa do tabulky Psi
                var pesId = _dataAccess.VlozPsa(jmeno, cisloNovehoCipu, datumNarozeni, plemenoId, barvaId, pohlaviId, fotografieId, vaha);

                // 2. Vložení pobytu psa do tabulky Pobyty
                var pobytId = _dataAccess.VlozPobyt(pesId, zacatekPobytu, duvodPobytuId, vaha);

                // 3. Vložení záznamu o pobytu
                _dataAccess.VlozZaznamOPobytu(pobytId);

                // 4. Vložení vlastností do tabulky psi_vlastnosti

                _dataAccess.PridatVlastnostiDoDatabaze(pesId, vlastnosti);

                // Přesměrování nebo zobrazení potvrzení
                TempData["Message"] = "Pes byl úspěšně přidán!";
                Console.WriteLine("Pes přidán");
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
                    // 1. Pokud je důvod pobytu "odložen majitelem", aktualizujeme psa
                    if (idDuvod == 1)
                    {
                        // Aktualizace psa - nastavení majitele na null
                        var osobaId = _dataAccess.GetMajitelIdByPesId(pesId);
                        _dataAccess.AktualizujMajitelePsaNaNull(pesId); // Tuto metodu musíme přidat do data access                        
                        if (osobaId != null)
                        {
                            _dataAccess.ZpracujMajiteleBezPsa(osobaId.Value);
                        }
                    }

                    // 2. Aktualizace váhy psa
                    _dataAccess.AktualizujVahuPsa(pesId, vaha); // Tato metoda bude aktualizovat váhu v tabulce psi

                    // 3. Vložení nového pobytu
                    int pobytId = _dataAccess.VlozPobyt(pesId, zacatekPobytu, idDuvod, vaha);

                    // 4. Vložení záznamu o pobytu
                    _dataAccess.VlozZaznamOPobytu(pobytId);


					var originallRole = HttpContext.Session.GetString("OriginalRole");
					ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

					//return RedirectToAction("PesDetails", new { id = pesId }); // Přesměrování na detail psa
					return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Ošetření chyby
                    Console.WriteLine("Chyba při přdání starého psa");
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
        public IActionResult PridatPobyt(int pesId, DateTime zacatekPobytu, int idDuvod, double vaha)
        {
            try
            {
                // 1. Pokud je důvod pobytu "odložen majitelem", aktualizujeme psa
                if (idDuvod == 1)
                {
                    // Aktualizace psa - nastavení majitele na null
                    _dataAccess.AktualizujMajitelePsaNaNull(pesId); // Tuto metodu musíme přidat do data access
                }

                // 2. Aktualizace váhy psa
                _dataAccess.AktualizujVahuPsa(pesId, vaha); // Tato metoda bude aktualizovat váhu v tabulce psi

                // 3. Vložení nového pobytu
                int pobytId = _dataAccess.VlozPobyt(pesId, zacatekPobytu, idDuvod, vaha);

                // 4. Vložení záznamu o pobytu
                _dataAccess.VlozZaznamOPobytu(pobytId);


				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				//return RedirectToAction("PesDetails", new { id = pesId }); // Přesměrování na detail psa
				return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Ošetření chyby
                Console.WriteLine("Chyba při přdání starého psa");
                ViewBag.ErrorMessage = ex.Message;

				var originallRole = HttpContext.Session.GetString("OriginalRole");
				ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

				return View();
            }
        }


    }
}
