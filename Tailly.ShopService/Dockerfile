FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Tailly.ShopService/Tailly.ShopService.csproj", "Tailly.ShopService/"]
RUN dotnet restore "./Tailly.ShopService/Tailly.ShopService.csproj"

COPY . .
WORKDIR "/src/Tailly.ShopService"
RUN dotnet build "./Tailly.ShopService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Tailly.ShopService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Tailly.ShopService.dll"]