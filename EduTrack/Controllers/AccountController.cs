using EduTrack.Models;
using EduTrack.ViewModels;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.Courses = new SelectList(
                _context.courses.ToList(), "Id", "Title", model.CourseId);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            ApplicationUser newUser = new ApplicationUser()
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);

            if (result.Succeeded)
            {
                var roleResult = await _userManager.AddToRoleAsync(newUser, "Student");

                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    await _userManager.DeleteAsync(newUser);
                    return View(model);
                }

                Student student = new Student()
                {
                    FullName = model.FullName,
                    Phone = model.Phone,
                    UserId = newUser.Id,
                    CourseId = model.CourseId
                };

                _context.students.Add(student);
                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(newUser, false);

                return RedirectToAction("Index", "Course");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }




        //Login
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {

            var result = _signInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, lockoutOnFailure: false).Result;
            if (result.Succeeded == true)
            {
                return RedirectToAction("Index","Home");
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
            return RedirectToAction("Index","Home");
        }
        //create SetupSystem
        public async Task<IActionResult> SetupSystem()// التعامل مع قاعدة البيانات باستخدام await
        {
            //انشاء roles
            IdentityRole r1 = new IdentityRole();
            r1.Name = "Admin";
            IdentityRole r2 = new IdentityRole();
            r2.Name = "Instructor";
            IdentityRole r3 = new IdentityRole();
            r3.Name = "Student";

            //عشان احفظ ل roles في الداتا بيز
            await _roleManager.CreateAsync(r1);//ابدأ العملية وانتظر اكتمالها قبل الانتقال للسطر التالي
            await _roleManager.CreateAsync(r2);
            await _roleManager.CreateAsync(r3);

            //create admin 
            ApplicationUser  Admin=new ApplicationUser();//مش عوزه احطه من ضمن ال اختيارات roles
            Admin.FullName = "hamsa";
            Admin.Email = "hamsa@gmail.com";
            Admin.UserName = "hamsa@gmail.com";
            var result = await _userManager.CreateAsync(Admin, "admin1234");//انشاء حساب واحفظه في الداتا بيز
            if (result.Succeeded == true)
            {
                _userManager.AddToRoleAsync(Admin,"Admin").Wait();//بضيف role للمستخدم

            }
            return Content("system setup completed successfully");
        }
    }
}
