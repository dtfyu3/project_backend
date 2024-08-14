using Microsoft.EntityFrameworkCore;
using System;

namespace WebApplication1.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
        }
        public DbSet<User> users { get; set; }
        public DbSet<Schedule> schedule { get; set; }
        public DbSet<Programm> program { get; set; }
    }
}
