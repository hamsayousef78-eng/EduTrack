using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "اسم الطالب")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public int Phone { get; set; }

        [Display(Name = "الجامعة / الكلية")]
        public string? University { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الكورس")]
        [Display(Name = "الكورس المراد التسجيل به")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "كلمه المرور مطلوب")]
        [Display(Name = "كلمه المرور")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "تاكيد كلمه المرور مطلوب ")]
        [Display(Name = "تاكيد كلمه المرور")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; }
    }
}
