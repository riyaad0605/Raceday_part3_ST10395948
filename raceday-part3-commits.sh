#!/bin/bash
# RaceDay Part 3 - Staged commit script for 20+ meaningful commits
# Run from the RaceDay_Part3 directory after initializing git

set -e

echo "Starting RaceDay Part 3 commit history..."

# 1 - Initial project setup
git add RaceDay.sln .gitignore .dockerignore
git commit -m "Initialize solution with API, MVC, and test projects

Set up the three-project solution structure for Part 3 of the RaceDay
event management system. Includes gitignore and dockerignore."

# 2 - Entity models
git add RaceDay.API/Models/
git commit -m "Add entity models with Blob Storage fields

Create User, Event, Category, EventCategory, Enrolment, and Result
entities. Add ProfileImageUrl to User and BannerImageUrl to Event
for Azure Blob Storage integration in Part 3."

# 3 - DTOs
git add RaceDay.API/DTOs/
git commit -m "Add data transfer objects for API endpoints

Define request and response DTOs for auth, events, categories,
event categories, enrolments, and results. Include new image URL
fields in response DTOs."

# 4 - DbContext and seed data
git add RaceDay.API/Data/
git commit -m "Configure EF Core DbContext with seed data

Set up RaceDayDbContext with fluent API configuration, unique
indexes, and cascade delete rules. Seed 4 users, 3 events,
4 categories, 7 event-category links, 5 enrolments, and 2 results."

# 5 - Blob Storage service
git add RaceDay.API/Services/
git commit -m "Implement Azure Blob Storage service

Add IBlobStorageService interface and BlobStorageService implementation
using Azure.Storage.Blobs. Supports file upload with GUID-based naming
and file deletion from specified containers."

# 6 - Auth controller
git add RaceDay.API/Controllers/AuthController.cs
git commit -m "Add authentication controller with session management

Implement register, login, and logout endpoints. Use BCrypt for
password hashing and ASP.NET Core session for auth state. Validate
roles and prevent duplicate email registration."

# 7 - Users controller with profile image upload
git add RaceDay.API/Controllers/UsersController.cs
git commit -m "Add user profile management with image upload

Implement GET and PUT for profile details, plus POST endpoint for
profile picture upload to Azure Blob Storage. Validates image types
and replaces old images on re-upload."

# 8 - Events controller with banner upload
git add RaceDay.API/Controllers/EventsController.cs
git commit -m "Add events CRUD with banner image upload

Full CRUD for events with Organiser-only access control and ownership
validation. Include banner image upload endpoint using Blob Storage.
Participants see only published events. Delete cleans up blob images."

# 9 - Categories controller
git add RaceDay.API/Controllers/CategoriesController.cs
git commit -m "Add categories CRUD controller

Implement CRUD for race distance categories with Organiser-only
write access. Categories represent distances like 5km, 10km, etc."

# 10 - Event categories controller
git add RaceDay.API/Controllers/EventCategoriesController.cs
git commit -m "Add event-category linking with enrolment counts

Allow Organisers to link categories to events with max participant
limits. Include current enrolment counts in response for capacity
tracking."

# 11 - Enrolments controller
git add RaceDay.API/Controllers/EnrolmentsController.cs
git commit -m "Add enrolment management for participants

Implement enrol, withdraw, and listing endpoints. Validate event
status, check capacity limits, and prevent duplicate enrolments.
Participants can only manage their own enrolments."

# 12 - Results controller
git add RaceDay.API/Controllers/ResultsController.cs
git commit -m "Add race results capture and viewing

Organisers can capture finish times and positions for enrolled
participants. Prevent duplicate results. Include My Results endpoint
for participants to view their race history."

# 13 - API Program.cs and config
git add RaceDay.API/Program.cs RaceDay.API/appsettings.json RaceDay.API/appsettings.Development.json RaceDay.API/RaceDay.API.csproj
git commit -m "Configure API startup with Blob Storage and CORS

Register EF Core, Azure BlobServiceClient, session, Swagger, and
CORS for MVC frontend. Auto-create database on startup. Configure
connection strings for SQL Server and Azure Blob Storage."

# 14 - MVC project setup
git add RaceDay.MVC/RaceDay.MVC.csproj RaceDay.MVC/Program.cs RaceDay.MVC/appsettings.json
git commit -m "Set up MVC project with HttpClient for API consumption

Configure MVC app with session support and typed HttpClient
pointing to the RaceDay API. No direct database access in MVC -
all data flows through the API service layer."

