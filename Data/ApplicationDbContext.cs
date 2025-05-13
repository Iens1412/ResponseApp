using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResponseApp.Models;

namespace ResponseApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<InviteLink> InviteLinks { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
    }
}