using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagement.API.Data;
using EventManagement.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParticipantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ParticipantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Participants/event/5
        [HttpGet("event/{eventId}")]
        public async Task<ActionResult<IEnumerable<Participant>>> GetEventParticipants(int eventId)
        {
            var participants = await _context.Participants
                .Where(p => p.EventId == eventId)
                .Select(p => new
                {
                    p.Id,
                    p.Type,
                    p.FirstName,
                    p.LastName,
                    p.PersonalCode,
                    p.CompanyName,
                    p.RegistrationCode,
                    p.NumberOfParticipants,
                    p.PaymentMethod,
                    p.AdditionalInfo,
                    DisplayName = p.Type == ParticipantType.Individual 
                        ? $"{p.FirstName} {p.LastName}"
                        : p.CompanyName
                })
                .ToListAsync();

            return Ok(participants);
        }

        // GET: api/Participants/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Participant>> GetParticipant(int id)
        {
            var participant = await _context.Participants
                .Select(p => new
                {
                    p.Id,
                    p.EventId,
                    p.Type,
                    p.FirstName,
                    p.LastName,
                    p.PersonalCode,
                    p.CompanyName,
                    p.RegistrationCode,
                    p.NumberOfParticipants,
                    p.PaymentMethod,
                    p.AdditionalInfo,
                    DisplayName = p.Type == ParticipantType.Individual 
                        ? $"{p.FirstName} {p.LastName}"
                        : p.CompanyName
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (participant == null)
            {
                return NotFound();
            }

            return Ok(participant);
        }

        // POST: api/Participants
        [HttpPost]
        public async Task<ActionResult<Participant>> CreateParticipant(Participant participant)
        {
            var @event = await _context.Events.FindAsync(participant.EventId);
            if (@event == null)
            {
                return BadRequest("Event not found");
            }

            if (@event.EventDate <= DateTime.UtcNow)
            {
                return BadRequest("Cannot add participants to past events");
            }

            if (participant.Type == ParticipantType.Individual)
            {
                if (string.IsNullOrEmpty(participant.FirstName) || 
                    string.IsNullOrEmpty(participant.LastName) || 
                    string.IsNullOrEmpty(participant.PersonalCode))
                {
                    return BadRequest("First name, last name, and personal code are required for individual participants");
                }

                if (!IsValidEstonianPersonalCode(participant.PersonalCode))
                {
                    return BadRequest("Invalid Estonian personal code");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(participant.CompanyName) || 
                    string.IsNullOrEmpty(participant.RegistrationCode) || 
                    !participant.NumberOfParticipants.HasValue)
                {
                    return BadRequest("Company name, registration code, and number of participants are required for company participants");
                }
            }

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParticipant), new { id = participant.Id }, participant);
        }

        // PUT: api/Participants/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateParticipant(int id, UpdateParticipantDto dto)
        {
            var existingParticipant = await _context.Participants
                .Include(p => p.Event)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingParticipant == null)
            {
                return NotFound();
            }

            if (existingParticipant.Event.EventDate <= DateTime.UtcNow)
            {
                return BadRequest("Cannot modify participants for past events");
            }

            // Validate required fields
            if (string.IsNullOrEmpty(dto.FirstName) || 
                string.IsNullOrEmpty(dto.LastName) || 
                string.IsNullOrEmpty(dto.PersonalCode))
            {
                return BadRequest("First name, last name, and personal code are required");
            }

            if (!IsValidEstonianPersonalCode(dto.PersonalCode))
            {
                return BadRequest("Invalid Estonian personal code");
            }

            // Update only the fields that can be changed
            existingParticipant.FirstName = dto.FirstName;
            existingParticipant.LastName = dto.LastName;
            existingParticipant.PersonalCode = dto.PersonalCode;
            existingParticipant.CompanyName = dto.CompanyName;
            existingParticipant.RegistrationCode = dto.RegistrationCode;
            existingParticipant.NumberOfParticipants = dto.NumberOfParticipants;
            existingParticipant.PaymentMethod = dto.PaymentMethod;
            existingParticipant.AdditionalInfo = dto.AdditionalInfo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParticipantExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Participants/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParticipant(int id)
        {
            var participant = await _context.Participants
                .Include(p => p.Event)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (participant == null)
            {
                return NotFound();
            }

            if (participant.Event.EventDate <= DateTime.UtcNow)
            {
                return BadRequest("Cannot delete participants from past events");
            }

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ParticipantExists(int id)
        {
            return _context.Participants.Any(e => e.Id == id);
        }

        private bool IsValidEstonianPersonalCode(string personalCode)
        {
            if (string.IsNullOrEmpty(personalCode) || personalCode.Length != 11)
                return false;

            if (!personalCode.All(char.IsDigit))
                return false;

            int[] weights = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 1 };
            int sum = 0;

            for (int i = 0; i < 10; i++)
            {
                sum += (personalCode[i] - '0') * weights[i];
            }

            int checkDigit = sum % 11;
            if (checkDigit == 10)
            {
                weights = new[] { 3, 4, 5, 6, 7, 8, 9, 1, 2, 3 };
                sum = 0;
                for (int i = 0; i < 10; i++)
                {
                    sum += (personalCode[i] - '0') * weights[i];
                }
                checkDigit = sum % 11;
            }

            return checkDigit == (personalCode[10] - '0');
        }
    }
} 