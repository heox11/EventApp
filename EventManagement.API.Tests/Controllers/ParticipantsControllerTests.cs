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
    public class ParticipantsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ParticipantsController _controller;

        public ParticipantsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new ParticipantsController(_context);
        }

        [Fact]
        public async Task GetEventParticipants_ReturnsOkResult()
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

            var participants = new List<Participant>
            {
                new Participant
                {
                    Id = 1,
                    EventId = 1,
                    Type = ParticipantType.Individual,
                    FirstName = "John",
                    LastName = "Doe",
                    PersonalCode = "37605030299",
                    PaymentMethod = PaymentMethod.BankTransfer
                },
                new Participant
                {
                    Id = 2,
                    EventId = 1,
                    Type = ParticipantType.Company,
                    CompanyName = "Test Company",
                    RegistrationCode = "12345678",
                    NumberOfParticipants = 5,
                    PaymentMethod = PaymentMethod.BankTransfer
                }
            };
            await _context.Participants.AddRangeAsync(participants);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetEventParticipants(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<IEnumerable<object>>().Subject;
            var participantsList = returnValue.ToList();
            participantsList.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetEventParticipants_WithInvalidEventId_ReturnsEmptyList()
        {
            // Act
            var result = await _controller.GetEventParticipants(999);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<IEnumerable<object>>().Subject;
            var participantsList = returnValue.ToList();
            participantsList.Should().BeEmpty();
        }

        [Fact]
        public async Task GetParticipant_WithValidId_ReturnsOkResult()
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

            var participant = new Participant
            {
                Id = 1,
                EventId = 1,
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };
            await _context.Participants.AddAsync(participant);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetParticipant(1);

            // Assert
            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var returnValue = okResult.Value.Should().BeAssignableTo<object>().Subject;
            returnValue.Should().Match<object>(p => 
                p.GetType().GetProperty("Id").GetValue(p).ToString() == "1" &&
                p.GetType().GetProperty("FirstName").GetValue(p).ToString() == "John");
        }

        [Fact]
        public async Task GetParticipant_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetParticipant(999);

            // Assert
            result.Result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreateParticipant_Individual_WithValidData_ReturnsCreatedResult()
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

            var participant = new Participant
            {
                EventId = 1,
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.CreateParticipant(participant);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var returnValue = createdResult.Value.Should().BeAssignableTo<Participant>().Subject;
            returnValue.FirstName.Should().Be("John");
            returnValue.LastName.Should().Be("Doe");
            returnValue.PersonalCode.Should().Be("37605030299");
        }

        [Fact]
        public async Task CreateParticipant_Company_WithValidData_ReturnsCreatedResult()
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

            var participant = new Participant
            {
                EventId = 1,
                Type = ParticipantType.Company,
                CompanyName = "Test Company",
                RegistrationCode = "12345678",
                NumberOfParticipants = 5,
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.CreateParticipant(participant);

            // Assert
            var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var returnValue = createdResult.Value.Should().BeAssignableTo<Participant>().Subject;
            returnValue.CompanyName.Should().Be("Test Company");
            returnValue.RegistrationCode.Should().Be("12345678");
            returnValue.NumberOfParticipants.Should().Be(5);
        }

        [Fact]
        public async Task CreateParticipant_WithInvalidEventId_ReturnsBadRequest()
        {
            // Arrange
            var participant = new Participant
            {
                EventId = 999,
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.CreateParticipant(participant);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateParticipant_WithPastEvent_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Past Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(-1),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);
            await _context.SaveChangesAsync();

            var participant = new Participant
            {
                EventId = 1,
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };

            // Act
            var result = await _controller.CreateParticipant(participant);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task UpdateParticipant_WithValidData_ReturnsNoContent()
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

            var participant = new Participant
            {
                Id = 1,
                EventId = 1,
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };
            await _context.Participants.AddAsync(participant);
            await _context.SaveChangesAsync();

            var updateDto = new UpdateParticipantDto
            {
                FirstName = "Jane",
                LastName = "Smith",
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.Cash
            };

            // Act
            var result = await _controller.UpdateParticipant(1, updateDto);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            var updatedParticipant = await _context.Participants.FindAsync(1);
            updatedParticipant.FirstName.Should().Be("Jane");
            updatedParticipant.LastName.Should().Be("Smith");
            updatedParticipant.PaymentMethod.Should().Be(PaymentMethod.Cash);
        }

        [Fact]
        public async Task UpdateParticipant_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.UpdateParticipant(999, new UpdateParticipantDto());

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UpdateParticipant_WithPastEvent_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Past Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(-1),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);

            var participant = new Participant
            {
                Id = 1,
                EventId = 1,
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };
            await _context.Participants.AddAsync(participant);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.UpdateParticipant(1, new UpdateParticipantDto());

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteParticipant_WithValidId_ReturnsNoContent()
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

            var participant = new Participant
            {
                Id = 1,
                EventId = 1,
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };
            await _context.Participants.AddAsync(participant);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteParticipant(1);

            // Assert
            result.Should().BeOfType<NoContentResult>();
            var participantInDb = await _context.Participants.FindAsync(1);
            participantInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeleteParticipant_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteParticipant(999);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteParticipant_WithPastEvent_ReturnsBadRequest()
        {
            // Arrange
            var @event = new Event
            {
                Id = 1,
                Name = "Past Event",
                Location = "Test Location",
                EventDate = DateTime.UtcNow.AddDays(-1),
                AdditionalInfo = "Test Info"
            };
            await _context.Events.AddAsync(@event);

            var participant = new Participant
            {
                Id = 1,
                EventId = 1,
                Type = ParticipantType.Individual,
                FirstName = "John",
                LastName = "Doe",
                PersonalCode = "37605030299",
                PaymentMethod = PaymentMethod.BankTransfer
            };
            await _context.Participants.AddAsync(participant);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteParticipant(1);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
} 