using PrayReminder.Domain.Entities.Enums;

namespace PrayReminder.Domain.Entities.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public long ChatId { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Region? Region { get; set; }
        public bool IsBlocked { get; set; }
    }
}
