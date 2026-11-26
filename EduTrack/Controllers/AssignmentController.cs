using EduTrack.Models;
using EduTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize]
    public class AssignmentController : Controller
    {
        private static readonly string[] AllowedAttachmentExtensions = { ".pdf" };

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignmentController(ApplicationDbContext context,
                                    IWebHostEnvironment webHostEnvironment,
                                    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public IActionResult Create(int courseId)
        {
            var course = _context.courses.FirstOrDefault(c => c.Id == courseId);
            if (course == null)
            {
                return NotFound();
            }

            if (!CanManageCourse(course))
            {
                return Forbid();
            }

            return View(new CreateAssignmentViewModel
            {
                CourseId = course.Id,
                CourseTitle = course.Title
            });
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateAssignmentViewModel model)
        {
            var course = _context.courses.FirstOrDefault(c => c.Id == model.CourseId);
            if (course == null)
            {
                return NotFound();
            }

            if (!CanManageCourse(course))
            {
                return Forbid();
            }

            model.CourseTitle = course.Title;

            if (model.AttachmentFile is { Length: > 0 })
            {
                var extension = Path.GetExtension(model.AttachmentFile.FileName).ToLowerInvariant();
                if (!AllowedAttachmentExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.AttachmentFile),
                        "ملف شرح التاسك يجب أن يكون بصيغة PDF فقط.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var assignment = new Assignment
            {
                Title = model.Title,
                Description = model.Description,
                Deadline = model.Deadline,
                MaxDegree = model.MaxDegree,
                CourseId = model.CourseId
            };

            if (model.AttachmentFile is { Length: > 0 })
            {
                assignment.AttachmentFilePath = SaveAttachment(model.AttachmentFile);
            }

            _context.assignments.Add(assignment);
            _context.SaveChanges();

            TempData["Success"] = "تم نشر التاسك بنجاح";
            return RedirectToAction("Details","course", new { id = course.Id });
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var assignment = _context.assignments
                .Include(a => a.course)
                .Include(a => a.submissions)
                    .ThenInclude(s => s.student)
                .FirstOrDefault(a => a.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            if (!CanViewAssignment(assignment))
            {
                return Forbid();
            }

            return View(assignment);
        }

        private bool CanManageCourse(Course course)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            if (!User.IsInRole("Instructor"))
            {
                return false;
            }

            var instructor = GetCurrentInstructor();
            return instructor != null && course.InstructorId == instructor.Id;
        }

        private bool CanViewAssignment(Assignment assignment)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            if (User.IsInRole("Instructor"))
            {
                return assignment.course != null && CanManageCourse(assignment.course);
            }

            if (User.IsInRole("Student"))
            {
                var student = GetCurrentStudent();
                return student != null && student.CourseId == assignment.CourseId;
            }

            return false;
        }

        private Instructor? GetCurrentInstructor()
        {
            var userId = _userManager.GetUserId(User);
            return _context.instructors.FirstOrDefault(i => i.UserId == userId);
        }

        private Student? GetCurrentStudent()
        {
            var userId = _userManager.GetUserId(User);
            return _context.students.FirstOrDefault(s => s.UserId == userId);
        }

        private string SaveAttachment(IFormFile file)
        {
            var folder = Path.Combine(GetWebRoot(), "uploads", "assignments");
            Directory.CreateDirectory(folder);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }

        private string GetWebRoot()
        {
            return _webHostEnvironment.WebRootPath
                ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
        }
    }
}
