FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BirFikrimVar.API/BirFikrimVar.API.csproj", "BirFikrimVar.API/"]
COPY ["BirFikrimVar.Core/BirFikrimVar.Core.csproj", "BirFikrimVar.Core/"]
COPY ["BirFikrimVar.Data/BirFikrimVar.Data.csproj", "BirFikrimVar.Data/"]
COPY ["BirFikrimVar.Service/BirFikrimVar.Service.csproj", "BirFikrimVar.Service/"]

RUN dotnet restore "BirFikrimVar.API/BirFikrimVar.API.csproj"

COPY . .

WORKDIR "/src/BirFikrimVar.API"

RUN dotnet publish "BirFikrimVar.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BirFikrimVar.API.dll"]
