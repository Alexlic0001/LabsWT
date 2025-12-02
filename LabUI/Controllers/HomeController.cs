using System.Diagnostics;
using LabUI.Models;
using Microsoft.AspNetCore.Mvc;

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
            _logger.LogInformation($"------> {User.Identity.IsAuthenticated}");
            return View(_logger);
        }

        public IActionResult Privacy()
        {
            return View();
        }

       
    }
}
