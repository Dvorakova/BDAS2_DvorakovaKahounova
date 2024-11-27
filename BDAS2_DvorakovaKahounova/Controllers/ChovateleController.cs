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
	}
}
