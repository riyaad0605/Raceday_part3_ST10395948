using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RaceDay.MVC.Models;

namespace RaceDay.MVC.Services
{
    /// <summary>
    /// Central service for all REST API calls. The MVC layer never touches the database directly;
    /// every data operation goes through this service to the RaceDay API.
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contextAccessor;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, IHttpContextAccessor contextAccessor)
        {
            _http = http;
            _contextAccessor = contextAccessor;
        }

        // Forward the MVC session cookie to the API so the API recognises the logged-in user.
        private void AttachSessionCookie()
        {
            var cookies = _contextAccessor.HttpContext?.Request.Headers["Cookie"].ToString();
            if (!string.IsNullOrEmpty(cookies))
                _http.DefaultRequestHeaders.Remove("Cookie");
        }

        // ── Auth ───────────────────────────────────────────────────
        public async Task<(bool Success, string Message, UserViewModel? User)> LoginAsync(LoginViewModel model)
        {
            var response = await PostAsync<UserViewModel>("api/auth/login", model);
            return response;
        }

        public async Task<(bool Success, string Message, UserViewModel? User)> RegisterAsync(RegisterViewModel model)
        {
            var response = await PostAsync<UserViewModel>("api/auth/register", model);
            return response;
        }

        public async Task LogoutAsync() => await PostAsync<object>("api/auth/logout", new { });

        // ── Users / Profile ────────────────────────────────────────
        public async Task<UserViewModel?> GetProfileAsync() => await GetAsync<UserViewModel>("api/users/profile");

        public async Task<(bool Success, string Message, UserViewModel? Data)> UpdateProfileAsync(UpdateProfileViewModel model)
            => await PutAsync<UserViewModel>("api/users/profile", model);

        public async Task<(bool Success, string Message)> UploadProfileImageAsync(Stream fileStream, string fileName, string contentType)
            => await UploadFileAsync("api/users/profile/image", fileStream, fileName, contentType);

        // ── Events ─────────────────────────────────────────────────
        public async Task<List<EventViewModel>> GetEventsAsync() => await GetAsync<List<EventViewModel>>("api/events") ?? new();
        public async Task<EventViewModel?> GetEventAsync(int id) => await GetAsync<EventViewModel>($"api/events/{id}");

        public async Task<(bool Success, string Message, EventViewModel? Data)> CreateEventAsync(CreateEventViewModel model)
            => await PostAsync<EventViewModel>("api/events", model);

        public async Task<(bool Success, string Message, EventViewModel? Data)> UpdateEventAsync(int id, CreateEventViewModel model)
            => await PutAsync<EventViewModel>($"api/events/{id}", model);

        public async Task<(bool Success, string Message)> DeleteEventAsync(int id) => await DeleteAsync($"api/events/{id}");

        public async Task<(bool Success, string Message)> UploadBannerAsync(int eventId, Stream fileStream, string fileName, string contentType)
            => await UploadFileAsync($"api/events/{eventId}/banner", fileStream, fileName, contentType);

        // ── Categories ─────────────────────────────────────────────
        public async Task<List<CategoryViewModel>> GetCategoriesAsync() => await GetAsync<List<CategoryViewModel>>("api/categories") ?? new();
        public async Task<CategoryViewModel?> GetCategoryAsync(int id) => await GetAsync<CategoryViewModel>($"api/categories/{id}");

        public async Task<(bool Success, string Message, CategoryViewModel? Data)> CreateCategoryAsync(CreateCategoryViewModel model)
            => await PostAsync<CategoryViewModel>("api/categories", model);

        public async Task<(bool Success, string Message, CategoryViewModel? Data)> UpdateCategoryAsync(int id, CreateCategoryViewModel model)
            => await PutAsync<CategoryViewModel>($"api/categories/{id}", model);

        public async Task<(bool Success, string Message)> DeleteCategoryAsync(int id) => await DeleteAsync($"api/categories/{id}");

        // ── Event Categories ───────────────────────────────────────
        public async Task<List<EventCategoryViewModel>> GetEventCategoriesAsync(int eventId)
            => await GetAsync<List<EventCategoryViewModel>>($"api/eventcategories/event/{eventId}") ?? new();

        public async Task<(bool Success, string Message, EventCategoryViewModel? Data)> LinkCategoryAsync(LinkCategoryViewModel model)
            => await PostAsync<EventCategoryViewModel>("api/eventcategories", model);

        public async Task<(bool Success, string Message)> UnlinkCategoryAsync(int id) => await DeleteAsync($"api/eventcategories/{id}");

        // ── Enrolments ─────────────────────────────────────────────
        public async Task<List<EnrolmentViewModel>> GetMyEnrolmentsAsync()
            => await GetAsync<List<EnrolmentViewModel>>("api/enrolments/my") ?? new();

        public async Task<List<EnrolmentViewModel>> GetEventEnrolmentsAsync(int eventId)
            => await GetAsync<List<EnrolmentViewModel>>($"api/enrolments/event/{eventId}") ?? new();

        public async Task<(bool Success, string Message, EnrolmentViewModel? Data)> EnrolAsync(int eventCategoryId)
            => await PostAsync<EnrolmentViewModel>("api/enrolments", new { eventCategoryId });

        public async Task<(bool Success, string Message)> WithdrawAsync(int enrolmentId)
            => await DeleteAsync($"api/enrolments/{enrolmentId}");

        // ── Results ────────────────────────────────────────────────
        public async Task<List<ResultViewModel>> GetEventResultsAsync(int eventId)
            => await GetAsync<List<ResultViewModel>>($"api/results/event/{eventId}") ?? new();

        public async Task<List<ResultViewModel>> GetMyResultsAsync()
            => await GetAsync<List<ResultViewModel>>("api/results/my") ?? new();

        public async Task<(bool Success, string Message, ResultViewModel? Data)> CaptureResultAsync(CaptureResultViewModel model)
            => await PostAsync<ResultViewModel>("api/results", model);

        public async Task<(bool Success, string Message, ResultViewModel? Data)> UpdateResultAsync(int id, CaptureResultViewModel model)
            => await PutAsync<ResultViewModel>($"api/results/{id}", model);

        // ── Generic HTTP helpers ───────────────────────────────────
        private async Task<T?> GetAsync<T>(string url)
        {
            ForwardCookies();
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return default;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        private async Task<(bool Success, string Message, T? Data)> PostAsync<T>(string url, object body)
        {
            ForwardCookies();
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                // Capture session cookie from API response
                CaptureApiCookies(response);
                return (true, "Success", data);
            }

            var error = TryGetMessage(json);
            return (false, error, default);
        }

        private async Task<(bool Success, string Message, T? Data)> PutAsync<T>(string url, object body)
        {
            ForwardCookies();
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _http.PutAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<T>(json, _jsonOptions);
                return (true, "Success", data);
            }

            return (false, TryGetMessage(json), default);
        }

        private async Task<(bool Success, string Message)> DeleteAsync(string url)
        {
            ForwardCookies();
            var response = await _http.DeleteAsync(url);
            if (response.IsSuccessStatusCode) return (true, "Success");
            var json = await response.Content.ReadAsStringAsync();
            return (false, TryGetMessage(json));
        }

        private async Task<(bool Success, string Message)> UploadFileAsync(string url, Stream fileStream, string fileName, string contentType)
        {
            ForwardCookies();
            using var formContent = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            formContent.Add(streamContent, "file", fileName);

            var response = await _http.PostAsync(url, formContent);
            if (response.IsSuccessStatusCode) return (true, "Success");
            var json = await response.Content.ReadAsStringAsync();
            return (false, TryGetMessage(json));
        }

        private void ForwardCookies()
        {
            var context = _contextAccessor.HttpContext;
            if (context == null) return;

            // Forward the API session cookie stored in MVC session
            var apiCookie = context.Session.GetString("ApiSessionCookie");
            if (!string.IsNullOrEmpty(apiCookie))
            {
                _http.DefaultRequestHeaders.Remove("Cookie");
                _http.DefaultRequestHeaders.Add("Cookie", apiCookie);
            }
        }

        private void CaptureApiCookies(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var cookie = cookies.FirstOrDefault(c => c.Contains(".AspNetCore.Session"));
                if (cookie != null)
                {
                    var value = cookie.Split(';')[0]; // just the key=value part
                    _contextAccessor.HttpContext?.Session.SetString("ApiSessionCookie", value);
                }
            }
        }

        private static string TryGetMessage(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? "An error occurred.";
            }
            catch { }
            return "An unexpected error occurred.";
        }
    }
}
