FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos los archivos de proyecto usando la nueva ruta relativa al contexto (raíz)
COPY ["src/Services.Inventario/Services.Inventario.csproj", "Services.Inventario/"]
COPY ["src/Shared.Core/Shared.Core.csproj", "Shared.Core/"]

# Restauramos dependencias
RUN dotnet restore "Services.Inventario/Services.Inventario.csproj"

# Copiamos todo el contenido de la carpeta src (donde está Shared.Core y los servicios)
COPY src/ .

# Nos movemos a la carpeta del servicio para compilar
WORKDIR "/src/Services.Inventario"
RUN dotnet build "Services.Inventario.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Services.Inventario.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Services.Inventario.dll"]