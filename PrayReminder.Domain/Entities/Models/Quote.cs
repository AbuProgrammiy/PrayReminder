namespace PrayReminder.Domain.Entities.Models
{
    public class Quote
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public string? Author { get; set; }
    }
}
