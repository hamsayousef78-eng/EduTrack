
using Microsoft.Identity.Client.NativeInterop;
using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم التاسك مطلوب")]
        [Display(Name = "اسم التاسك")]
        public string Title { get; set; }

        [Required(ErrorMessage = "تفاصيل التاسك مطلوبه")]
        [Display(Name = "شرح  المطلوب")]
        public string Description { get; set; }

        [Required(ErrorMessage = "آخر موعد للتسليم مطلوب")]
        [Display(Name = "آخر موعد للتسليم ")]
        public DateTime Deadline { get; set; }

        [Range(1, 1000)]
        [Display(Name = "الدرجة النهائية")]
        public int MaxDegree { get; set; } = 100;

        [Display(Name = "ملف التاسك")]
        public string? AttachmentFilePath { get; set; }
        //الكورس الواحد بيشرحه محاضر واحد
        [Display(Name = "الكورس")]
        public int CourseId { get; set; }
        //Navigation
        public Course? course { get; set; }

        //التاسك الواحد بيبقي ليه اكتر من ملف للحل
        public List<Submission>? submissions { get; set; }
    }
}


