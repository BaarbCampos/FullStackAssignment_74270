FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["SportsStore.OrderApi/SportsStore.OrderApi.csproj", "SportsStore.OrderApi/"]
COPY ["SportsStore.Shared/SportsStore.Shared.csproj", "SportsStore.Shared/"]

RUN dotnet restore "SportsStore.OrderApi/SportsStore.OrderApi.csproj"

COPY . .
WORKDIR "/src/SportsStore.OrderApi"
RUN dotnet publish "SportsStore.OrderApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080

RUN apt-get update && apt-get install -y netcat-openbsd && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish/ ./