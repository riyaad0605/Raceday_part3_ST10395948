using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class EventsControllerTests
    {
        [Fact]
        public async Task GetAll_AsOrganiser_ReturnsAllEvents()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new EventsController(db, TestHelper.CreateMockBlobService())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(1, "Organiser", "thabo@raceday.co.za")
                }
            };

            var result = await controller.GetAll() as OkObjectResult;
            Assert.NotNull(result);
            var events = result!.Value as IEnumerable<object>;
            Assert.NotNull(events);
        }

        [Fact]
        public async Task Create_AsOrganiser_Returns201()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new EventsController(db, TestHelper.CreateMockBlobService())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(1, "Organiser", "thabo@raceday.co.za")
                }
            };

            var dto = new EventDTO
            {
                Name = "New Test Event", Description = "A test", EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town", EventType = "Run", Status = "Draft"
            };

            var result = await controller.Create(dto) as CreatedAtActionResult;
            Assert.NotNull(result);
            Assert.Equal(201, result!.StatusCode);
        }

        [Fact]
        public async Task Create_AsParticipant_Returns403()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new EventsController(db, TestHelper.CreateMockBlobService())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(3, "Participant", "sipho@email.co.za")
                }
            };

            var dto = new EventDTO
            {
                Name = "Blocked", Description = "Should fail", EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Joburg", EventType = "Run", Status = "Draft"
            };

            var result = await controller.Create(dto) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(403, result!.StatusCode);
        }

        [Fact]
        public async Task GetById_ExistingEvent_ReturnsEvent()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new EventsController(db, TestHelper.CreateMockBlobService())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(1, "Organiser")
                }
            };

            var result = await controller.GetById(1) as OkObjectResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Delete_OtherOrganisersEvent_Returns403()
        {
            using var db = TestHelper.CreateDbContext();
            // User 2 (Naledi) trying to delete User 1's (Thabo) event
            var controller = new EventsController(db, TestHelper.CreateMockBlobService())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(2, "Organiser")
                }
            };

            var result = await controller.Delete(1) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(403, result!.StatusCode);
        }
    }
}
