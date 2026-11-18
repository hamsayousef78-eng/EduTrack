using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "اسم المحاضر مطلوب")]
        [Display(Name = "اسم المحاضر")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "التخصص مطلوب")]
        [Display(Name = "التخصص")]
        public int Specialization { get; set; }

        [Display(Name = "الصورة الشخصية")]
        public string? ImagePath { get; set; }

        [Display(Name = "نبذة عن المحاضر")]
        public string? Bio { get; set; }

        //المحاضر الواحد بيدي اكتر من كورس
        public List<Course> ?courses { get; set; }


        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
