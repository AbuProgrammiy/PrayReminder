﻿namespace PrayReminder.Domain.Entities.Models
{
    public class Quote
    {
        public Guid Id { get; set; }
        public string Body { get; set; }
        public string? Author { get; set; }
    }
}
