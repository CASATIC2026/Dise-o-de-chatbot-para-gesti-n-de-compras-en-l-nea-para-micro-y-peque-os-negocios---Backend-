# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ORIGEN (Raíz del proyecto) -> DESTINO (Dentro del contenedor en /src/)
COPY ["src/Services.Pagos/Services.Pagos.csproj", "Services.Pagos/"]
COPY ["src/Shared.Core/Shared.Core.csproj", "Shared.Core/"]

# Ejecutamos el restore apuntando al DESTINO (donde acabamos de copiarlo)
RUN dotnet restore "Services.Pagos/Services.Pagos.csproj"

# Copiamos todo lo demás
COPY src/ .

WORKDIR "/src/Services.Pagos"
RUN dotnet build "Services.Pagos.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Services.Pagos.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Services.Pagos.dll"]