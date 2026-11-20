using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class ResultsControllerTests
    {
        [Fact]
        public async Task CaptureResult_AsOrganiser_Returns201()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new ResultsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(1, "Organiser")
                }
            };

            // EnrolmentId 5 (Lerato's second enrolment) has no seeded result
            var dto = new ResultDTO { EnrolmentId = 5, FinishTimeSeconds = 2400, Position = 1 };
            var result = await controller.CaptureResult(dto) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result!.StatusCode);
        }

        [Fact]
        public async Task CaptureResult_DuplicateResult_Returns400()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new ResultsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(1, "Organiser")
                }
            };

            // EnrolmentId 1 already has a seeded result
            var dto = new ResultDTO { EnrolmentId = 1, FinishTimeSeconds = 1000, Position = 1 };
            var result = await controller.CaptureResult(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByEvent_ReturnsResults()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new ResultsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(1, "Organiser")
                }
            };

            // Event 1 has 2 seeded results
            var result = await controller.GetByEvent(1) as OkObjectResult;
            Assert.NotNull(result);
            var results = result!.Value as IEnumerable<object>;
            Assert.NotNull(results);
            Assert.Equal(2, results!.Count());
        }

        [Fact]
        public async Task CaptureResult_AsParticipant_Returns403()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new ResultsController(db)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = TestHelper.CreateMockHttpContext(3, "Participant")
                }
            };

            var dto = new ResultDTO { EnrolmentId = 3, FinishTimeSeconds = 5000, Position = 5 };
            var result = await controller.CaptureResult(dto) as ObjectResult;
            Assert.NotNull(result);
            Assert.Equal(403, result!.StatusCode);
        }
    }
}
