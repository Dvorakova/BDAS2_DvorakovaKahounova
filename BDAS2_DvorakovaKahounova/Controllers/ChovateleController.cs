using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;

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
            List<Pes> psi = _dataAccess.GetAllPsiProChovatele();
            return View(psi);
            
        }
		public IActionResult PridatPsa()
		{
            if (User.Identity.IsAuthenticated && User.IsInRole("C"))
            {
                return View(); // Zobrazí stránku rezervací
            }

            // Pokud podmínky nejsou splněny, přesměruj na přihlášení nebo jinou stránku
            return RedirectToAction("Login", "Osoba");
        }

		[HttpPost]
		public IActionResult PridatOdcerveniAkce(int pesId, DateTime datumOdcerveni)
		{
			// Zavolání metody PridatOdcerveni, která přidá odčervení pro konkrétního psa
			_dataAccess.PridatOdcerveni(pesId, datumOdcerveni);

			// Po dokončení akce přesměrujeme na nějakou stránku nebo zobrazíme zprávu
			return RedirectToAction("Index"); // nebo nějaký jiný pohled
		}

		[HttpPost]
		public IActionResult PridatOckovaniAkce(int pesId, DateTime datumOckovani)
		{
			// Zavolání metody PridatOdcerveni, která přidá odčervení pro konkrétního psa
			_dataAccess.PridatOckovani(pesId, datumOckovani);

			// Po dokončení akce přesměrujeme na nějakou stránku nebo zobrazíme zprávu
			return RedirectToAction("Index"); // nebo nějaký jiný pohled
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
        public IActionResult PridatPsa(string cisloCipu, string action)
        {
            if(action == "search")
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

                return View(new PesMajitelModel());
            }
            return View();

        }

        //zkouška odeslání dat z formuláře pro přidání nového psa:
        //[HttpPost]
        //public IActionResult PridatPsa(Pes model, List<int> vlastnosti)
        //{
        //    // Debug: Zobrazení ID zaškrtnutých vlastností
        //    Console.WriteLine("Zaškrtnuté vlastnosti: " + string.Join(", ", vlastnosti));

        //    // Logika pro uložení psa a jeho vlastností
        //    //int idPsa = _dataAccess.PridatPsa(model); // Předpokládáme, že tato metoda vrací ID psa

        //    // Uložení zaškrtnutých vlastností k danému psovi
        //    //foreach (var idVlastnost in vlastnosti)
        //    //{
        //    //    //_dataAccess.PridatVlastnostKPsovi(idPsa, idVlastnost);
        //    //}

        //    // Přesměrování nebo potvrzení akce
        //    //return RedirectToAction("DetailPsa", new { id = idPsa });
        //    return View();
        //}



    }
}
