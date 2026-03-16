FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["src/Services.Inventario/Services.Inventario.csproj", "Services.Inventario/"]
COPY ["src/Shared.Core/Shared.Core.csproj", "Shared.Core/"]

RUN dotnet restore "Services.Inventario/Services.Inventario.csproj"

COPY src/ .
WORKDIR "/src/Services.Inventario"
RUN dotnet build "Services.Inventario.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Services.Inventario.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Services.Inventario.dll"]