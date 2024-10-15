using Microsoft.EntityFrameworkCore;
using PrayReminder.Domain.Entities.Models;

namespace PrayReminder.Application.Abstractions
{
    public interface IApplicationDbContext
    {
        public DbSet<User> Users { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
