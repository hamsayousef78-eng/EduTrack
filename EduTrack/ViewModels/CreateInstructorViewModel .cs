using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    public class CreateInstructorViewModel
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "اسم المحاضر")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "التخصص مطلوب")]
        [Display(Name = "التخصص")]
        public string Specialization { get; set; }


        [Display(Name = "الصورة الشخصية")]
        public string? ImagePath { get; set; }//لي  IFormFile

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمه المرور مطلوب")]
        [Display(Name = "كلمه المرور")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
