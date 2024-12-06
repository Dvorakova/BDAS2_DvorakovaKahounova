using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Security.Claims;

namespace BDAS2_DvorakovaKahounova.Controllers
{
    public class PesController : Controller
    {
        private readonly PesDataAcess _dataAccess;

        // Konstruktor controlleru, který přijímá instanci PesDataAccess
        public PesController(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("OracleConnection");
            _dataAccess = new PesDataAcess(connectionString);
        }

        public IActionResult PsiKAdopci()
        {
			List<Pes> psi = new List<Pes>();

			try
            {
				psi = _dataAccess.GetAllPsi();
			}
            catch (Exception)
            {
                ViewBag.Message = "Došlo při načítání psů k adopci.";
            }
            

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			// Předání seznamu psů do pohledu (view)
			return View(psi);
        }

        public IActionResult MujPes()
        {
            int osobaId = GetLoggedInUserId();
            if (osobaId == 0)
			{
				return RedirectToAction("Login");
            }

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));
			List<Pes> psi = new List<Pes>();
			try
            {
				psi = _dataAccess.GetPsiProOsobu(osobaId);
			}
            catch (Exception)
            {
                ViewBag.Error = "Chyba při načítání psů.";
            }
			
            return View(psi);
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

        //pokud pes nemá v databázi vlastní obrázek, je mu přiřazen defaultní obrázek (např. na stránce MujPes)
        public IActionResult DefaultImage()
        {
            var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/ObrazekDefault.png");
            var imageFileStream = System.IO.File.OpenRead(defaultImagePath);
            return File(imageFileStream, "image/png");
        }
        // načtení obrázku
        public IActionResult GetImage(int id)
        {
            var fotografie = _dataAccess.GetFotografieById(id);
            if (fotografie == null || fotografie.obsah_souboru == null)
            {
                return NotFound();
            }
            return File(fotografie.obsah_souboru, fotografie.typ_souboru);
        }


        [HttpPost]
        public async Task<IActionResult> Rezervovat(int idPsa)
        {
            // Získání ID přihlášeného uživatele
            int rezervatorId = GetLoggedInUserId();
            if (rezervatorId == 0)
            {
				return RedirectToAction("Login", "Osoba"); // Pokud není uživatel přihlášen
            }
            
			try
			{
				//Zavolání metody pro vytvoření rezervace
				_dataAccess.VytvorRezervaciSTransakci(idPsa, rezervatorId);

			}
			catch (Exception ex)
			{
				TempData["Message"] = "Došlo k chybě při vytváření rezervace.";

				return RedirectToAction("PsiKAdopci", "Pes");
			}

			var identity = (ClaimsIdentity)User.Identity;

            string role = identity.FindFirst(ClaimTypes.Role)?.Value;
            
            if (role.Equals("M") || role.Equals("U"))
            {
				// Odstraníme staré hodnoty z claims
				identity.RemoveClaim(identity.FindFirst(ClaimTypes.Role));

                if (role.Equals("U"))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, "R"));
                }
                else if (role.Equals("M"))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, "P"));
                }

                // Vytvoříme nový ClaimsPrincipal s aktuálními claims
                var principal = new ClaimsPrincipal(identity);

                // Uložíme nový ClaimsPrincipal do cookies pro autentizaci
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

			return RedirectToAction("Index", "Rezervace");
        }
    }
}


