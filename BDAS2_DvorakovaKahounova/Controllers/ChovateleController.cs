using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Mvc;

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
            return View();
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

	}
}
