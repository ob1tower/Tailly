FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Tailly.SpecialistService/Tailly.SpecialistService.csproj", "Tailly.SpecialistService/"]
RUN dotnet restore "./Tailly.SpecialistService/Tailly.SpecialistService.csproj"

COPY . .

WORKDIR "/src/Tailly.SpecialistService"
RUN dotnet build "Tailly.SpecialistService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Tailly.SpecialistService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tailly.SpecialistService.dll"]