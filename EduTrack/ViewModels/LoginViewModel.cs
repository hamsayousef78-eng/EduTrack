using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }
        [Required(ErrorMessage = "كلمه المرور مطلوب")]
        [Display(Name = "كلمه المرور")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }

    }
}
