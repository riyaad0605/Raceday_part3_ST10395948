# ── Build Stage ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for better layer caching
COPY RaceDay.sln .
COPY RaceDay.API/RaceDay.API.csproj RaceDay.API/
COPY RaceDay.MVC/RaceDay.MVC.csproj RaceDay.MVC/
COPY RaceDay.Tests/RaceDay.Tests.csproj RaceDay.Tests/

RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish RaceDay.API/RaceDay.API.csproj -c Release -o /app/api
RUN dotnet publish RaceDay.MVC/RaceDay.MVC.csproj -c Release -o /app/mvc

# ── Runtime Stage ──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install supervisord to run both services
RUN apt-get update && apt-get install -y supervisor && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/api /app/api
COPY --from=build /app/mvc /app/mvc

# Supervisord configuration to run both API and MVC
RUN echo '[supervisord]\n\
nodaemon=true\n\
\n\
[program:api]\n\
command=dotnet /app/api/RaceDay.API.dll\n\
environment=ASPNETCORE_URLS="http://+:5000",ASPNETCORE_ENVIRONMENT="Development"\n\
stdout_logfile=/dev/stdout\n\
stdout_logfile_maxbytes=0\n\
stderr_logfile=/dev/stderr\n\
stderr_logfile_maxbytes=0\n\
\n\
[program:mvc]\n\
command=dotnet /app/mvc/RaceDay.MVC.dll\n\
environment=ASPNETCORE_URLS="http://+:5001",ASPNETCORE_ENVIRONMENT="Development",ApiSettings__BaseUrl="http://localhost:5000"\n\
stdout_logfile=/dev/stdout\n\
stdout_logfile_maxbytes=0\n\
stderr_logfile=/dev/stderr\n\
stderr_logfile_maxbytes=0' > /etc/supervisor/conf.d/raceday.conf

EXPOSE 5000 5001

CMD ["supervisord", "-c", "/etc/supervisor/conf.d/raceday.conf"]
