using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.DTOs;
using RaceDay.Tests.Helpers;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_WithValidData_Returns201()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new AuthController(db)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { Session = new MockSession() } }
            };

            var dto = new RegisterDTO { Email = "new@test.com", Password = "Test123!", FirstName = "Test", LastName = "User", Role = "Participant" };
            var result = await controller.Register(dto) as CreatedAtActionResult;

            Assert.NotNull(result);
            Assert.Equal(201, result!.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_Returns400()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new AuthController(db)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { Session = new MockSession() } }
            };

            var dto = new RegisterDTO { Email = "thabo@raceday.co.za", Password = "Test123!", FirstName = "Dup", LastName = "User", Role = "Organiser" };
            var result = await controller.Register(dto) as BadRequestObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Login_WithValidCredentials_Returns200()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new AuthController(db)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { Session = new MockSession() } }
            };

            var dto = new LoginDTO { Email = "thabo@raceday.co.za", Password = "Pass123!" };
            var result = await controller.Login(dto) as OkObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            using var db = TestHelper.CreateDbContext();
            var controller = new AuthController(db)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { Session = new MockSession() } }
            };

            var dto = new LoginDTO { Email = "thabo@raceday.co.za", Password = "WrongPassword!" };
            var result = await controller.Login(dto) as UnauthorizedObjectResult;

            Assert.NotNull(result);
        }
    }
}
