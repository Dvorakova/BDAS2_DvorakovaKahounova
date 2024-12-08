using BDAS2_DvorakovaKahounova.DataAcessLayer;
using BDAS2_DvorakovaKahounova.Models;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Diagnostics;
using System.Net.Mail;
using System.Net;
using System.Security.Claims;
using BDAS2_DvorakovaKahounova.Email;

namespace BDAS2_DvorakovaKahounova.Controllers
{
    public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		private readonly HomeDataAccess _dataAccess;
		private readonly IEmailSender _emailSender;

		public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IEmailSender emailSender)
		{
			_logger = logger;
			string connectionString = configuration.GetConnectionString("OracleConnection");
			_dataAccess = new HomeDataAccess(connectionString);
			this._emailSender = emailSender;
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
			return View(new EmailModel());
		}

		[HttpPost]
		public async Task<IActionResult> Kontakty(EmailModel model, string email)
		{
			if (!ModelState.IsValid)
			{
				var receiver = "utuleklucky@gmail.com";

				string subject = "email od nepøihlášeného uživatele.";
				if (User.Identity.IsAuthenticated)
				{
					var userNameClaim = User.FindFirst(ClaimTypes.Name);
					string userName = userNameClaim?.Value;
					subject = "email od uživatele: " + userName;
				}

				await _emailSender.SendEmailAsync(email, receiver, subject, model.Message);

                TempData["SuccessMessage"] = "Email se podaøilo odeslat.";
				return View(new EmailModel());
			}

			TempData["ErrorMessage"] = "Nastala neoèekávaná chyba.";

            return View(model);
		}
	}
}
