using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public class ProgrammContext : DbContext
    {
        public DbSet<Programm> program { get; set; }
        public ProgrammContext(DbContextOptions<ProgrammContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
