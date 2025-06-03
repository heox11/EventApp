using System;

namespace EventManagement.API.Models
{
    public class Participant
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public ParticipantType Type { get; set; }
        
        // Individual participant fields
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PersonalCode { get; set; }
        
        // Company participant fields
        public string? CompanyName { get; set; }
        public string? RegistrationCode { get; set; }
        public int? NumberOfParticipants { get; set; }
        
        // Common fields
        public PaymentMethod PaymentMethod { get; set; }
        public string? AdditionalInfo { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum ParticipantType
    {
        Individual,
        Company
    }

    public enum PaymentMethod
    {
        BankTransfer,
        Cash
    }
} 