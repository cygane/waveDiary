using Microsoft.EntityFrameworkCore;
using project.Data;

namespace project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<UsersLike> UsersLikes { get; set; }
    }
}
