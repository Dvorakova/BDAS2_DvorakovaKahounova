using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BDAS2_DvorakovaKahounova.Controllers
{
    //[Authorize]
    public class PrihlasenyUzivatelController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost] // Pouze pro POST žádosti
        public async Task<IActionResult> Logout()
        {

            // Přesměrovat na domovskou stránku
            return RedirectToAction("Index", "Home");
        }
    }
}
