using PrayReminder.Domain.Entities.Enums;

namespace PrayReminder.Domain.Entities.Models
{
    public class PrayTimes
    {
        public int Id { get; set; }
        public TimeOnly Bomdod { get; set; }
        public TimeOnly Quyosh { get; set; }
        public TimeOnly Peshin { get; set; }
        public TimeOnly Asr { get; set; }
        public TimeOnly Shom { get; set; }
        public TimeOnly Xufton { get; set; }
        public Region Region { get; set; }
        public DateOnly Date { get; set; }
    }
}
