using FasonPortal.Data;
using FasonPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FasonPortal.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		[AllowAnonymous]  // Giriþ yapýlmadan eriþilebilir
		public IActionResult Index()
		{
			return View();
		}

		[AllowAnonymous]  // Giriþ yapýlmadan eriþilebilir
		public IActionResult Privacy()
		{
			return View();
		}

		[AllowAnonymous]  // Giriþ yapýlmadan eriþilebilir
		public async Task<IActionResult> TestDatabaseConnection()
		{
			try
			{
				var canConnect = await _context.Database.CanConnectAsync();
				if (canConnect)
				{
					return Content("Database connection successful!");
				}
				else
				{
					return Content("Database connection failed.");
				}
			}
			catch (Exception ex)
			{
				return Content($"An error occurred: {ex.Message}");
			}
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
