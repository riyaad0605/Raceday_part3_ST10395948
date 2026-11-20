# RaceDay - Event Management System

A full-stack ASP.NET Core application for managing running, walking, and cycling events. Built as a Portfolio of Evidence (POE) for PROG6212.

## Architecture

The system follows a clean separation between the API and presentation layers:

- **RaceDay.API** — RESTful Web API built with ASP.NET Core 8, Entity Framework Core, and SQL Server. Handles authentication (session-based with BCrypt), CRUD operations, role-based access control, and Azure Blob Storage integration for image uploads.
- **RaceDay.MVC** — ASP.NET Core MVC web application that consumes the API via HttpClient. Contains no direct database access or business logic. Provides distinct interfaces for Organisers and Participants.
- **RaceDay.Tests** — xUnit test project with unit tests covering authentication, events, enrolments, and results controllers.

## Features

### Organiser Interface
- Dashboard with event statistics and upcoming events
- Full CRUD for events with banner image upload (Azure Blob Storage)
- Category management (distance categories like 5km, 10km, 21km, 42km)
- View participant enrolments per event
- Capture and manage race results with positions and finish times

### Participant Interface
- Browse and filter published events by name, location, and type
- View event details with available categories and capacity
- Enrol in event categories with real-time spot availability
- View enrolled events with colour-coded statuses (green = Confirmed, amber = Pending)
- View personal race results with formatted finish times and positions
- Profile management with profile picture upload

### Technical Features
- Session-based authentication with BCrypt password hashing
- Role-based navigation (Organiser vs Participant)
- Azure Blob Storage for event banners and profile pictures
- Responsive UI with Bootstrap 5 and custom RaceDay branding
- Comprehensive error handling and user feedback
- Docker containerization

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express / LocalDB)
- [Docker](https://www.docker.com/get-started) (for containerized deployment)
- Azure Storage Account (for blob storage) or Azurite (local emulator)

## Running Locally

1. Update the connection string in `RaceDay.API/appsettings.json` to point to your SQL Server instance.
2. Update the Azure Blob Storage connection string (use `UseDevelopmentStorage=true` for Azurite).

```bash
# Restore and build
dotnet restore
dotnet build

# Run the API (terminal 1)
cd RaceDay.API
dotnet run --urls "http://localhost:5000"

# Run the MVC app (terminal 2)
cd RaceDay.MVC
dotnet run --urls "http://localhost:5001"
```

3. Open `http://localhost:5001` in your browser.

### Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Organiser | thabo@raceday.co.za | Pass123! |
| Organiser | naledi@raceday.co.za | Pass123! |
| Participant | sipho@email.co.za | Pass123! |
| Participant | lerato@email.co.za | Pass123! |

## Docker

Build and run the application with a single command:

```bash
docker build -t raceday-app .
docker run -p 5000:5000 -p 5001:5001 raceday-app
```

Access the MVC app at `http://localhost:5001` and the API (Swagger) at `http://localhost:5000/swagger`.

**Note:** The Docker container uses an in-memory database by default. For persistent storage, mount a SQL Server connection string as an environment variable.

## Running Tests

```bash
dotnet test RaceDay.Tests/RaceDay.Tests.csproj --verbosity normal
```

## CI/CD

The project includes a GitHub Actions workflow (`.github/workflows/ci.yml`) that:
1. Restores dependencies
2. Builds both API and MVC projects
3. Runs all unit tests
4. Builds the Docker image

## Project Structure

```
RaceDay/
├── RaceDay.API/              # REST API
│   ├── Controllers/          # 7 API controllers
│   ├── Data/                 # DbContext with seed data
│   ├── DTOs/                 # Data transfer objects
│   ├── Models/               # 6 entity models
│   └── Services/             # Azure Blob Storage service
├── RaceDay.MVC/              # MVC Frontend
│   ├── Controllers/          # 6 MVC controllers
│   ├── Models/               # View models
│   ├── Services/             # ApiService (HttpClient wrapper)
│   ├── Views/                # Razor views with Bootstrap UI
│   └── wwwroot/              # Static files (CSS)
├── RaceDay.Tests/            # Unit tests
│   ├── Helpers/              # Test utilities
│   └── *Tests.cs             # Test classes
├── Dockerfile                # Multi-stage Docker build
├── .github/workflows/ci.yml  # CI/CD pipeline
└── README.md
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/auth/register | Register a new user |
| POST | /api/auth/login | Log in |
| POST | /api/auth/logout | Log out |
| GET | /api/users/profile | Get current user profile |
| PUT | /api/users/profile | Update profile |
| POST | /api/users/profile/image | Upload profile picture |
| GET | /api/events | List events |
| GET | /api/events/{id} | Get event details |
| POST | /api/events | Create event |
| PUT | /api/events/{id} | Update event |
| DELETE | /api/events/{id} | Delete event |
| POST | /api/events/{id}/banner | Upload event banner |
| GET/POST/PUT/DELETE | /api/categories/* | Category CRUD |
| GET/POST/DELETE | /api/eventcategories/* | Link categories to events |
| POST | /api/enrolments | Enrol in category |
| GET | /api/enrolments/my | My enrolments |
| GET | /api/enrolments/event/{id} | Event enrolments |
| DELETE | /api/enrolments/{id} | Withdraw |
| POST | /api/results | Capture result |
| GET | /api/results/event/{id} | Event results |
| GET | /api/results/my | My results |
| PUT | /api/results/{id} | Update result |

## Video Presentation

[Link to video presentation]

## Author

Riyaad — PROG6212 Portfolio of Evidence
