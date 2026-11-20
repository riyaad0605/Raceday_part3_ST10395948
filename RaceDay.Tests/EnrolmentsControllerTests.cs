using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentsControllerTests
    {
        [Fact]
        public async Task GetMyEnrolments_ReturnsOwnEnrolments()
        {
            using var db = TestHelper.CreateDbContext();
            // Sipho (UserId 3) has 3 enrolments in seed data
            var controller = new EnrolmentsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(3, "Participant")
                }
            };

            var result = await controller.GetMyEnrolments() as OkObjectResult;
            Assert.NotNull(result);
            var enrolments = result!.Value as IEnumerable<object>;
            Assert.NotNull(enrolments);
            Assert.Equal(3, enrolments!.Count());
        }

        [Fact]
        public async Task Enrol_AsOrganiser_Returns403()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new EnrolmentsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(1, "Organiser")
                }
            };

            var dto = new EnrolmentDTO { EventCategoryId = 1 };
            var result = await controller.Enrol(dto) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(403, result!.StatusCode);
        }

        [Fact]
        public async Task Withdraw_OwnEnrolment_Returns204()
        {
            using var db = TestHelper.CreateDbContext();
            // Lerato (UserId 4) has enrolment 4
            var controller = new EnrolmentsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(4, "Participant")
                }
            };

            var result = await controller.Withdraw(4) as NoContentResult;
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Withdraw_OtherUsersEnrolment_Returns403()
        {
            using var db = TestHelper.CreateDbContext();
            // Lerato (UserId 4) trying to withdraw Sipho's (UserId 3) enrolment 1
            var controller = new EnrolmentsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(4, "Participant")
                }
            };

            var result = await controller.Withdraw(1) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(403, result!.StatusCode);
        }
    }
}
