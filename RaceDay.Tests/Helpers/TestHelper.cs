using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using RaceDay.API.Data;
using RaceDay.API.Services;

namespace RaceDay.Tests.Helpers
{
    /// <summary>
    /// Shared test helpers: in-memory DbContext, mock session, mock blob service.
    /// </summary>
    public static class TestHelper
    {
        /// <summary>Create a fresh in-memory DbContext with seed data applied.</summary>
        public static RaceDayDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        /// <summary>Create a mock HttpContext with session values set for a specific user.</summary>
        public static HttpContext CreateMockHttpContext(int userId, string role, string email = "test@test.com")
        {
            var session = new MockSession();
            session.SetInt32("UserId", userId);
            session.SetString("Role", role);
            session.SetString("Email", email);

            var context = new DefaultHttpContext { Session = session };
            return context;
        }

        /// <summary>Create a mock blob storage service that returns predictable URLs.</summary>
        public static IBlobStorageService CreateMockBlobService()
        {
            var mock = new Mock<IBlobStorageService>();
            mock.Setup(b => b.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("https://teststorage.blob.core.windows.net/test/image.jpg");
            mock.Setup(b => b.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            return mock.Object;
        }
    }

    /// <summary>Minimal in-memory ISession implementation for unit testing.</summary>
    public class MockSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public string Id => Guid.NewGuid().ToString();
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[]? value) => _store.TryGetValue(key, out value);
    }
}
