using EduTrack.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EduTrack.Controllers
{
    public class AccountController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        //Register
        public IActionResult Register()
        {
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
                _signInManager.SignInAsync(newUser, isPersistent: false);
                return RedirectToAction("Index");
            }
            return View();
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
    }
}
