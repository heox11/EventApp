using System;

namespace EventManagement.API.Models
{
    public class ParticipantDto
    {
        public ParticipantType Type { get; set; }
        
        // Individual participant fields
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PersonalCode { get; set; }
        
        // Company participant fields
        public string? Name { get; set; }
        public string? ReigstrationCode { get; set; }
        public string? participantsnR { get; set; }
        
        // Common fields
        public PaymentMethod PaymentMethod { get; set; }
        public string? lisainfo { get; set; }
    }

    public class UpdateParticipantDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PersonalCode { get; set; }
        public string? CompanyName { get; set; }
        public string? RegistrationCode { get; set; }
        public int? NumberOfParticipants { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? AdditionalInfo { get; set; }
    }
} 