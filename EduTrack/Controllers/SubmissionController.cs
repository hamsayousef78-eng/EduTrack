using EduTrack.Models;
using EduTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduTrack.Controllers
{
    [Authorize]
    public class SubmissionController : Controller
    {
        private static readonly string[] AllowedSolutionExtensions = { ".pdf", ".zip" };

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubmissionController(ApplicationDbContext context,
                                    IWebHostEnvironment webHostEnvironment,
                                    UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public IActionResult Submit(int assignmentId)
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return Forbid();
            }

            var assignment = _context.assignments
                .Include(a => a.course)
                .FirstOrDefault(a => a.Id == assignmentId);

            if (assignment == null)
            {
                return NotFound();
            }

            if (assignment.CourseId != student.CourseId)
            {
                return Forbid();
            }

            return View(new SubmitAssignmentViewModel
            {
                AssignmentId = assignment.Id,
                AssignmentTitle = assignment.Title,
                CourseTitle = assignment.course?.Title,
                Deadline = assignment.Deadline
            });
        }

        [HttpPost]
        [Authorize(Roles = "Student")]

        public IActionResult Submit(SubmitAssignmentViewModel model)
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return Forbid();
            }

            var assignment = _context.assignments
                .Include(a => a.course)
                .FirstOrDefault(a => a.Id == model.AssignmentId);

            if (assignment == null)
            {
                return NotFound();
            }

            if (assignment.CourseId != student.CourseId)
            {
                return Forbid();
            }

            model.AssignmentTitle = assignment.Title;
            model.CourseTitle = assignment.course?.Title;
            model.Deadline = assignment.Deadline;

            if (model.SolutionFile == null || model.SolutionFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.SolutionFile), "يرجى إرفاق ملف الحل.");
            }
            else
            {
                var extension = Path.GetExtension(model.SolutionFile.FileName).ToLowerInvariant();
                if (!AllowedSolutionExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.SolutionFile),
                        "ملف الحل يجب أن يكون بصيغة PDF أو ZIP.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var submission = new Submission
            {
                AssignmentId = assignment.Id,
                StudentId = student.Id,
                SolutionFilePath = SaveSolution(model.SolutionFile!),
                StudentNotes = model.StudentNotes,
                SubmittedAt = DateTime.Now,
                Status = "Pending"
            };

            _context.submissions.Add(submission);
            _context.SaveChanges();

            TempData["Success"] = "تم تسليم الحل بنجاح";
            return RedirectToAction(nameof(MySubmissions));
        }

        [HttpGet]
        [Authorize(Roles = "Student")]
        public IActionResult MySubmissions()
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return View(new List<Submission>());
            }

            var submissions = _context.submissions
                .Include(s => s.assignment!)
                    .ThenInclude(a => a.course!)
                .Where(s => s.StudentId == student.Id)
                .OrderByDescending(s => s.SubmittedAt)
                .ToList();

            return View(submissions);
        }

        [HttpGet]
        [Authorize(Roles = "Instructor,Admin")]
        public IActionResult Grade(int id)
        {
            var submission = _context.submissions
                .Include(s => s.assignment!)
                    .ThenInclude(a => a.course!)
                .Include(s => s.student)
                .FirstOrDefault(s => s.Id == id);

            if (submission == null)
            {
                return NotFound();
            }

            if (!CanGrade(submission))
            {
                return Forbid();
            }

            return View(MapGradeViewModel(submission));//دي Method مساعدة بتحوّل الـ Entity لـ ViewModel
        }

        [HttpPost]
        [Authorize(Roles = "Instructor,Admin")]
        public IActionResult Grade(GradeSubmissionViewModel model)
        {
            var submission = _context.submissions
                .Include(s => s.assignment!)
                    .ThenInclude(a => a.course!)
                .Include(s => s.student)
                .FirstOrDefault(s => s.Id == model.SubmissionId);

            if (submission == null)
            {
                return NotFound();
            }

            if (!CanGrade(submission))
            {
                return Forbid();
            }

            var maxDegree = submission.assignment?.MaxDegree ?? 100;
            if (model.Grade < 0 || model.Grade > maxDegree)
            {
                ModelState.AddModelError(nameof(model.Grade),
                    $"الدرجة يجب أن تكون بين 0 و {maxDegree}.");
            }

            if (!ModelState.IsValid)
            {
                var viewModel = MapGradeViewModel(submission);
                viewModel.Grade = model.Grade;
                viewModel.InstructorFeedback = model.InstructorFeedback;
                return View(viewModel);
            }

            submission.Grade = model.Grade;
            submission.InstructorFeedback = model.InstructorFeedback;
            submission.Status = "Graded";

            _context.SaveChanges();

            TempData["Success"] = "تم رصد الدرجة بنجاح";
            return RedirectToAction("Details", "Assignment",new { id = submission.assignment.Id });
        }

        private GradeSubmissionViewModel MapGradeViewModel(Submission submission)
        {
            return new GradeSubmissionViewModel
            {
                SubmissionId = submission.Id,
                AssignmentId = submission.AssignmentId,
                StudentName = submission.student?.FullName,
                AssignmentTitle = submission.assignment?.Title,
                MaxDegree = submission.assignment?.MaxDegree ?? 100,
                SolutionFilePath = submission.SolutionFilePath,
                StudentNotes = submission.StudentNotes,
                SubmittedAt = submission.SubmittedAt,
                Grade = submission.Grade ?? 0,
                InstructorFeedback = submission.InstructorFeedback
            };
        }

        private bool CanGrade(Submission submission)
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
            return instructor != null
                && submission.assignment?.course != null
                && submission.assignment.course.InstructorId == instructor.Id;
        }

        private Student? GetCurrentStudent()
        {
            var userId = _userManager.GetUserId(User);
            return _context.students.FirstOrDefault(s => s.UserId == userId);
        }

        private Instructor? GetCurrentInstructor()
        {
            var userId = _userManager.GetUserId(User);
            return _context.instructors.FirstOrDefault(i => i.UserId == userId);
        }

        private string SaveSolution(IFormFile file)
        {
            var folder = Path.Combine(GetWebRoot(), "uploads", "submissions");
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
