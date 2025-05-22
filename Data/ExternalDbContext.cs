using Microsoft.EntityFrameworkCore;
using ResponseApp.Models;

namespace ResponseApp.Data
{
    public class ExternalDbContext : DbContext
    {
        public ExternalDbContext(DbContextOptions<ExternalDbContext> options)
            : base(options) { }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Response> Responses { get; set; }
    }
}