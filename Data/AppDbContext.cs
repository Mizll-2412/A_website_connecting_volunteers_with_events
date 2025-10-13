using Microsoft.EntityFrameworkCore;
using khoaluantotnghiep.Models;

namespace khoaluantotnghiep.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
            public DbSet<User> User { get; set; }
            public DbSet<Volunteer> Volunteer { get; set; }
            public DbSet<Admin> Admin { get; set; }
            public DbSet<Organization> Organization { get; set; }
            public DbSet<Event> Event { get; set; }

    }
    
}