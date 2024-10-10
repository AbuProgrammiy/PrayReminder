using PrayReminder.Domain.Entities.Enums;

namespace PrayReminder.Domain.Entities.DTOs
{
    public class UpdateUserRegionDTO
    {
        public long ChatId { get; set; }
        public Region Region { get; set; }
    }
}
