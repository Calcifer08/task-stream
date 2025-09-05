FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ["src/Services/Auth/TaskStream.Auth.Domain/TaskStream.Auth.Domain.csproj", "src/Services/Auth/TaskStream.Auth.Domain/"]
COPY ["src/Services/Auth/TaskStream.Auth.Application/TaskStream.Auth.Application.csproj", "src/Services/Auth/TaskStream.Auth.Application/"]
COPY ["src/Services/Auth/TaskStream.Auth.Infrastructure/TaskStream.Auth.Infrastructure.csproj", "src/Services/Auth/TaskStream.Auth.Infrastructure/"]
COPY ["src/Services/Auth/TaskStream.Auth.API/TaskStream.Auth.API.csproj", "src/Services/Auth/TaskStream.Auth.API/"]
COPY ["src/Shared/TaskStream.Shared.Messaging/TaskStream.Shared.Messaging.csproj", "src/Shared/TaskStream.Shared.Messaging/"]
COPY ["src/Shared/TaskStream.Shared.Protos/TaskStream.Shared.Protos.csproj", "src/Shared/TaskStream.Shared.Protos/"]

RUN dotnet restore "src/Services/Auth/TaskStream.Auth.API/TaskStream.Auth.API.csproj"

COPY ["src/Services/Auth/", "./src/Services/Auth/"]
COPY ["src/Shared/", "./src/Shared/"]

WORKDIR "/app/src/Services/Auth/TaskStream.Auth.API"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskStream.Auth.API.dll"]
