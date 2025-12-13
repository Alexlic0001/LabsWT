using System.Diagnostics;
using LabUI.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace LabUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            Log.Information("Hello из метода Index контроллера Home!");
            return View();

            _logger.LogInformation($"------> {User.Identity.IsAuthenticated}");
            return View(_logger);

            
        }

        public IActionResult Privacy()
        {
            return View();
        }

       
    }
}
