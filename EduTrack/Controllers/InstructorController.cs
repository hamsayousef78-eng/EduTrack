using EduTrack.Models;
using EduTrack.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace EduTrack.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InstructorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public InstructorController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment ,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            var instructor = _context.instructors.Include(i => i.courses).Include(i => i.User).ToList();
            return View(instructor);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(CreateInstructorViewModel model, IFormFile? imageFile)
        {


            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (imageFile != null && imageFile.Length > 0)
            {
                model.ImagePath = SaveImage(imageFile);
            }
            ApplicationUser newUser = new ApplicationUser()
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email
            };
            var result = _userManager.CreateAsync(newUser, model.Password).Result;
            if (result.Succeeded == true)
            {
                _userManager.AddToRoleAsync(newUser, "Instructor").Wait();
               // _signInManager.SignInAsync(newUser, isPersistent: false);

                Instructor instructor = new Instructor();
                //بحط اي متغير انا عملاه انه مطلوب
                instructor.FullName = model.FullName;
                //instructor.Specialization = model.Specialization;
                instructor.UserId = newUser.Id;
                instructor.ImagePath = model.ImagePath;
                _context.instructors.Add(instructor);
                _context.SaveChanges();
                _signInManager.SignInAsync(newUser, isPersistent: false);
                return RedirectToAction("Index", "Instructor");
            }
            return View();
        }

        public IActionResult Details(int id)
        {
            var instructor = _context.instructors.Include(i => i.courses).Include(i => i.User).FirstOrDefault(i => i.Id == id);

            if (instructor != null)
            {
                return View(instructor);
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
