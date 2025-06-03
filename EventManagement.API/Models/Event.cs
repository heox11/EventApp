using System;
using System.Collections.Generic;

namespace EventManagement.API.Models
{
    public class Event
    {
        public Event()
        {
            Participants = new List<Participant>();
        }

        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime EventDate { get; set; }
        public required string Location { get; set; }
        public required string AdditionalInfo { get; set; }
        public ICollection<Participant> Participants { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 