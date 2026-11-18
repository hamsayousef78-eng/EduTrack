using EduTrack.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EduTrack.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewBag.CoursesCount = _context.courses.Count();
            ViewBag.InstructorsCount = _context.instructors.Count();
            ViewBag.StudentsCount = _context.students.Count();
            ViewBag.SubmissionsCount = _context.submissions.Count();
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
    }
}
