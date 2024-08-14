namespace WebApplication1.Models
{
    using Microsoft.EntityFrameworkCore;

        public class UsersContext : DbContext
        {
            public DbSet<User> users { get; set; }
            public UsersContext(DbContextOptions<UsersContext> options)
                : base(options)
            {
                Database.EnsureCreated();
            }
        }
}
