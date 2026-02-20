# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Services.Inventario.csproj", "Services.Inventario/"]
COPY ["./Shared.Core/Shared.Core.csproj", "Shared.Core/"]
RUN dotnet restore "Services.Inventario/Services.Inventario.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Services.Inventario"
RUN dotnet build "Services.Inventario.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Services.Inventario.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Services.Inventario.dll"]
