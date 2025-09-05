FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ["src/Gateways/TaskStream.ApiGateway/TaskStream.ApiGateway.csproj", "src/Gateways/TaskStream.ApiGateway/"]
COPY ["src/Shared/TaskStream.Shared.Protos/TaskStream.Shared.Protos.csproj", "src/Shared/TaskStream.Shared.Protos/"]

RUN dotnet restore "src/Gateways/TaskStream.ApiGateway/TaskStream.ApiGateway.csproj"

COPY ["src/Gateways/TaskStream.ApiGateway/", "./src/Gateways/TaskStream.ApiGateway/"]
COPY ["src/Shared/TaskStream.Shared.Protos/", "./src/Shared/TaskStream.Shared.Protos/"]

WORKDIR "/app/src/Gateways/TaskStream.ApiGateway"
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TaskStream.ApiGateway.dll"]