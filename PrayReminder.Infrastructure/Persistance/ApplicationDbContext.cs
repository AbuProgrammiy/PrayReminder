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
        public DbSet<Quote> Quotes { get; set; }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Quote>().HasData
            (
                new Quote { Id = 1, Body = "namozga shoshiling ish qochib ketmaydi" },
                new Quote { Id = 2, Body = "“Albatta, namoz mo‘minlarga vaqtida farz qilingandir” (Niso surasi, 103-oyat)" },
                new Quote { Id = 3, Body = "yashang ishni tashang, namoz vaqti bo'ldi" }
            );
        }
    }
}
