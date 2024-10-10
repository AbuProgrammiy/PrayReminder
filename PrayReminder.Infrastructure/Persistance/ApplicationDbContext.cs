using Microsoft.EntityFrameworkCore;
using PrayReminder.Application.Abstractions;
using PrayReminder.Domain.Entities.Models;

namespace PrayReminder.Infrastructure.Persistance
{
    public class ApplicationDbContext:DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
            Database.Migrate();
        }

        public DbSet<User> Users { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }
    }
}
