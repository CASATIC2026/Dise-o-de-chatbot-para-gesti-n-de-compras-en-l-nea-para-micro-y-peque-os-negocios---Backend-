FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services.ChatBot/Services.ChatBot.csproj", "Services.ChatBot/"]

RUN dotnet restore "Services.ChatBot/Services.ChatBot.csproj"

COPY src/ .
WORKDIR "/src/Services.ChatBot"
RUN dotnet build "Service.ChatBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Services.ChatBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Services.ChatBot.dll"]