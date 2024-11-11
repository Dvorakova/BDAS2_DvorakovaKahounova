using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Index()
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
    }
}


