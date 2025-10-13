using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
            public DbSet<User> User { get; set; }
            public DbSet<Volunteer> Volunteers { get; set; }
            public DbSet<Admin> Admins { get; set; }
            public DbSet<Organization> Organizations { get; set; }
            public DbSet<Event> Events { get; set; }

    }
    
}