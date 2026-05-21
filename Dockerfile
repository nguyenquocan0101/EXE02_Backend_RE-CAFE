# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["EXE02_Backend_RE-CAFE.csproj", "./"]
RUN dotnet restore "EXE02_Backend_RE-CAFE.csproj"

COPY . .
RUN dotnet publish "EXE02_Backend_RE-CAFE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EXE02_Backend_RE-CAFE.dll"]
