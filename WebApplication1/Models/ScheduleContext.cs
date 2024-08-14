namespace WebApplication1.Models
{
    using Microsoft.EntityFrameworkCore;

    public class ScheduleContext : DbContext
    {
        public DbSet<Schedule> schedule { get; set; }
        public ScheduleContext(DbContextOptions<ScheduleContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
