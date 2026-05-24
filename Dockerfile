# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
ARG BUILD_CONFIGURATION=Release

COPY ["EXE02_Backend_RE-CAFE.csproj", "./"]
RUN dotnet restore "EXE02_Backend_RE-CAFE.csproj"

COPY . .
RUN dotnet publish "EXE02_Backend_RE-CAFE.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_EnableDiagnostics=0

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EXE02_Backend_RE-CAFE.dll"]
