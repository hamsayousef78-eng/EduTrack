
using Microsoft.Identity.Client.NativeInterop;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Submission
    {
        public int Id { get; set; }

        [Display(Name = "تاريخ التسليم")]
        public DateTime SubmittedAt { get; set; } 

        [Required(ErrorMessage = "ملف الحل مطلوب ")]
        [Display(Name = " ملف الحل")]
        public string SolutionFilePath { get; set; }

        [Display(Name = "ملاحظات الطالب")]
        public string? StudentNotes { get; set; }

        [Display(Name = "الدرجة")]
        public int? Grade { get; set; }

        [Display(Name = "تعليق المحاضر ")]
        public string? InstructorFeedback { get; set; }

        [Required]
        [Display(Name = "الحالة")]
        public string Status { get; set; } 

        //ملف الحل الواحد بيكون ل تاسك واحد
        [Display(Name = "التاسك")]
        public int AssignmentId{ get; set; }
        //Navigation
        public Assignment? assignment { get; set; }
        // الملف الواحد بيرفعه طالب واحد
        [Display(Name = "الطالب")]
        public int StudentId { get; set; }
        //Navigation
        public Student? student { get; set; }
    }
}


