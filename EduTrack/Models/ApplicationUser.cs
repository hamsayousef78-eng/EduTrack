using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace EduTrack.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "الاسم بالكامل مطلوب")]//يعني  ادخال الاسم اجباري ولو ما دخلش هيظهر رساله انه مطلوب
        [Display(Name = "الاسم بالكامل")]//الاسم اللي هيظهر للمستخدم
        public string FullName { get; set; }
    }
}