# 15 - MVC view models
git add RaceDay.MVC/Models/
git commit -m "Add MVC view models for all features

Define view models for auth, events, categories, enrolments,
results, profile, and dashboard. Include formatted time display
for race results."

# 16 - API service (HttpClient wrapper)
git add RaceDay.MVC/Services/
git commit -m "Implement ApiService as central API client

Create typed HttpClient wrapper that forwards session cookies to
the API for authentication. All MVC controllers use this service
instead of direct database access."

# 17 - MVC controllers
git add RaceDay.MVC/Controllers/
git commit -m "Add MVC controllers with role-based access

Implement Auth, Home, Events, Categories, Enrolments, Results,
and Profile controllers. Each enforces role-based access and
delegates all data operations to the ApiService."

# 18 - Layout and shared views
git add RaceDay.MVC/Views/_ViewImports.cshtml RaceDay.MVC/Views/_ViewStart.cshtml RaceDay.MVC/Views/Shared/
git commit -m "Create shared layout with role-based navigation

Build responsive navbar with Bootstrap that shows different menu
items for Organisers and Participants. Include toast notifications
for success and error messages."

# 19 - Auth views
git add RaceDay.MVC/Views/Auth/
git commit -m "Add login and registration pages with RaceDay branding

Design auth pages with gradient headers, demo account display,
and form validation. Include role selection on registration."

# 20 - Participant views (home, events detail, my events, my results)
git add RaceDay.MVC/Views/Home/Index.cshtml RaceDay.MVC/Views/Events/Details.cshtml RaceDay.MVC/Views/Enrolments/MyEvents.cshtml RaceDay.MVC/Views/Results/MyResults.cshtml
git commit -m "Build participant interface with event browsing and enrolment

Create event browsing with search and filter, event detail page with
category enrolment, My Events page with status badges, and My Results
page with formatted times and positions."

# 21 - Organiser views (dashboard, event CRUD, categories, enrolments, results)
git add RaceDay.MVC/Views/Home/Dashboard.cshtml RaceDay.MVC/Views/Events/Index.cshtml RaceDay.MVC/Views/Events/Create.cshtml RaceDay.MVC/Views/Events/Edit.cshtml RaceDay.MVC/Views/Categories/ RaceDay.MVC/Views/Enrolments/EventEnrolments.cshtml RaceDay.MVC/Views/Results/EventResults.cshtml
git commit -m "Build organiser interface with dashboard and management views

Create dashboard with stats cards and upcoming events, event CRUD
with banner image upload, category management, enrolment viewer,
and results capture form with medal-style position badges."

# 22 - Profile view
git add RaceDay.MVC/Views/Profile/
git commit -m "Add profile page with image upload

Design profile page showing current picture, upload form, and
editable name fields. Profile image stored via Azure Blob Storage
through the API."

# 23 - Error view
git add RaceDay.MVC/Views/Home/Error.cshtml
git commit -m "Add error handling page

Create user-friendly error page with navigation back to home."

# 24 - Custom CSS and branding
git add RaceDay.MVC/wwwroot/
git commit -m "Design RaceDay brand theme with custom CSS

Create sporting/energetic visual theme with red and navy colour
scheme. Style cards, buttons, badges, stat cards, hero sections,
and status indicators. Green for Confirmed, amber for Pending."

# 25 - Unit tests
git add RaceDay.Tests/
git commit -m "Add comprehensive unit tests for API controllers

Test auth (register, login, duplicate email), events (CRUD, role
access, ownership), enrolments (enrol, withdraw, role checks),
and results (capture, duplicate prevention, role access). Use
InMemory EF Core and mock blob service."

# 26 - Dockerfile
git add Dockerfile
git commit -m "Add multi-stage Dockerfile for containerization

Build both API and MVC projects in SDK stage, run with supervisord
in runtime stage to serve both services. Expose ports 5000 and 5001."

# 27 - CI/CD pipeline
git add .github/
git commit -m "Configure GitHub Actions CI/CD pipeline

Build both projects, run unit tests, and build Docker image on
push and pull request to main branch."

# 28 - README
git add README.md
git commit -m "Add comprehensive README with Docker instructions

Document architecture, features, setup instructions, demo accounts,
Docker commands, API endpoints, and project structure. Include
test running instructions and CI/CD description."

echo ""
echo "All 28 commits created successfully!"
echo "Run 'git log --oneline' to verify."
echo ""
echo "To push to GitHub:"
echo "  git remote add origin https://github.com/YOUR_USERNAME/RaceDay.git"
echo "  git push -u origin main"
