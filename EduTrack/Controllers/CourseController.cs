using EduTrack.Models;
using EduTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public CourseController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment,
                                 UserManager<ApplicationUser> userManager,
                                 RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Authorize]
        public IActionResult Index()
        {
            string currentUser = _userManager.GetUserId(User);
            if (User.IsInRole("Admin"))
            {
                var course = _context.courses.
                             Include(c => c.instructor).
                             Include(c => c.assignments).
                             Include(c => c.students).ToList();
                return View(course);
            }

            if (User.IsInRole("Instructor"))
            {
                var instructor = _context.instructors.FirstOrDefault(c => c.UserId == currentUser);
                if (instructor != null)
                {
                    var course = _context.courses.
                               Include(c => c.instructor).
                               Include(c => c.assignments).
                               Include(c => c.students).Where(c => c.InstructorId == instructor.Id).ToList();
                    return View(course);
                }
            }
            if (User.IsInRole("Student"))
            {
                var student = _context.students.FirstOrDefault(c => c.UserId == currentUser);
                if (student != null)
                {
                    var course = _context.courses.
                               Include(c => c.instructor).
                               Include(c => c.assignments).
                               Include(c => c.students).Where(c => c.Id == student.CourseId).ToList();
                    return View(course);
                }
            }
            return View();
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.instructors = new SelectList(_context.instructors.ToList(), "Id", "FullName");
            return View();
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(CourseFormViewModel model, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.instructors = new SelectList(_context.instructors.ToList(), "Id", "FullName");
                return View(model);
            }
            if (imageFile != null && imageFile.Length > 0)
            {
                model.ThumbnailFile = SaveImage(imageFile);
            }

            Course course = new Course();
            course.Title = model.Title;
            course.Description = model.Description;
            course.TotalHours = model.TotalHours;
            course.ThumbnailPath = model.ThumbnailFile;//صح ولا
            course.InstructorId = model.InstructorId;
            _context.courses.Add(course);
            _context.SaveChanges();
            return RedirectToAction("Index", "course");
        }


        [Authorize]
        public IActionResult Details(int id)
        {
            var course = _context.courses.
                Include(c => c.instructor).
                Include(c => c.students).
                Include(c => c.assignments).
                 ThenInclude(c => c.submissions).
                FirstOrDefault(i => i.Id == id);

            if (course != null)
            {
                return View(course);
            }
            else
            {
                return NotFound();
            }
        }


        //save image
        private string SaveImage(IFormFile? imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "instructors");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            string extension = Path.GetExtension(imageFile.FileName);
            string uniqueFileName = Guid.NewGuid().ToString() + extension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream);
            }
            return $"/images/instructors/{uniqueFileName}";
        }



    }
}




















