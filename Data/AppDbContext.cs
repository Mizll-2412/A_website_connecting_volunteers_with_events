using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
            public DbSet<User> User { get; set; }

    }
    
}