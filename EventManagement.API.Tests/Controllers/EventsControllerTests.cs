using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventManagement.API.Controllers;
using EventManagement.API.Data;
using EventManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace EventManagement.API.Tests.Controllers
{
    public class EventsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly EventsController _controller;

        public EventsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new EventsController(_context);
        }

        [Fact]
        public async Task GetEvents_ReturnsOkResult()
        {
            // Arrange
            var events = new List<Event>
            {
                new Event 
                { 
                    Id = 1, 
                    Name = "Test Event 1", 
                    Location = "Test Location 1", 
                    EventDate = DateTime.Now.AddDays(1),
                    AdditionalInfo = "Test Info 1"
                },
                new Event 
                { 
                    Id = 2, 
                    Name = "Test Event 2", 
                    Location = "Test Location 2", 
                    EventDate = DateTime.Now.AddDays(2),
                    AdditionalInfo = "Test Info 2"
                }
            };
            await _context.Events.AddRangeAsync(events);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetEvents();

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<IEnumerable<object>>().Subject;
            var eventsList = returnValue.ToList();
            eventsList.Should().HaveCount(2);
            eventsList.Should().Contain(e => 
                e.GetType().GetProperty("Id").GetValue(e).ToString() == "1" &&
                e.GetType().GetProperty("Name").GetValue(e).ToString() == "Test Event 1");
            eventsList.Should().Contain(e => 
                e.GetType().GetProperty("Id").GetValue(e).ToString() == "2" &&
                e.GetType().GetProperty("Name").GetValue(e).ToString() == "Test Event 2");
        }

        [Fact]
        public async Task GetEvent_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var testEvent = new Event 
            { 
                Id = 1, 
                Name = "Test Event", 
                Location = "Test Location", 
                EventDate = DateTime.Now.AddDays(1),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(testEvent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetEvent(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<object>().Subject;
            returnValue.Should().Match<object>(e => 
                e.GetType().GetProperty("Id").GetValue(e).ToString() == "1" &&
                e.GetType().GetProperty("Name").GetValue(e).ToString() == "Test Event");
        }

        [Fact]
        public async Task GetEvent_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetEvent(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateEvent_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var newEvent = new Event
            {
                Name = "New Test Event",
                Location = "New Test Location",
                EventDate = DateTime.UtcNow.AddDays(1),
                AdditionalInfo = "New Test Info"
            };

            // Act
            var result = await _controller.CreateEvent(newEvent);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var returnValue = createdResult.Value.Should().BeAssignableTo<Event>().Subject;
            returnValue.Name.Should().Be("New Test Event");
            returnValue.Location.Should().Be("New Test Location");
            returnValue.Id.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreateEvent_WithPastDate_ReturnsBadRequest()
        {
            // Arrange
            var newEvent = new Event
            {
                Name = "Past Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(-1),
                AdditionalInfo = "Test Info"
            };

            // Act
            var result = await _controller.CreateEvent(newEvent);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateEvent_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var existingEvent = new Event
            {
                Id = 1,
                Name = "Original Event",
                Location = "Original Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Original Info"
            };
            await _context.Events.AddAsync(existingEvent);
            await _context.SaveChangesAsync();

            // Create update data
            var updateData = new Event
            {
                Id = 1,
                Name = "Updated Event",
                Location = "Updated Location",
                EventDate = DateTime.UtcNow.AddDays(3),
                AdditionalInfo = "Updated Info"
            };

            // Act
            var result = await _controller.UpdateEvent(1, updateData);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            
            // Verify the changes in the database
            var eventInDb = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == 1);
            eventInDb.Should().NotBeNull();
            eventInDb.Name.Should().Be("Updated Event");
            eventInDb.Location.Should().Be("Updated Location");
            eventInDb.EventDate.Should().Be(updateData.EventDate);
            eventInDb.AdditionalInfo.Should().Be("Updated Info");
        }

        [Fact]
        public async Task UpdateEvent_WithMismatchedId_ReturnsBadRequest()
        {
            // Arrange
            var updatedEvent = new Event
            {
                Id = 2,
                Name = "Updated Event",
                Location = "Updated Location",
                EventDate = DateTime.UtcNow.AddDays(3),
                AdditionalInfo = "Updated Info"
            };

            // Act
            var result = await _controller.UpdateEvent(1, updatedEvent);

            // Assert
            result.Should().BeOfType<BadRequestResult>();
        }

        [Fact]
        public async Task UpdateEvent_WithPastDate_ReturnsBadRequest()
        {
            // Arrange
            var existingEvent = new Event
            {
                Id = 1,
                Name = "Original Event",
                Location = "Original Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Original Info"
            };
            await _context.Events.AddAsync(existingEvent);
            await _context.SaveChangesAsync();

            var updatedEvent = new Event
            {
                Id = 1,
                Name = "Updated Event",
                Location = "Updated Location",
                EventDate = DateTime.UtcNow.AddDays(-1),
                AdditionalInfo = "Updated Info"
            };

            // Act
            var result = await _controller.UpdateEvent(1, updatedEvent);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteEvent_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var existingEvent = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(existingEvent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteEvent(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            var eventInDb = await _context.Events.FindAsync(1);
            eventInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeleteEvent_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteEvent(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteEvent_WithPastEvent_ReturnsBadRequest()
        {
            // Arrange
            var pastEvent = new Event
            {
                Id = 1,
                Name = "Past Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(-1),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(pastEvent);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteEvent(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Individual_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "37605030299", // Valid Estonian personal code
                PaymentMethod = PaymentMethod.BankTransfer,
                lisainfo = "Additional info"
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var returnValue = createdResult.Value.Should().BeAssignableTo<Participant>().Subject;
            returnValue.FirstName.Should().Be("John");
            returnValue.LastName.Should().Be("Doe");
            returnValue.PersonalCode.Should().Be("37605030299");
            returnValue.Type.Should().Be(ParticipantType.Individual);
        }

        [Fact]
        public async Task AddParticipant_Company_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Company,
                Name = "Test Company",
                ReigstrationCode = "12345678",
                participantsnR = "5",
                PaymentMethod = PaymentMethod.BankTransfer,
                lisainfo = "Additional info"
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var returnValue = createdResult.Value.Should().BeAssignableTo<Participant>().Subject;
            returnValue.CompanyName.Should().Be("Test Company");
            returnValue.RegistrationCode.Should().Be("12345678");
            returnValue.NumberOfParticipants.Should().Be(5);
            returnValue.Type.Should().Be(ParticipantType.Company);
        }

        [Fact]
        public async Task AddParticipant_WithInvalidEventId_ReturnsNotFound()
        {
            // Arrange
            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "12345678901",
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(999, participantDto);

            // Assert
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_WithPastEvent_ReturnsBadRequest()
        {
            // Arrange
            var pastEvent = new Event
            {
                Id = 1,
                Name = "Past Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(-1),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(pastEvent);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "12345678901",
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Individual_WithInvalidPersonalCode_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "123", // Invalid personal code
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Individual_WithMissingRequiredFields_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Individual,
                // Missing required fields
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Company_WithMissingRequiredFields_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Company,
                // Missing required fields
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Company_WithInvalidNumberOfParticipants_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Company,
                Name = "Test Company",
                ReigstrationCode = "12345678",
                participantsnR = "invalid", // Invalid number format
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Individual_WithEmptyStrings_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Individual,
                FirstName = "", // Empty string
                LastName = "", // Empty string
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Company_WithEmptyStrings_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Company,
                Name = "", // Empty string
                ReigstrationCode = "", // Empty string
                participantsnR = "5",
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Individual_WithWhitespaceStrings_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Individual,
                FirstName = "   ", // Whitespace
                LastName = "   ", // Whitespace
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Company_WithWhitespaceStrings_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Company,
                Name = "   ", // Whitespace
                ReigstrationCode = "   ", // Whitespace
                participantsnR = "5",
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Individual_WithNonNumericPersonalCode_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "3760503029A", // Non-numeric character
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Individual_WithWrongLengthPersonalCode_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "3760503029", // Wrong length (10 digits)
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Company_WithZeroParticipants_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Company,
                Name = "Test Company",
                ReigstrationCode = "12345678",
                participantsnR = "0", // Zero participants
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddParticipant_Company_WithNegativeParticipants_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Test Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(2),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participantDto = new ParticipantDto
            {
                Type = ParticipantType.Company,
                Name = "Test Company",
                ReigstrationCode = "12345678",
                participantsnR = "-1", // Negative participants
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.AddParticipant(1, participantDto);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 