using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;

namespace BDAS2_DvorakovaKahounova.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

		private readonly HomeDataAccess _dataAccess;

		public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
			string connectionString = configuration.GetConnectionString("OracleConnection");
			_dataAccess = new HomeDataAccess(connectionString);
		}


		public IActionResult Index()
        {

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View();
        }

        public IActionResult Privacy()
        {

			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
		{
			var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Kontakty()
		{
            if (User.Identity.IsAuthenticated)
            {
                string email = User.FindFirstValue(ClaimTypes.Email);

                ViewBag.UserEmail = email;
            }
            else
            {
                ViewBag.UserEmail = "";
            }

            var originallRole = HttpContext.Session.GetString("OriginalRole");
			ViewData["IsAdmin"] = (User.Identity.IsAuthenticated && (originallRole == "A" || User.IsInRole("A")));

			ViewBag.Title = "Kontakty";
            return View();
        }


        //[HttpPost]
        //public IActionResult SendEmail(string email, string message)
        //{
        //    try
        //    {
        //        // Pokud je uivatel pøihlášenı, pouijeme jeho e-mail, jinak pouijeme e-mail z formuláøe
        //        var userEmail = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.Email) : email;

        //        // Pokud není zadanı e-mail (a uivatel není pøihlášenı), vrátíme chybovou hlášku
        //        if (string.IsNullOrEmpty(userEmail))
        //        {
        //            TempData["ErrorMessage"] = "Pro odeslání zprávy musíte bıt pøihlášeni nebo zadat e-mail.";
        //            return RedirectToAction("Kontakty");
        //        }

        //        var fromAddress = new MailAddress(userEmail, "Uivatel");  // Odesílatel bude mít e-mail pøihlášeného uivatele nebo z formuláøe
        //        var toAddress = new MailAddress("utulek.lucky@seznam.cz", "Utulek Lucky");
        //        const string subject = "Novı dotaz od uivatele";

        //        var smtp = new SmtpClient
        //        {
        //            Host = "smtp.seznam.cz", // Napø. smtp.gmail.com pro Gmail
        //            Port = 587,
        //            EnableSsl = true,
        //            Credentials = new NetworkCredential("utulek.lucky@seznam.cz", "BeLucky7") // Tvé pøihlašovací údaje
        //        };

        //        var mailMessage = new MailMessage(fromAddress, toAddress)
        //        {
        //            Subject = subject,
        //            Body = $"E-mail uivatele: {userEmail}\n\nZpráva:\n{message}"
        //        };

        //        smtp.Send(mailMessage);

        //        // Po úspìšném odeslání pøesmìruj nebo vra nìjakou zpìtnou vazbu uivateli
        //        TempData["SuccessMessage"] = "Vaše zpráva byla úspìšnì odeslána.";
        //        return RedirectToAction("Kontakty");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Chybová hláška
        //        TempData["ErrorMessage"] = $"Došlo k chybì: {ex.Message}";
        //        return RedirectToAction("Kontakty");
        //    }
        //}



    }
}
