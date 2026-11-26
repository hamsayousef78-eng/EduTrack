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

        public CourseController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment,
                                 UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var currentUser = _userManager.GetUserId(User);

            if (User.IsInRole("Admin"))
            {
                var courses = _context.courses
                    .Include(c => c.instructor)
                    .Include(c => c.assignments)
                    .Include(c => c.students)
                    .ToList();
                return View(courses);
            }

            if (User.IsInRole("Instructor"))
            {
                var instructor = _context.instructors.FirstOrDefault(i => i.UserId == currentUser);
                if (instructor == null)
                {
                    return View(new List<Course>());
                }

                var courses = _context.courses
                    .Include(c => c.instructor)
                    .Include(c => c.assignments)
                    .Include(c => c.students)
                    .Where(c => c.InstructorId == instructor.Id)
                    .ToList();
                return View(courses);
            }

            if (User.IsInRole("Student"))
            {
                var student = _context.students.FirstOrDefault(s => s.UserId == currentUser);
                if (student == null)
                {
                    return View(new List<Course>());
                }

                var courses = _context.courses
                    .Include(c => c.instructor)
                    .Include(c => c.assignments)
                    .Include(c => c.students)
                    .Where(c => c.Id == student.CourseId)
                    .ToList();
                return View(courses);
            }

            return Forbid();//لو المستخدم ملوش role
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Instructors = new SelectList(_context.instructors.ToList(), "Id", "FullName");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Create(CourseFormViewModel model)
        {

            if (!ModelState.IsValid)
            {
                ViewBag.Instructors = new SelectList(_context.instructors.ToList(), "Id", "FullName");
                return View(model);
            }

            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                TotalHours = model.TotalHours,
                InstructorId = model.InstructorId
            };

            if (model.ThumbnailFile is { Length: > 0 })
            {
                course.ThumbnailPath = SaveImage(model.ThumbnailFile);
            }

            _context.courses.Add(course);
            _context.SaveChanges();

            TempData["Success"] = "تم إضافة الكورس بنجاح";
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id)
        {
            var course = _context.courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.Instructors = new SelectList(_context.instructors.ToList(), "Id", "FullName");
            return View(new CourseFormViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                TotalHours = course.TotalHours,
                InstructorId = course.InstructorId,
                ExistingThumbnailPath = course.ThumbnailPath
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id, CourseFormViewModel model)
        {
            var course = _context.courses.FirstOrDefault(c => c.Id == id);
            if (course == null)
            {
                return NotFound();
            }

            //model.Id = id;مش فاهمه
  

            if (!ModelState.IsValid)
            {
                ViewBag.Instructors = new SelectList(_context.instructors.ToList(), "Id", "FullName");
                model.ExistingThumbnailPath = course.ThumbnailPath;
                return View(model);
            }

            var oldImagePath = course.ThumbnailPath;
            var imageChanged = false;

            if (model.ThumbnailFile is { Length: > 0 })
            {
                course.ThumbnailPath = SaveImage(model.ThumbnailFile);
                imageChanged = true;
            }

            course.Title = model.Title;
            course.Description = model.Description;
            course.TotalHours = model.TotalHours;
            course.InstructorId = model.InstructorId;

            _context.SaveChanges();

            // نحذف الصورة القديمة بعد نجاح حفظ الصورة الجديدة والبيانات
            if (imageChanged)
            {
                DeleteImage(oldImagePath);
            }

            TempData["Success"] = "تم تعديل الكورس بنجاح";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var course = _context.courses
                .Include(c => c.instructor)
                .Include(c => c.students)
                .Include(c => c.assignments)
                    .ThenInclude(a => a.submissions)
                .FirstOrDefault(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

    

     

        private string SaveImage(IFormFile imageFile)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "courses");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream);
            }

            // نخزن اسم الملف فقط لأن الـ Views تضيف ~/images/courses/ بنفسها
            return uniqueFileName;
        }

        private void DeleteImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }


        }

    }
}
