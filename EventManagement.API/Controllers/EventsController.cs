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
    public class EventsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEvents()
        {
            var events = await _context.Events
                            .Include(e => e.Participants)
                .OrderBy(e => e.Id)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.EventDate,
                    e.Location,
                    e.AdditionalInfo,
                    ParticipantCount = e.Participants.Sum(p => p.Type == ParticipantType.Individual ? 1 : (p.NumberOfParticipants ?? 0)),
                    IsPastEvent = e.EventDate <= DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/Events/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Event>> GetEvent(int id)
        {
            try
            {
                Console.WriteLine($"Attempting to fetch event with ID: {id}");

                var @event = await _context.Events
                    .Include(e => e.Participants)
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.EventDate,
                        e.Location,
                        e.AdditionalInfo,
                        ParticipantCount = e.Participants.Sum(p => p.Type == ParticipantType.Individual ? 1 : (p.NumberOfParticipants ?? 0)),
                        IsPastEvent = e.EventDate <= DateTime.UtcNow,
                        Participants = e.Participants.Select(p => new
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
                            p.AdditionalInfo
                        })
                    })
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (@event == null)
                {
                    Console.WriteLine($"Event with ID {id} not found");
                    return NotFound($"Event with ID {id} not found");
                }

                Console.WriteLine($"Successfully fetched event: {@event.Name}");
                return Ok(@event);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching event: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Events
        [HttpPost]
        public async Task<ActionResult<Event>> CreateEvent(Event @event)
        {
            if (@event.EventDate <= DateTime.UtcNow)
            {
                return BadRequest("Event date must be in the future");
            }

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, @event);
        }

        // PUT: api/Events/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, Event @event)
        {
            if (id != @event.Id)
            {
                return BadRequest();
            }

            if (@event.EventDate <= DateTime.UtcNow)
            {
                return BadRequest("Event date must be in the future");
            }

            var existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null)
            {
                return NotFound();
            }

            // Update the properties of the existing event
            existingEvent.Name = @event.Name;
            existingEvent.Location = @event.Location;
            existingEvent.EventDate = @event.EventDate;
            existingEvent.AdditionalInfo = @event.AdditionalInfo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Events/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            if (@event.EventDate <= DateTime.UtcNow)
            {
                return BadRequest("Cannot delete past events");
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }

        // POST: api/Events/{id}/participants
        [HttpPost("{id}/participants")]
        public async Task<ActionResult<Participant>> AddParticipant(int id, ParticipantDto dto)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound("Event not found");
            }

            if (@event.EventDate <= DateTime.UtcNow)
            {
                return BadRequest("Cannot add participants to past events");
            }

            var participant = new Participant
            {
                EventId = id,
                Type = dto.Type,
                PaymentMethod = dto.PaymentMethod,
                AdditionalInfo = dto.lisainfo
            };

            if (dto.Type == ParticipantType.Individual)
            {
                if (string.IsNullOrWhiteSpace(dto.FirstName) || 
                    string.IsNullOrWhiteSpace(dto.LastName) || 
                    string.IsNullOrWhiteSpace(dto.PersonalCode))
                {
                    return BadRequest("First name, last name, and personal code are required for individual participants");
                }

                if (!IsValidEstonianPersonalCode(dto.PersonalCode))
                {
                    return BadRequest("Invalid Estonian personal code");
                }

                participant.FirstName = dto.FirstName;
                participant.LastName = dto.LastName;
                participant.PersonalCode = dto.PersonalCode;
                participant.CompanyName = string.Empty;
                participant.RegistrationCode = string.Empty;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.Name) || 
                    string.IsNullOrWhiteSpace(dto.ReigstrationCode) || 
                    string.IsNullOrWhiteSpace(dto.participantsnR) ||
                    !int.TryParse(dto.participantsnR, out int numberOfParticipants) ||
                    numberOfParticipants <= 0)
                {
                    return BadRequest("Company name, registration code, and a positive number of participants are required for company participants");
                }

                participant.CompanyName = dto.Name;
                participant.RegistrationCode = dto.ReigstrationCode;
                participant.NumberOfParticipants = numberOfParticipants;
                participant.FirstName = string.Empty;
                participant.LastName = string.Empty;
                participant.PersonalCode = string.Empty;
            }

            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, participant);
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