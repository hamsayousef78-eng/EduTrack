using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Controllers
{
    public class InstructorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
