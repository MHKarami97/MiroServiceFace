using Microsoft.AspNetCore.Mvc;

namespace Faces.Api.Controllers
{
    public class Faces : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}