FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ["src/Services/Tasks/TaskStream.Tasks.Domain/TaskStream.Tasks.Domain.csproj", "src/Services/Tasks/TaskStream.Tasks.Domain/"]
COPY ["src/Services/Tasks/TaskStream.Tasks.Application/TaskStream.Tasks.Application.csproj", "src/Services/Tasks/TaskStream.Tasks.Application/"]
COPY ["src/Services/Tasks/TaskStream.Tasks.Infrastructure/TaskStream.Tasks.Infrastructure.csproj", "src/Services/Tasks/TaskStream.Tasks.Infrastructure/"]
COPY ["src/Services/Tasks/TaskStream.Tasks.API/TaskStream.Tasks.API.csproj", "src/Services/Tasks/TaskStream.Tasks.API/"]
COPY ["src/Shared/TaskStream.Shared.Messaging/TaskStream.Shared.Messaging.csproj", "src/Shared/TaskStream.Shared.Messaging/"]
COPY ["src/Shared/TaskStream.Shared.Protos/TaskStream.Shared.Protos.csproj", "src/Shared/TaskStream.Shared.Protos/"]

RUN dotnet restore "src/Services/Tasks/TaskStream.Tasks.API/TaskStream.Tasks.API.csproj"

COPY ["src/Services/Tasks/", "./src/Services/Tasks/"]
COPY ["src/Shared/", "./src/Shared/"]

WORKDIR "/app/src/Services/Tasks/TaskStream.Tasks.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskStream.Tasks.API.dll"]