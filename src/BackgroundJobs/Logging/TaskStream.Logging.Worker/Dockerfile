FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ["src/BackgroundJobs/Logging/TaskStream.Logging.Worker/TaskStream.Logging.Worker.csproj", "src/BackgroundJobs/Logging/TaskStream.Logging.Worker/"]

RUN dotnet restore "src/BackgroundJobs/Logging/TaskStream.Logging.Worker/TaskStream.Logging.Worker.csproj"

COPY ["src/BackgroundJobs/Logging/TaskStream.Logging.Worker/", "./src/BackgroundJobs/Logging/TaskStream.Logging.Worker/"]

WORKDIR "/app/src/BackgroundJobs/Logging/TaskStream.Logging.Worker"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskStream.Logging.Worker.dll"]