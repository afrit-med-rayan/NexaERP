FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NexaERP.API/NexaERP.API.csproj", "src/NexaERP.API/"]
COPY ["src/NexaERP.Application/NexaERP.Application.csproj", "src/NexaERP.Application/"]
COPY ["src/NexaERP.Domain/NexaERP.Domain.csproj", "src/NexaERP.Domain/"]
COPY ["src/NexaERP.Infrastructure/NexaERP.Infrastructure.csproj", "src/NexaERP.Infrastructure/"]
RUN dotnet restore "src/NexaERP.API/NexaERP.API.csproj"
COPY . .
WORKDIR "/src/src/NexaERP.API"
RUN dotnet build "NexaERP.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NexaERP.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NexaERP.API.dll"]
