using System.ComponentModel.DataAnnotations;

namespace EduTrack.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الطالب مطلوب")]
        [Display(Name = "اسم الطالب")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public int Phone { get; set; }

        [Display(Name = "الجامعة / الكلية")]
        public string? University { get; set; }

        //الطالب بيسجل في كورس واحد
        [Display(Name = "الكورس")]
        public int CourseId {  get; set; }
        //Navigation
        public Course? course { get; set; }

        //الطالب الواحد بيسلم sاكتر من ملف للحل
        public List<Submission>? submissions { get; set; }
    }
}

