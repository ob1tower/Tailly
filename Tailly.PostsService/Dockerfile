FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Tailly.PostsService/Tailly.PostsService.csproj", "Tailly.PostsService/"]
RUN dotnet restore "./Tailly.PostsService/Tailly.PostsService.csproj"

COPY . .
WORKDIR "/src/Tailly.PostsService"
RUN dotnet build "./Tailly.PostsService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tailly.PostsService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=build /src/Tailly.PostsService/wwwroot ./wwwroot
ENTRYPOINT ["dotnet", "Tailly.PostsService.dll"]