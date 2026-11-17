using EduTrack.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace EduTrack.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Instructor> instructors { get; set; }

        public DbSet<Course> courses { get; set; }
        public DbSet<Student> students { get; set; }
        public DbSet<Submission> submissions { get; set; }
        public DbSet<Assignment> assignments { get; set; }
    }

}
