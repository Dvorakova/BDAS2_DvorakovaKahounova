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
        //        // Pokud je u�ivatel p�ihl�en�, pou�ijeme jeho e-mail, jinak pou�ijeme e-mail z formul��e
        //        var userEmail = User.Identity.IsAuthenticated ? User.FindFirstValue(ClaimTypes.Email) : email;

        //        // Pokud nen� zadan� e-mail (a u�ivatel nen� p�ihl�en�), vr�t�me chybovou hl�ku
        //        if (string.IsNullOrEmpty(userEmail))
        //        {
        //            TempData["ErrorMessage"] = "Pro odesl�n� zpr�vy mus�te b�t p�ihl�eni nebo zadat e-mail.";
        //            return RedirectToAction("Kontakty");
        //        }

        //        var fromAddress = new MailAddress(userEmail, "U�ivatel");  // Odes�latel bude m�t e-mail p�ihl�en�ho u�ivatele nebo z formul��e
        //        var toAddress = new MailAddress("utulek.lucky@seznam.cz", "Utulek Lucky");
        //        const string subject = "Nov� dotaz od u�ivatele";

        //        var smtp = new SmtpClient
        //        {
        //            Host = "smtp.seznam.cz", // Nap�. smtp.gmail.com pro Gmail
        //            Port = 587,
        //            EnableSsl = true,
        //            Credentials = new NetworkCredential("utulek.lucky@seznam.cz", "BeLucky7") // Tv� p�ihla�ovac� �daje
        //        };

        //        var mailMessage = new MailMessage(fromAddress, toAddress)
        //        {
        //            Subject = subject,
        //            Body = $"E-mail u�ivatele: {userEmail}\n\nZpr�va:\n{message}"
        //        };

        //        smtp.Send(mailMessage);

        //        // Po �sp�n�m odesl�n� p�esm�ruj nebo vra� n�jakou zp�tnou vazbu u�ivateli
        //        TempData["SuccessMessage"] = "Va�e zpr�va byla �sp�n� odesl�na.";
        //        return RedirectToAction("Kontakty");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Chybov� hl�ka
        //        TempData["ErrorMessage"] = $"Do�lo k chyb�: {ex.Message}";
        //        return RedirectToAction("Kontakty");
        //    }
        //}



    }
}
