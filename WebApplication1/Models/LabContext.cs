using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class LabContext : DbContext
    {
        public DbSet<Lab> lab { get; set; }
        public LabContext(DbContextOptions<LabContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
