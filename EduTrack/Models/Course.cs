using Microsoft.Identity.Client.NativeInterop;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الكورس مطلوب")]
        [Display(Name = "اسم الكورس")]
        public string Title { get; set; }

        [Required(ErrorMessage = "وصف الكورس مطلوب")]
        [Display(Name = "وصف الكورس")]
        public int Description { get; set; }

        [Required(ErrorMessage = "عدد ساعات الكورس مطلوب")]
        [Range(1, 500, ErrorMessage = "عدد الساعات يجب أن يكون بين 1 و 500")]
        [Display(Name = "عدد الساعات")]
        public int TotalHours { get; set; }

        [Display(Name = "صوره الغلاف")]
        public string? ThumbnailPath { get; set; }
        //الكورس الواحد بيشرحه محاضر واحد
        [Display(Name = "المحاضر")]
        public int InstructorId { get; set; }
        //Navigation
        public Instructor? instructor { get; set; }
        //الكورس الواحد فيه اكتر من طالب
        public List<Student>? students { get; set; }

        //الكورس الواحد فيه اكتر من تاسك
        public List<Assignment>? assignments { get; set; }
    }
}

