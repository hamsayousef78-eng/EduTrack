using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Controllers
{
    public class StudentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
