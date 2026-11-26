using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.ViewModels
{
    public class CourseFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الكورس مطلوب")]
        [Display(Name = "اسم الكورس")]
        public string Title { get; set; }

        [Required(ErrorMessage = "وصف الكورس مطلوب")]
        [Display(Name = "وصف الكورس")]
        public string Description { get; set; }

        [Required(ErrorMessage = "عدد ساعات الكورس مطلوب")]
        [Range(1, 500, ErrorMessage = "عدد الساعات يجب أن يكون بين 1 و 500")]
        [Display(Name = "عدد الساعات")]
        public int TotalHours { get; set; }


        [Display(Name = "صوره الغلاف")]
        public IFormFile? ThumbnailFile { get; set; }
        public string? ExistingThumbnailPath { get; set; }

        [Required(ErrorMessage = " اختيار المحاضر")]
        [Range(1, int.MaxValue, ErrorMessage = "يرجى اختيار المحاضر")]
        [Display(Name = "المحاضر")]
        public int InstructorId { get; set; }

        
    }
}
