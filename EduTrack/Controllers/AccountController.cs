using EduTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;

namespace EduTrack.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }
        //Register
        public IActionResult Register()
        {
            ViewBag.course = new SelectList(_context.courses.ToList(),"Id", "Title");
            return View();
        }
        [HttpPost]
        public IActionResult Register(string fullname, string email, string password)
        {
            ApplicationUser newUser = new ApplicationUser()
            {
                FullName = fullname,
                Email = email,
                UserName = email
            };
            var result = _userManager.CreateAsync(newUser, password).Result;
            if (result.Succeeded == true)
            {
                _userManager.AddToRoleAsync(newUser,"Student").Wait();
                _signInManager.SignInAsync(newUser, isPersistent: false);
            }

            Student student = new Student();
            student.UserId = newUser.Id;
            //student.CourseId = courseId;
            _context.students.Add(student);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }


        //Login
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(string email, string password)
        {

            var result = _signInManager.PasswordSignInAsync(email, password, isPersistent: false, lockoutOnFailure: false).Result;
            if (result.Succeeded == true)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = "الايميل او الباسورد غير صحيحه";
                return View();
            }
        }

        //Logout
        public IActionResult Logout()
        {
            _signInManager.SignOutAsync().Wait();
            return RedirectToAction("Index");
        }
        //create SetupSystem
        public IActionResult SetupSystem()
        {
            //انشاء roles
            IdentityRole r1 = new IdentityRole();
            r1.Name = "Admin";
            IdentityRole r2 = new IdentityRole();
            r2.Name = "Instructor";
            IdentityRole r3 = new IdentityRole();
            r3.Name = "Student";

            //عشان احفظ ل roles في الداتا بيز
            _roleManager.CreateAsync(r1);
            _roleManager.CreateAsync(r2);
            _roleManager.CreateAsync(r3);

            //create admin 
            ApplicationUser  Admin=new ApplicationUser();//مش عوزه احطه من ضمن ال اختيارات roles
            Admin.FullName = "hamsa";
            Admin.Email = "hamsa@gmail.com";
            Admin.UserName = "hamsa@gmail.com";
            var result = _userManager.CreateAsync(Admin, "admin1234").Result;//انشاء حساب واحفظه في الداتا بيز
            if (result.Succeeded == true)
            {
                _userManager.AddToRoleAsync(Admin,"Admin").Wait();//بضيف role للمستخدم

            }
            return Content("system setup completed successfully");
        }
    }
}
