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

        //index přesunut do chovatele a využit pro chovatele
        //public IActionResult Index()
        //{
        //    List<Pes> psi = _dataAccess.GetAllPsi();

        //    // Předání seznamu psů do pohledu (view)
        //    return View(psi);
        //}

        public IActionResult PsiKAdopci()
        {
            List<Pes> psi = _dataAccess.GetAllPsi();

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
                List<Pes> psi = _dataAccess.GetPsiProOsobu(osobaId);
            return View(psi);
        }

        //pridano KK
        private int GetLoggedInUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim != null)
            {
                return int.Parse(userIdClaim.Value);
            }
            //var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // získá ID uživatele z Claims
            //return int.Parse(userId);
            return 0;
        }

        //pridano ND
        //pokud pes nemá v databázi vlastní obrázek, je mu přiřazen defaultní obrázek (na stránce MujPes)
        public IActionResult DefaultImage()
        {
            var defaultImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/ObrazekDefault.png");
            var imageFileStream = System.IO.File.OpenRead(defaultImagePath);
            return File(imageFileStream, "image/png");
        }

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

            var identity = (ClaimsIdentity)User.Identity;

            string role = identity.FindFirst(ClaimTypes.Role)?.Value;
            if (role.Equals("M") || role.Equals("U"))
            {

                _dataAccess.ZmenTypOsoby(rezervatorId);

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

                _dataAccess.PridatDoRezervatori(rezervatorId);
            }

            try
            {
                //Zavolání metody pro vytvoření rezervace
               
                _dataAccess.VytvorRezervaci(idPsa, rezervatorId);

            }
            catch (Exception ex)
            {
                // Můžete logovat chybu nebo informovat uživatele
                return StatusCode(500, "Došlo k chybě při vytváření rezervace.");
            }

            // Přesměrování na seznam psů k adopci
            //return RedirectToAction("PsiKAdopci");
            return RedirectToAction("Index", "Rezervace");
        }

        



    }
}


