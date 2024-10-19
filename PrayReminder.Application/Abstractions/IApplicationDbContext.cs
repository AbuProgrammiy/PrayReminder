using Microsoft.EntityFrameworkCore;
using PrayReminder.Domain.Entities.Models;

namespace PrayReminder.Application.Abstractions
{
    public interface IApplicationDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
