using AjaxDemo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AjaxDemo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly MydbContext _context;
        public HomeController(ILogger<HomeController> logger, MydbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult JsonTest()
        {
            return View();
        }

        public IActionResult JsonHomework()
        {
            return View();
        }

        public IActionResult First()
        {
            var categories = _context.Categories;
            return View(categories);
        }

        public IActionResult Address()
        {
            return View();
        }

        public IActionResult ShowImages()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult Travel()
        {
            return View();
        }

        public IActionResult Cors() 
        {
            return View();
        }
    }
}
